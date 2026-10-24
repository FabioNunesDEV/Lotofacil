## Why

Em Meus jogos, numa tela de celular (390 px), as linhas dos jogos conferidos ficam com as bolas empilhadas numa
coluna só. A coluna da direita da linha (acertos, prêmio com largura mínima de 96 px e o botão de excluir) ocupa
cerca de 213 px e deixa uns 46 px para as dezenas. O problema foi notado na verificação da fatia 15 e já existia
antes dela.

## What Changes

- Em telas estreitas (abaixo de 600 px), a linha do jogo passa a ter duas colunas: o número do jogo e, ao lado, as
  dezenas ocupando toda a largura que sobra. O resultado (acertos, prêmio e excluir) desce para baixo das dezenas,
  na mesma coluna.
- No desktop nada muda.
- Só CSS de Meus jogos; nenhum texto, regra ou comportamento muda.

## Capabilities

### New Capabilities
<!-- Nenhuma. -->

### Modified Capabilities
<!-- Nenhuma: as specs não descrevem a disposição da linha do jogo; a mudança é só de layout
     (skip_specs: true). -->

## Impact

- `MeusJogos.razor.css`: regra para telas abaixo de 600 px.
- Testes atuais da página continuam valendo (a marcação não muda). Verificação manual em 390 px e no desktop.
