## Why

Em Gerar jogos, na lista "Jogos gerados", a linha de cada jogo usa a grade `28px 1fr auto`: os chips de perfil
(pares e ímpares, soma, repetidas) ficam numa coluna `auto` que ocupa a largura de todos eles numa linha só. No
desktop (1280 px) sobram cerca de 90 px para as dezenas, que ficam espremidas em duas colunas, com um espaço vazio até
os chips. O problema apareceu ao tirar as capturas de tela do README.

## What Changes

- As dezenas do jogo gerado passam a ocupar a largura da linha, e os chips de perfil descem para baixo delas,
  alinhados às dezenas; o botão de remover fica no fim da linha dos chips, à direita.
- O número do jogo fica centralizado na altura das duas linhas.
- As bolas da lista passam de 30 para 28 px, e a coluna do número, de 28 para 24 px, para as 15 dezenas caberem numa
  linha a partir de 1280 px de tela.
- Só CSS de `GerarJogos.razor.css`; nenhum texto, regra ou comportamento muda.

## Capabilities

### New Capabilities
<!-- Nenhuma. -->

### Modified Capabilities
<!-- Nenhuma: as specs não descrevem a disposição da linha do jogo gerado; a mudança é só de layout
     (skip_specs: true). -->

## Impact

- `GerarJogos.razor.css`: grade da linha do jogo gerado.
- Testes atuais de Gerar jogos continuam valendo (a marcação não muda). Verificação manual no desktop e em 390 px.
