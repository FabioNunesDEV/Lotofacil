## Why

No Painel, numa tela de celular (390 px), a lista "Seus jogos no concurso" passa 8 px da largura da tela e cria
rolagem horizontal. A linha do jogo usa a mesma grade de Meus jogos (número, dezenas e resultado com prêmio de largura
mínima de 96 px), mas não recebeu a correção feita em `layout-meus-jogos-celular`. O problema foi notado na
verificação de `menu-recolhido-com-icones`.

## What Changes

- Abaixo de 600 px, a linha do jogo no Painel passa a ter duas colunas (número e dezenas), e o resultado (quantidade
  de dezenas, acertos e prêmio) desce para baixo das dezenas, como em Meus jogos.
- O prêmio perde a largura mínima no celular. Desktop sem mudança.
- Só CSS do Painel; nenhum texto, regra ou comportamento muda.

## Capabilities

### New Capabilities
<!-- Nenhuma. -->

### Modified Capabilities
<!-- Nenhuma: as specs não descrevem a disposição da linha do jogo; a mudança é só de layout
     (skip_specs: true). -->

## Impact

- `Painel.razor.css`: regra para telas até 600 px.
- Testes atuais do Painel continuam valendo (a marcação não muda). Verificação manual em 390 px e no desktop.
