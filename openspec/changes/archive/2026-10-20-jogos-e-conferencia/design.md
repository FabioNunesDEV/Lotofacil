## Context

Motivação em `proposal.md`; comportamento em `specs/jogos/spec.md`. Estado atual relevante:

- `Concurso` (domínio) com dezenas em coluna JSON ordenada e rateio em tabela própria; `ObterRateio(faixa)` devolve o
  rateio de uma faixa. O repositório usa `IDbContextFactory` (um contexto por operação) e é singleton.
- A base é criada/atualizada por migrations na inicialização (`dotnet-ef` como ferramenta local, pasta
  `Migracoes`); há um teste que falha se o modelo mudar sem migration.
- A Web usa MudBlazor, `ISnackbar` para mensagens, CSS isolado por componente e o componente `Dezenas` para bolas.
- O protótipo (`prototipo/`, página "Meus jogos" e diálogo de conferência) define layout, textos e cores.
- Ainda não há configuração de preço da aposta.

## Goals / Non-Goals

**Goals:**
- Regras do jogo e da conferência no domínio, testáveis sem banco.
- Conferência calculada na leitura (jogo + concurso gravado), sem estado a sincronizar.
- Cadastro que a geração de jogos possa reutilizar depois (mesmo serviço, mesmas regras).

**Non-Goals:**
- Apostas de 16 a 20 dezenas, Teimosinha, Bolão, edição de jogo.
- Guardar o resultado da conferência no banco ou notificar prêmios na atualização.
- Paginação da lista de jogos (uso pessoal: dezenas de jogos, não milhares).

## Decisions

### Domínio (`Lotofacil.Dominio/Jogos`)
- `Jogo`: `Id` (gerado pelo banco), `NumeroConcurso`, `Dezenas` (15, distintas, 1–25, ordenadas), `CadastradoEm`
  (`DateTime`). Construtor valida as regras, como `Concurso`, lançando `ArgumentException` com mensagem em
  português; construtor privado para o EF Core.
- `ConferenciaJogo` (record): `Acertos` e `Premio` (`decimal`, zero sem prêmio), criado por
  `ConferenciaJogo.Conferir(Jogo jogo, Concurso concurso)`; lança se o número do concurso não for o do jogo.
  Prêmio = `concurso.ObterRateio(acertos).ValorPremio` para 11 a 15 acertos. Usa sempre o rateio gravado.
- `RegrasLotofacil.DiasPrescricao = 90` e `RegrasLotofacil.DezenasPorJogo = 15` (a aposta mínima; 16–20 ficam
  para depois e mudarão esta regra).
- `IJogoRepositorio`: `AdicionarAsync(Jogo)`, `ListarAsync()` (ordem de cadastro), `ExisteAsync(numeroConcurso,
  dezenas)`, `ExcluirAsync(id)`.

### Aplicação (`Lotofacil.Aplicacao/Jogos`)
`JogosServico`, com:
- `CadastrarAsync(int numeroConcurso, IReadOnlyCollection<int> dezenas)` → `ResultadoCadastro(bool Sucesso,
  string Mensagem)`. Ordem das verificações: base vazia → concurso não futuro → duplicado → grava. As mensagens da
  spec são montadas aqui (regra de negócio, não apresentação). Dezenas inválidas são erro de programação da tela
  (o volante não permite) e continuam como `ArgumentException`.
- `ObterProximoConcursoAsync()` → `int?` (último gravado + 1; `null` com a base vazia), para a sugestão.
- `ListarAsync()` → `IReadOnlyList<GrupoJogos>`: agrupa por concurso (maior primeiro), busca cada concurso
  sorteado no repositório de concursos (`ObterAsync`, um por grupo) e confere cada jogo. `GrupoJogos(int
  NumeroConcurso, Concurso? Concurso, IReadOnlyList<JogoConferido> Jogos)` com `PremioTotal`, `Premiado` e
  `Prescricao` derivados; `JogoConferido(Jogo Jogo, int Numero, ConferenciaJogo? Conferencia)`.
- `ExcluirAsync(int id)`.
- O resumo dos indicadores (jogos, concursos, premiados, total em prêmios) é derivado dos grupos na própria página
  a partir dessas propriedades; o valor investido usa o preço da configuração.

Alternativa descartada: gravar acertos e prêmio no banco quando a atualização roda. Exigiria acoplar a atualização
aos jogos e reprocessar em correções; calcular na leitura custa uma consulta por concurso com jogos.

### Infraestrutura
- `JogoConfiguracao`: tabela `Jogo`, `Id` autoincremento, `Dezenas` como `PrimitiveCollection` (JSON, já
  ordenada), índice **único** em (`NumeroConcurso`, `Dezenas`) como garantia no banco além da verificação no
  serviço. Sem chave estrangeira para `Concurso` (o concurso ainda não existe quando o jogo é cadastrado).
- `JogoRepositorio` no mesmo padrão do `ConcursoRepositorio`; `ExisteAsync` compara a lista ordenada.
- Migration `CriacaoJogos` gerada com `dotnet tool run dotnet-ef migrations add CriacaoJogos --project
  src/Lotofacil.Infraestrutura --output-dir Migracoes`.
- Registro do repositório em `AddInfraestrutura`.

### Configuração
`appsettings.json`: `"Lotofacil": { "PrecoApostaMinima": 3.50 }`, lido no `Program.cs` (padrão 3,50) e
registrado como `OpcoesApostas(decimal PrecoApostaMinima)` na Web (só a página usa o preço).

### Web
- `Components/Pages/MeusJogos.razor(.cs)`: cabeçalho com "Novo jogo" e filtros (`MudChipSet`), quatro indicadores
  (`MudPaper`), grupos (`MudExpansionPanels` abertos) com cabeçalho, resultado oficial e linhas de jogo; ações de
  excluir com `IDialogService.ShowMessageBoxAsync` (sim/não); mensagens com `ISnackbar`.
- `Components/Jogos/CadastroJogo.razor(.cs)`: `MudDialog` com o campo do concurso, o `Volante` e a contagem.
- `Components/Jogos/Volante.razor(.cs)`: grade 5 × 5 de bolas clicáveis (modo de marcação, limite de 15) ou
  somente leitura com a classe de situação de cada dezena (acerto, erro, não marcada) — usado no cadastro e na
  conferência.
- `Components/Jogos/ConferenciaJogo.razor(.cs)`: `MudDialog` com o volante, a legenda e a lista de detalhes.
- `FormatacaoJogo`: "1 jogo"/"N jogos", número do jogo "01", acertos, prêmio ou "—", datas.
- Menu: `MudNavLink` "Meus jogos" (ícone `ConfirmationNumber`) abaixo de "Estatísticas".
- Após cadastrar ou excluir, a página recarrega a lista do serviço.

### Testes
- **Unidade (domínio)**: regras do `Jogo`; `ConferenciaJogo` com rateios diferentes (inclusive valor antigo de
  faixa fixa, para provar que usa o rateio gravado), menos de 11 acertos, concurso errado.
- **Unidade (aplicação)**: cadastro (sucesso e cada recusa, com a mensagem), próximo concurso, agrupamento e
  ordem, conferência por grupo, prêmio total, prescrição, exclusão.
- **Unidade (bUnit)**: menu, lista vazia, cadastro (sugestão, contagem, limite de 15, botão, mensagens), grupos,
  linhas, filtros, indicadores, janela de conferência, exclusão com confirmação e cancelamento.
- **Integração**: `JogoRepositorio` com SQLite em memória (gravar, listar na ordem, existe, excluir, índice único);
  o teste de migrations em dia passa a cobrir a tabela nova.

## Risks / Trade-offs

- [Concurso digitado muito à frente (ex.: 38090)] → O jogo fica aguardando; o usuário pode excluir. Sem limite
  superior para não impedir quem joga com antecedência.
- [Correção de rateio pela Caixa depois de gravado] → A conferência usa o que está gravado; reimportar não altera
  concursos existentes (comportamento das fatias anteriores). Fica registrado como limitação conhecida.
- [Muitos concursos com jogos] → Uma consulta por concurso com jogos ao listar; aceitável para uso pessoal.
- [Mudança futura para 16–20 dezenas] → `DezenasPorJogo` e a conferência ficam isolados no domínio; a mudança
  amplia o `Jogo` e a conferência sem afetar a página de forma estrutural.

## Migration Plan

Nova migration `CriacaoJogos`, aplicada automaticamente na inicialização, sem tocar nas tabelas existentes. Nova
chave de configuração com padrão no código. Para desfazer, reverter o código e remover a tabela `Jogo` (ou a base
de teste).
