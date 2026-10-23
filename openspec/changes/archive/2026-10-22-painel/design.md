## Context

Motivação em `proposal.md`; comportamento em `specs/painel/spec.md`, `specs/jogos/spec.md` e
`specs/aplicacao-web/spec.md`. Estado atual relevante:

- `Painel.razor` (rota `/`) só tem o título. Todas as páginas rodam no modo `InteractiveServer` global.
- Os dados do Painel já existem em serviços da Aplicação:
  - `IConcursoRepositorio`: `ContarAsync`, `ObterUltimoNumeroAsync`, `ObterAsync`;
  - `EstatisticasServico.CalcularAsync(null)`: `EstatisticasDezenas` com `MaisSorteadas` e `MaisAtrasadas`
    (todo o histórico), ou `null` com a base vazia;
  - `JogosServico.ListarAsync`: `GrupoJogos` por concurso, já conferidos pelo rateio gravado, com `PremioTotal` e
    `Prescricao` (data do sorteio + 90 dias).
- A página `Resultados` faz a atualização com `AtualizacaoConcursosServico` e monta, no próprio code-behind, os
  textos do andamento e do desfecho (`MostrarAndamento`, `InformarAtualizacao`).
- Componentes reaproveitáveis: `Dezenas`, `DetalheConcurso` (rateio, arrecadação e ordem do sorteio),
  `RankingDezenas` (mesmo visual e textos das Estatísticas) e `BolasJogo` (dezenas do jogo com as acertadas em
  destaque).
- `MeusJogos.NovoJogoAsync` abre o diálogo `CadastroJogo` com o concurso sugerido e recarrega a lista se salvou.

## Goals / Non-Goals

**Goals:**
- Painel montado só com serviços e componentes existentes, mais um serviço de resumo pequeno.
- Regras do resumo (jogos aguardando, prêmios dentro do prazo) fora do componente, testáveis sem tela.
- Mensagens da atualização idênticas nas duas páginas, escritas num lugar só.

**Non-Goals:**
- Cache ou consultas novas no repositório: o Painel recalcula tudo ao abrir, como as páginas Estatísticas e Meus
  jogos.
- Mudar a página Resultados além da extração dos textos da atualização.

## Decisions

### Aplicação: `PainelServico` e `ResumoPainel`
- Novo `PainelServico` (`Lotofacil.Aplicacao/Painel`) com `ObterResumoAsync(DateOnly hoje, CancellationToken)`,
  que devolve `ResumoPainel?` (`null` com a base vazia). Depende de `IConcursoRepositorio`, `EstatisticasServico`
  e `JogosServico`, reaproveitando a conferência e o cálculo das estatísticas sem duplicá-los.
- `ResumoPainel` (record) com: `TotalConcursos`, `UltimoConcurso` (`Concurso`), `Estatisticas`
  (`EstatisticasDezenas`), `JogosUltimoConcurso` (`GrupoJogos?`, o grupo do último concurso gravado),
  `JogosAguardando` (quantidade de jogos de grupos não conferidos), `ConcursosAguardando` (números desses grupos,
  do menor para o maior) e `PremiosAReceber`.
- `GrupoJogos` ganha `DentroDoPrazo(DateOnly hoje)`: conferido e `hoje <= Prescricao`. `PremiosAReceber` soma o
  `PremioTotal` dos grupos dentro do prazo. A regra fica no grupo porque é ele que já conhece a prescrição.
- "Hoje" chega por parâmetro; a página passa `DateOnly.FromDateTime(DateTime.Today)`.
  - Alternativa: `TimeProvider` injetado. Descartada: o parâmetro é mais simples e os testes passam a data
    diretamente, sem relógio falso.
- Alternativa ao serviço: o componente chamar os três serviços e somar. Descartada: a regra do prazo e a contagem
  dos aguardando são regras de negócio, que o `CLAUDE.md` proíbe no componente.

### Web: textos da atualização compartilhados
- Nova classe estática `AvisosAtualizacao` (`Components/Concursos`) com:
  - `Consultando` ("Consultando o último concurso da Caixa…") e `Andamento(AndamentoAtualizacao)`;
  - `Desfecho(ResultadoAtualizacao)`, que devolve `AvisoAtualizacao(string Texto, Severity Severidade)` com as
    mensagens de hoje (base vazia, já atualizada, 1 ou n gravados, interrupção com ou sem gravados);
  - `FalhaInesperada`, o texto da falha não prevista.
- `Resultados` e `Painel` usam essas funções; cada página continua com seu `try/catch`, o registro no log e o
  `Snackbar` (falha com `RequireInteraction`), porque o andamento e o bloqueio dos botões são de cada página.
- Os testes bUnit atuais da página Resultados garantem que as mensagens não mudaram.
- Alternativa: um componente "botão de atualizar" com andamento e mensagens embutidos. Descartada: na página
  Resultados o andamento é compartilhado com a importação e os dois botões se bloqueiam mutuamente; o componente
  precisaria de parâmetros de ida e volta só para isso.

### Web: página Painel
- `Painel.razor` + `Painel.razor.cs` + `Painel.razor.css`, no padrão das outras páginas: cabeçalho com subtítulo e
  botões; barra de andamento durante a atualização; até carregar, nada além do cabeçalho; depois, a mensagem de base
  vazia ou o conteúdo.
- Indicadores em `MudPaper` (mesmo visual dos de Meus jogos); cartões "Último resultado" (`Dezenas` +
  `DetalheConcurso`), "Destaques" (dois `RankingDezenas`, `PorAtraso` no segundo, link para `/estatisticas`) e
  "Seus jogos no concurso N" (linhas com `BolasJogo`, acertos e prêmio, link para `/meus-jogos`).
- As linhas de jogo do Painel são só leitura (sem exclusão nem clique de conferência), por isso usam marcação
  própria em vez de extrair a linha de Meus jogos.
- Textos novos em `FormatacaoPainel` (`Components/Painel`), testáveis isoladamente: "Último sorteio em <data>",
  "Estimativa <valor>"/"Estimativa não informada", detalhe dos jogos aguardando, ganhadores de 15 acertos ("1
  ganhador", "<n> ganhadores", "Acumulou"). A data com dia da semana vai para `FormatacaoConcurso.DataComDiaSemana`
  (`"dddd, dd/MM/yyyy"` na cultura pt-BR, já configurada na aplicação).
- Após uma atualização que gravou concursos, o Painel chama de novo `ObterResumoAsync`.

### Web: atalho "Novo jogo"
- O Painel navega para `meus-jogos?novo=true`.
- `MeusJogos` recebe `[SupplyParameterFromQuery(Name = "novo")] bool? Novo`. Em `OnAfterRenderAsync(firstRender)`,
  se `Novo == true`, troca o endereço para `meus-jogos` com `replace: true` e chama o `NovoJogoAsync` de hoje.
  - Abrir no primeiro render (e não em `OnInitializedAsync`) evita esperar o resultado do diálogo durante a
    inicialização, o que travaria o primeiro desenho da página, e garante a lista já carregada atrás do diálogo.
  - `replace: true` impede que recarregar ou voltar no navegador reabra o cadastro.
- Alternativa: abrir o `CadastroJogo` no próprio Painel. Descartada pela decisão do usuário: o atalho leva a Meus
  jogos, onde o jogo salvo aparece na lista.

### Injeção de dependência
- `PainelServico` registrado como singleton em `Program.cs`, como os demais serviços.

## Risks / Trade-offs

- [O Painel recalcula estatísticas e confere todos os jogos a cada abertura] → mesmo custo das páginas
  Estatísticas e Meus jogos, já verificado com o histórico real (~3.800 concursos); só otimizar se ficar lento.
- [`JogosServico.ListarAsync` busca o concurso de cada grupo] → quantidade de grupos é pequena (um por concurso
  apostado); aceitável localmente.
- ["Prêmios a receber" não sabe se o prêmio já foi recebido] → o indicador mostra o que ainda está no prazo; marcar
  prêmio como recebido está fora do escopo (proposal).
- [`NavigateTo` com `replace` dentro de `OnAfterRenderAsync` re-renderiza a página] → o parâmetro `Novo` passa a
  `null` e o cadastro não reabre; coberto por teste bUnit.
