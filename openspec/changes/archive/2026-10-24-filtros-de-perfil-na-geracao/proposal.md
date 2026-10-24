## Why

O gerador da fatia 8 escolhe as dezenas só pela estratégia, pelas fixas e pelas excluídas. A fatia 9 passou a mostrar
como os concursos se distribuem por perfil, mas o usuário ainda não consegue pedir jogos de um perfil (por exemplo,
7 ou 8 pares e soma entre 180 e 209) nem descartar jogos com sequências muito longas, como 01–15. Esta fatia (10 do
`Docs/roteiro.md`) leva os perfis da fatia 9 ao gerador como filtros opcionais. As decisões que o roteiro marcava
como "a confirmar" foram tomadas pelo usuário no início da fatia.

## What Changes

- Na página **"Gerar jogos"**, nova seção **"Filtros de perfil"**, desligada por padrão:
  - **mínimo e máximo** (cada um opcional) para os 7 perfis da aba "Por perfil" das Estatísticas: pares, primos,
    múltiplos de 3, Fibonacci, soma, repetidas do último concurso gravado e moldura;
  - **"Evitar padrões populares"**: liga o campo **"Maior sequência permitida"** (padrão 9), que recusa jogos com
    mais dezenas seguidas que o limite. Com 9, só caem as sequências de 10 ou mais, cerca de 1% das combinações de
    15 dezenas (01–15, por exemplo);
  - **dica de chance teórica** em cada perfil limitado, quando o jogo tem 15 dezenas: o percentual das 3.268.760
    combinações que fica dentro do mínimo e do máximo informados;
  - texto fixo dizendo que os filtros só escolhem o perfil dos jogos e não mudam a chance de acerto. Evitar padrões
    populares só pode reduzir a divisão do prêmio quando o jogo é premiado.
- **Jogos de 16 a 20 dezenas**: os filtros valem sobre todas as dezenas do jogo. Os limites de quantidade aceitam
  de 0 a n, e os de soma vão da menor à maior soma possível com n dezenas. As dicas de chance aparecem só para 15.
- **Geração com filtros**: um jogo só entra na lista quando atende a todos os filtros ligados. Quando os filtros não
  deixam chegar à quantidade pedida, a página mostra os jogos achados e avisa. Quando há poucas combinações, a
  enumeração de todas as possíveis também passa pelos filtros. Se nenhum jogo atender, a geração é recusada com
  mensagem.
- **Validação**: mínimo maior que o máximo recusa a geração com mensagem que nomeia o perfil.
- Retirado do roteiro o critério "jogos só com dezenas até 31 (datas)". Na Lotofácil, todas as dezenas vão até 25.
- Linha ou coluna completa do volante **não** vira filtro de padrão popular, porque aparece em 48,8% das combinações.
- `CLAUDE.md` (decisões da geração) e `Docs/roteiro.md` (fatia 10 concluída) atualizados.
- Testes de unidade (domínio, aplicação, formatos) e de componente com bUnit.

## Capabilities

### New Capabilities
<!-- nenhuma -->

### Modified Capabilities
- `geracao-jogos`: novos requisitos para os filtros de perfil, o limite de sequência e a dica de chance teórica. As
  "Regras da geração" passam a considerar os filtros (jogos que não atendem são descartados, avisos e recusas novos).

## Impact

- `Lotofacil.Dominio`: filtros da geração (limites por perfil e maior sequência) nos parâmetros. O gerador descarta
  os jogos fora dos filtros, tanto no sorteio quanto na enumeração, e calcula a maior sequência de um jogo. Reaproveita
  `ValorPerfil` e `DistribuicaoTeorica` da fatia 9.
- `Lotofacil.Aplicacao`: `GeracaoJogosServico` passa ao gerador o último resultado gravado, usado pelo filtro de
  repetidas.
- `Lotofacil.Web`: seção de filtros na página Gerar jogos, formatos dos avisos e das dicas.
- `Lotofacil.Infraestrutura`: sem mudança; sem migration (filtros não são gravados).
- Sem pacotes novos.
- Fora do escopo: gravar os filtros escolhidos, filtros de linhas/colunas, ciclos, duplas/trincas, Teimosinha e
  Bolão.
