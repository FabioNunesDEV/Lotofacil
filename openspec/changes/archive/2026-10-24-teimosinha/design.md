## Context

A motivação está em `proposal.md` e o comportamento, em `specs/teimosinha/spec.md`. Estado atual relevante:

- `Jogo` (domínio) valida as dezenas no construtor e é gravado na tabela `Jogo`, com índice único
  (NumeroConcurso, Dezenas).
- O cadastro passa por `JogosServico`:
  - `CadastrarAsync` (um jogo) e `CadastrarVariosAsync` (vários; recusa duplicados e grava os demais) verificam
    antes o concurso (base vazia, concurso já sorteado) em `VerificarConcursoAsync`;
  - `GeracaoJogosServico.SalvarAsync` delega a `CadastrarVariosAsync`;
  - `ListarAsync` monta os `GrupoJogos` com `JogoConferido` para Meus jogos.
- Resultados e Painel atualizam pelo `AtualizacaoConcursosServico` e, com concursos gravados, recarregam e chamam
  `AvisoJogosPremiados`. A importação só é feita em Resultados. O `Program.cs` migra a base e carrega as
  configurações na inicialização.
- O menu tem Painel, Resultados, Estatísticas, Gerar jogos, Meus jogos e Configurações.

## Goals / Non-Goals

**Goals:**
- Reaproveitar o jogo comum: a Teimosinha só cria jogos, e conferência, aviso, exportação e exclusão continuam os
  mesmos.
- Criação idempotente, que pode rodar várias vezes (inicialização, cada atualização) sem duplicar.

**Non-Goals:**
- Teimosinha com número fixo de concursos (como na Caixa), pausar sem remover, editar ou renomear a Teimosinha,
  remover um jogo só da Teimosinha (remove-se a Teimosinha inteira e cria de novo), listar as removidas, Bolão.

## Decisions

- **Domínio:**
  - `Teimosinha` (`Lotofacil.Dominio.Teimosinhas`) tem `Id`, `Nome`, `Ativa`, `UltimoConcursoCriado` e a coleção
    `Jogos` de `JogoTeimosinha` (`ConcursoInicial` e `Dezenas`). Métodos:
    - `Acrescentar(dezenas, concursoInicial)` recusa dezenas repetidas na Teimosinha;
    - `JogosAte(concurso)` devolve os jogos com início até o concurso;
    - `RegistrarCriacao(concurso)`;
    - `Remover()` torna a Teimosinha inativa.
  - O nome é aparado e validado (1 a 40 caracteres). `NomeIgual(nome)` compara sem diferenciar maiúsculas.
  - A validação das dezenas do `Jogo` vira o método estático `Jogo.OrdenarEValidar(dezenas)`, usado pelo `Jogo` e
    pelo `JogoTeimosinha`, para manter uma única regra de 15 a 20 dezenas.
  - `Jogo` ganha `TeimosinhaId` (`int?`), opcional no construtor.
  - `ITeimosinhaRepositorio` tem `ListarAtivasAsync`, `ObterAsync(id)`, `ListarNomesAsync` (todas, inclusive
    removidas, para a marca) e `SalvarAsync` (insere ou atualiza com os jogos). A busca pelo nome é feita entre as
    ativas com `NomeIgual`, em memória, porque são poucas; não há método próprio.
  - `JogoTeimosinha` tem `Id` como propriedade, não de sombra: o repositório atualiza Teimosinhas lidas sem
    rastreamento, e o EF perde a chave de sombra nesse caso.
- **Infraestrutura:**
  - Tabela `Teimosinha` e tabela `TeimosinhaJogo`, com os jogos como coleção própria da Teimosinha (`OwnsMany`,
    chave `TeimosinhaId` + `Id`, dezenas em JSON como no `Jogo`).
  - Coluna `TeimosinhaId` nula em `Jogo`, com chave estrangeira sem exclusão em cascata. A Teimosinha nunca é apagada,
    só fica inativa, e é isso que mantém o nome nos jogos antigos.
  - Migration `CriacaoTeimosinha`.
- **Aplicação — `TeimosinhaServico`** (`Lotofacil.Aplicacao.Teimosinhas`):
  - `SalvarAsync(nome, concurso, jogos)` é usado pelo cadastro manual (um jogo) e por Gerar jogos (vários):
    1. Valida o nome e o concurso com a mesma verificação do cadastro, que passa a ser pública em `JogosServico`
       (`VerificarConcursoAsync`).
    2. Busca a ativa com o nome ou cria uma nova, com `UltimoConcursoCriado` igual ao concurso.
    3. Separa os jogos já existentes na Teimosinha e os duplicados no concurso. Se nenhum sobrar, devolve a recusa
       sem gravar a Teimosinha.
    4. Grava a Teimosinha com os jogos novos (início = concurso) e grava os jogos comuns do concurso com o
       `TeimosinhaId` direto pelo `IJogoRepositorio`. `CadastrarVariosAsync` não muda, para não quebrar a assinatura
       usada por Gerar jogos e pelos testes.
    - Devolve `ResultadoSalvarTeimosinha` (gravados, recusados, motivo e o nome como gravado), para as mensagens.
      Com um jogo só, o motivo é o específico ("já está cadastrado para o concurso", "já faz parte da Teimosinha").
  - `CriarJogosAsync()`: para cada ativa, percorre de `UltimoConcursoCriado + 1` até o próximo concurso, cria os
    jogos de `JogosAte(concurso)` que ainda não existem (`IJogoRepositorio.ExisteAsync`) e registra o último
    concurso. Devolve `JogosCriadosTeimosinha(Quantidade, PrimeiroConcurso, UltimoConcurso)`. Não aplica a regra de
    concurso futuro, como a spec pede.
  - `ListarAtivasAsync()`, `RemoverAsync(id)` e `ListarNomesAsync()` (dicionário id → nome).
  - Alternativa descartada: conferir a Teimosinha sem criar jogos. O usuário escolheu criar jogos, que reaproveita
    tudo o que já existe.
  - Alternativa descartada: guardar o nome no `Jogo`. Duplicaria o dado e não identificaria a Teimosinha.
- **Quando criar:**
  - **Inicialização:** o `Program.cs` chama `CriarJogosAsync` depois de carregar as configurações.
  - **Resultados e Painel:** com concursos gravados pela atualização, a página chama `CriarJogosAsync` antes de
    recarregar a lista e de `AvisarJogosPremiadosAsync`. Assim os jogos criados para concursos já sorteados entram
    na conferência do aviso.
  - **Importação:** Resultados também chama depois de uma importação que gravou concursos, mas sem mensagem.
- **Web:**
  - `CampoTeimosinha` (`Components/Teimosinhas`): componente com o switch "Teimosinha (repetir em todo concurso)" e
    um `MudTextField` "Nome da Teimosinha" (`Immediate`). As ativas são sugeridas por um `datalist` nativo, ligado
    ao campo pelo atributo `list`. É usado no `CadastroJogo` e em Gerar jogos, junto de "Salvar jogos". Com o switch
    ligado e o nome vazio, o salvar fica desabilitado (`CampoTeimosinha.Completo`).
    - Alternativa descartada: `MudAutocomplete` com `CoerceValue`. Ele só confirma o texto digitado ao sair do campo,
      e o "Salvar" ainda estaria desabilitado no clique que tira o foco.
  - `CadastroJogo` e `GerarJogos` chamam `TeimosinhaServico.SalvarAsync` quando o switch está ligado. Desligado,
    nada muda. As mensagens ficam em `FormatacaoTeimosinha`.
  - Página `Teimosinhas.razor(.cs)` em `/teimosinhas`:
    - um cartão por ativa, com o nome, "Desde o concurso N", a linha "<k> jogos · <custo> por concurso" (preço de
      `ConfiguracoesServico`) e os jogos (`BolasJogo`);
    - "Remover" pede confirmação com `ShowMessageBoxAsync` e mostra "Teimosinha removida.";
    - item no menu (`Icons.Material.Filled.Repeat`) entre "Meus jogos" e "Configurações".
  - Meus jogos carrega `ListarNomesAsync` junto da lista e mostra o chip "Teimosinha · <nome>" (`marca-teimosinha`)
    nas linhas com `TeimosinhaId`, abaixo das bolas. Na coluna da direita, o chip apertaria as bolas em telas
    estreitas.
  - A criação depois de atualizar fica em `AvisoTeimosinhas.CriarJogosAsync`, usado por Resultados e Painel. Depois
    da importação, Resultados chama `CriarJogosAsync` direto, sem aviso.
  - `FormatacaoTeimosinha` monta "Teimosinhas: 2 jogos criados para o concurso 3814." e a variação "para os
    concursos 3814 a 3816.", a linha de custo, o "Desde o concurso" e as mensagens de salvamento.
- **Testes:**
  - domínio: regras da Teimosinha, `JogosAte`, nome, dezenas repetidas;
  - integração: repositório (salvar com jogos, ativas, nomes, `Jogo.TeimosinhaId`) e migration numa base existente;
  - serviço com substitutos: salvar novo, acrescentar, recusas, criação com pulados, duplicado, início posterior e
    removida;
  - bUnit: campo no cadastro e em Gerar jogos, página Teimosinhas, remover, marca em Meus jogos, mensagem depois de
    atualizar em Resultados e Painel e ordem antes do aviso.

## Risks / Trade-offs

- [Jogos criados para concursos já sorteados (pulados)] → decisão da spec, para a Teimosinha valer em todo concurso.
  Um jogo criado à toa pode ser excluído em Meus jogos.
- [Remover não apaga o jogo já criado para o próximo concurso, ainda não sorteado] → decisão do usuário (manter os
  já criados). O usuário pode excluí-lo em Meus jogos.
- [Criação na inicialização com muitas Teimosinhas] → poucas por usuário e poucos concursos por vez: custo
  desprezível.

## Migration Plan

Migration `CriacaoTeimosinha` aplicada na inicialização: cria as duas tabelas e a coluna `TeimosinhaId` (nula) em
`Jogo`. Os jogos atuais ficam sem Teimosinha. Rollback: remover a migration.
