## 1. Domínio

- [x] 1.1 Criar `LimitePerfil` e `FiltrosGeracao` (`Nenhum`, `Ligados`, `Atende`, `MaiorSequencia`, `MenorLimite`/
  `MaiorLimite` por perfil e n, nome do filtro nas mensagens); verificar com testes de unidade: maior sequência de
  01 a 15 = 15, de 01 03 05 … = 1 e de 04 05 06 10 11 = 3; limite só com mínimo ou só com máximo; repetidas contra o
  último resultado; jogo de 18 dezenas contado inteiro; soma de 120 a 270 com 15 e de 210 a 310 com 20
- [x] 1.2 Adicionar `filtros` opcional a `ParametrosGeracao`, com validação das faixas (0..n, soma, sequência 1..n);
  verificar com testes de unidade (valores fora da faixa lançam exceção; mínimo > máximo é aceito para o gerador
  recusar)
- [x] 1.3 Aplicar os filtros no `GeradorJogos` (novo parâmetro com o último resultado, recusa por mínimo > máximo,
  enumeração filtrada, sorteio com descarte e limite de tentativas com filtros) e adicionar `ComFiltros` e
  `JogosPedidos` em `ResultadoGeracao`; verificar com testes de unidade: todos os jogos atendem a pares 7–8 e soma
  180–209; sequência 9 e 4 respeitadas; fixas 01–10 com sequência 9 recusadas; pares mínimo 13 recusado; poucas
  combinações com filtro (13 fixas, 8 excluídas → 2 dos 6); filtro quase impossível devolve menos jogos com
  `JogosPedidos`; testes atuais do gerador continuam passando sem filtros

## 2. Aplicação

- [x] 2.1 Passar o último resultado ao gerador em `GeracaoJogosServico.GerarAsync` e repassar `ComFiltros` e
  `JogosPedidos` em `JogosGerados`; verificar com testes de unidade (filtro de repetidas usa o último concurso
  gravado; resultado repassado)

## 3. Web

- [x] 3.1 Adicionar a `FormatacaoGeracao` o resumo dos filtros ligados, a dica "Chance teórica: <p>" e os avisos de
  poucas combinações com e sem filtros e de jogos a menos, com singular e plural; verificar com testes de unidade
  (pares 7–8 = 57,17%; soma 180–209 = 58,73%; "Foram gerados 3 dos 10 jogos pedidos: ..."; "Foi gerado 1 dos ...")
- [x] 3.2 Criar o componente `FiltrosPerfil` (7 linhas Mín./Máx. com faixas por n, rótulo "Repetidas do <último>",
  dica só com 15 dezenas, "Evitar padrões populares" com sequência 9 ao ligar, texto fixo, "Limpar filtros");
  verificar com testes bUnit (campos vazios ao abrir, dica aparece e some com 18 dezenas, sequência 9 ao ligar,
  limpar)
- [x] 3.3 Colocar a seção recolhível "Filtros de perfil" na página Gerar jogos, montar `ParametrosGeracao` com os
  filtros, ajustar limites ao trocar "Dezenas por jogo" e exibir avisos e recusas novos; verificar com testes bUnit
  dos cenários da spec (fechada com "Nenhum filtro ligado", "2 filtros ligados", jogos respeitando os filtros,
  mínimo maior que o máximo, nenhum jogo atende) e com os testes atuais da página passando

## 4. Verificação final

- [x] 4.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 4.2 Verificação manual numa base separada com o histórico real: cada filtro sozinho e combinados, com 15 e 18
  dezenas, conferindo o perfil dos jogos gerados; dicas comparadas com os teóricos da aba "Por perfil"; evitar
  padrões populares com 9 e 4; avisos de jogos a menos, poucas combinações e recusas; salvar jogos filtrados; layout
  em 390px
- [x] 4.3 Atualizar `CLAUDE.md` (decisões da geração: filtros, sequência, apostas múltiplas) e `Docs/roteiro.md`
  (fatia 10 concluída, critério "até 31" retirado); verificar relendo os arquivos
