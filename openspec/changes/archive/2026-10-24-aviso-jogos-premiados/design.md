## Context

A motivação está em `proposal.md` e o comportamento, em `specs/aviso-jogos-premiados/spec.md`. Estado atual
relevante:

- As páginas Resultados e Painel chamam `AtualizacaoConcursosServico.AtualizarAsync`, que devolve
  `ResultadoAtualizacao` (`UltimoNumeroAntes`, `ConcursosGravados`, `Falha`, `PrimeiroGravado`,
  `UltimoNumeroNaBase`). Cada página mostra o desfecho por `AvisosAtualizacao.Desfecho` num snackbar e recarrega os
  dados quando algo foi gravado. A lógica do desfecho está duplicada nas duas páginas, com o texto centralizado em
  `AvisosAtualizacao`.
- `JogosServico.ListarAsync` agrupa todos os jogos por concurso (`GrupoJogos`) e confere os de concursos gravados
  (`JogoConferido.Numero` é a posição no concurso, `ConferenciaJogo.Premio`/`Acertos`). `GrupoJogos` já tem
  `Prescricao` e `DentroDoPrazo(hoje)`.
- Meus jogos usa `IDialogService` (MudBlazor) para os diálogos de cadastro e conferência. A rota de Meus jogos é
  `/meus-jogos`.

## Goals / Non-Goals

**Goals:**
- Conferência dos concursos novos na Aplicação, reaproveitando o agrupamento e a conferência de `ListarAsync`.
- Uma única implementação do aviso (diálogo ou snackbar) usada pelas duas páginas.
- Textos em classe estática testável, como nas demais páginas.

**Non-Goals:**
- Guardar quais prêmios já foram avisados ou avisar prêmios trazidos pela importação do histórico.
- Mudar os desfechos atuais da atualização.

## Decisions

### Aplicação (`Lotofacil.Aplicacao/Jogos`)
- **`JogosServico.ConferirConcursosAsync(primeiro, ultimo)`**: devolve uma `ConferenciaConcursos` com os grupos dos
  concursos de `primeiro` a `ultimo`, em ordem crescente de número, já conferidos. O agrupamento e a conferência de
  `ListarAsync` vão para um método privado comum, que recebe os jogos a agrupar; `ListarAsync` continua devolvendo
  todos, do maior para o menor.
  - Alternativa descartada: nova consulta no repositório filtrando por intervalo. A base é local e pequena, e
    `IJogoRepositorio.ListarAsync` já existe; filtrar em memória evita mexer na Infraestrutura.
- **`ConferenciaConcursos`** (record): `Grupos` e as contas usadas pelo aviso: `JogosConferidos`, `JogosPremiados`,
  `PremioTotal` e `GruposPremiados` (só os grupos com jogo premiado, cada um só com os jogos premiados, mantendo o
  número do jogo no concurso).
- A página passa `resultado.PrimeiroGravado` e `resultado.UltimoNumeroNaBase` quando `ConcursosGravados > 0`, o que
  também cobre a atualização interrompida depois de gravar.

### Web
- **`DialogoJogosPremiados`** (`Components/Jogos`): recebe `ConferenciaConcursos` e `Hoje`, mostra o resumo, os
  concursos premiados com data e prazo, os jogos e o total. "Ver em Meus jogos" fecha o diálogo e navega para
  `/meus-jogos` pelo `NavigationManager`; "Fechar" só fecha.
- **`AvisoJogosPremiados`** (estática, `Components/Jogos`): `MostrarAsync(dialogos, mensagens, conferencia, hoje)`
  decide entre nada (sem jogos), snackbar informativo (nenhum premiado) e o diálogo (algum premiado). As duas
  páginas chamam esse método depois do desfecho e da recarga, com o `JogosServico` injetado.
  - Alternativa descartada: componente invisível colocado nas duas páginas. A decisão não depende de estado da
    página, e um método estático com os serviços passados é mais simples de testar e de ler.
- **`FormatacaoPremios`** (estática): resumo ("3 de 5 jogos conferidos foram premiados." e singulares), cabeçalho do
  concurso, prazo ("Receber até …" / "Prazo encerrado em …"), linha do jogo e total, além da mensagem de nenhum
  prêmio. Reaproveita `FormatacaoJogo.NumeroJogo`, `QuantidadeDezenas` e `Reais`.
- "Hoje" vem de `DateOnly.FromDateTime(DateTime.Today)`, como no Painel.

## Risks / Trade-offs

- [Erro na conferência depois de uma atualização bem-sucedida] → a exceção cai no tratamento já existente das
  páginas (log e mensagem de falha inesperada); os concursos já estão gravados, e Meus jogos continua conferindo na
  leitura.
- [Diálogo aberto logo após o snackbar do desfecho] → os dois ficam visíveis: o snackbar no canto e o diálogo no
  centro, sem um esconder o outro.
- [Muitos concursos gravados de uma vez, por exemplo após semanas sem atualizar] → o diálogo lista só os concursos
  com prêmio e rola quando necessário (`MudDialog` com conteúdo rolável).

## Migration Plan

Sem migration nem dado gravado. As páginas só ganham o passo do aviso depois da atualização.
