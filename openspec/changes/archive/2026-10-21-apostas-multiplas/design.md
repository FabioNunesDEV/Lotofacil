## Context

Motivação em `proposal.md`; comportamento em `specs/jogos/spec.md`. Estado atual relevante:

- `Jogo` (domínio) valida exatamente `RegrasLotofacil.DezenasPorJogo = 15` dezenas; as dezenas são gravadas como
  coleção primitiva (texto JSON ordenado) com índice único `(NumeroConcurso, Dezenas)`.
- `ConferenciaJogo(Acertos, Premio)` paga um único prêmio: `concurso.ObterRateio(acertos).ValorPremio`.
- `GrupoJogos` soma os prêmios; a página `MeusJogos` calcula custo e investido como `quantidade de jogos × preço`
  (`OpcoesApostas.PrecoApostaMinima`, registrado na Web).
- O `Volante` já recebe `Limite` e bloqueia novas marcações quando `marcadas >= Limite`, permitindo desmarcar — isso
  já cobre o caso de reduzir "Dezenas por jogo" com dezenas a mais marcadas.
- `BolasJogo` usa `flex-wrap`, então 20 bolas quebram linha sem ajuste de layout.

## Goals / Non-Goals

**Goals:**
- Regra da aposta múltipla (apostas equivalentes e prêmios por faixa) no domínio, testável sem banco e sem tela.
- Nenhuma mudança de esquema no banco.
- Cadastro de 15 a 20 dezenas pelo mesmo `JogosServico.CadastrarAsync`, que a geração vai reutilizar.

**Non-Goals:**
- Guardar quantidade de apostas, custo ou prêmios no banco (tudo continua calculado na leitura).
- Mover o preço da aposta para outra camada.

## Decisions

### Domínio
- `RegrasLotofacil`: `DezenasPorJogo` é substituída por `MinimoDezenasPorJogo = 15` e `MaximoDezenasPorJogo = 20`.
  Quem hoje usa 15 como "dezenas sorteadas" passa a usar `DezenasSorteadas` (já existe).
- Nova classe estática `Combinatoria` com `Combinacoes(int n, int k)` (fórmula multiplicativa, retorna 0 quando
  `k < 0` ou `k > n`). Os valores cabem em `int` (máximo C(20, 15) = 15.504).
  - Alternativa: tabela fixa de apostas por quantidade de dezenas. Descartada: a conferência precisa de C(a, k) e
    C(n − a, 15 − k) para vários valores; a função única serve aos dois usos.
- `Jogo` aceita de `MinimoDezenasPorJogo` a `MaximoDezenasPorJogo` dezenas e ganha `QuantidadeApostas` =
  `Combinacoes(Dezenas.Count, DezenasSorteadas)`, propriedade calculada (o EF Core a ignora pelo mapeamento).
- Novo record `PremioFaixa(int Faixa, int Quantidade, decimal ValorPremio)` com `Total = Quantidade × ValorPremio`.
- `ConferenciaJogo` passa a `ConferenciaJogo(int Acertos, IReadOnlyList<PremioFaixa> Premios)`:
  - `Premios`: para k de 15 a 11, `Combinacoes(a, k) × Combinacoes(n − a, 15 − k)`, mantendo só as faixas com
    quantidade > 0 (da maior para a menor), com `ValorPremio` do rateio gravado da faixa k;
  - `Premio` = soma dos `Total` (mantém o nome usado hoje pela Aplicação e pela Web);
  - `Premiado` = `Premios.Count > 0`. Com a ≥ 11 sempre existe a faixa k = a (C(n − a, 15 − a) ≥ 1 porque n ≥ 15),
    então equivale à regra atual "11 ou mais acertos".
  - Para n = 15 a fórmula dá um único prêmio na faixa dos acertos: os testes atuais da conferência continuam válidos.
  - Alternativa: expandir o jogo nas C(n, 15) apostas de 15 e conferir uma a uma. Descartada: até 15.504 combinações
    por jogo a cada exibição, sem ganho de clareza.

### Aplicação
- `GrupoJogos` ganha `QuantidadeApostas` (soma de `Jogo.QuantidadeApostas` do grupo). O custo continua sendo
  calculado na Web multiplicando apostas pelo preço da configuração, como hoje — a única mudança é trocar "quantidade
  de jogos" por "quantidade de apostas".
- `JogosServico.CadastrarAsync` não muda de assinatura; a validação da quantidade fica no construtor de `Jogo`.
  A tela só permite salvar com a quantidade certa, então a exceção do domínio continua sendo erro de programação.

### Infraestrutura
- Sem migration: a coluna de dezenas já guarda uma lista de qualquer tamanho e o índice único compara o texto JSON
  ordenado, o que mantém "mesmas dezenas no mesmo concurso" e diferencia um jogo de 15 contido num de 16.
- O teste existente que falha se o modelo mudar sem migration confirma que `QuantidadeApostas` não entrou no modelo.

### Web
- `CadastroJogo`: `MudSelect<int>` "Dezenas por jogo" (15 a 20, padrão 15) ao lado de "Concurso"; o valor vai para
  `Volante.Limite`. `PodeSalvar` exige `marcadas == dezenasPorJogo`. A contagem usa
  `Combinacoes(dezenasPorJogo, 15) × PrecoApostaMinima` (injeta `OpcoesApostas`).
- `MeusJogos`: custo do grupo = `grupo.QuantidadeApostas × preço`; investido = soma das apostas × preço; detalhe do
  investido usa a soma das apostas em `FormatacaoJogo.QuantidadeApostas` (texto atual mantido). Linha do jogo mostra
  "<n> dezenas" quando `Dezenas.Count > 15`.
- `DialogoConferencia`: item "Aposta" (`FormatacaoJogo.Aposta(jogo)`: "15 dezenas" ou "16 dezenas · 16 apostas") e,
  para jogos de 16 a 20 premiados, a lista de `Premios` no formato "<quantidade> × <faixa> acertos · <valor>".
- Novos textos ficam em `FormatacaoJogo`, testados como os atuais.

## Risks / Trade-offs

- [Prêmio de faixa com valor zero no rateio gravado (rateio "não divulgado")] → comportamento igual ao atual: o jogo
  conta como premiado com prêmio R$ 0,00 naquela faixa; nada novo a tratar.
- [Valores altos em `decimal` (ex.: 15 prêmios de 14 com rateio grande)] → `decimal` comporta com folga; teste com a
  aposta de 20 dezenas cobre o maior multiplicador.
- [Bolas de jogos de 20 dezenas ocupando duas linhas na lista] → aceito; `flex-wrap` já trata. Conferir na
  verificação manual.
- [Construtor de `ConferenciaJogo` muda de assinatura] → é usado só pelo próprio `Conferir`; testes que verificam
  `Premio` continuam funcionando.
