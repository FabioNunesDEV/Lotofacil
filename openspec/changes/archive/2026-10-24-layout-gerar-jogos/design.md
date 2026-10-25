## Context

Em `GerarJogos.razor.css`, `.linha-gerado` usa `grid-template-columns: 28px 1fr auto`: o número, `.bolas` (bolas de
30 px com 4 px de espaço) e `.perfil` (chips e o botão de remover, alinhados à direita). A coluna `auto` assume a
largura de todos os chips numa linha (~350 px), e a coluna `1fr` das bolas encolhe até o mínimo do conteúdo: a 1280 px
sobram ~90 px para as 15 bolas. Meus jogos e o Painel já resolveram um caso parecido descendo o resultado para baixo
das dezenas.

## Goals / Non-Goals

**Goals:**
- As 15 dezenas numa linha no desktop (a partir de 1280 px de tela) e o perfil do jogo legível embaixo delas.

**Non-Goals:**
- Mudar a marcação, os textos ou o volante de fixas e excluídas (que tem CSS próprio).

## Decisions

- `.linha-gerado` com `24px 1fr` e `gap: 6px 12px`; `.numero-jogo` ocupa as duas linhas (`grid-row: span 2`);
  `.perfil` em `grid-column: 2`, alinhado à esquerda, com o botão de remover empurrado para a direita
  (`::deep .botao-remover { margin-left: auto }`, já que é um componente do MudBlazor).
- Bolas da lista de 30 para 28 px: com a coluna de ~482 px a 1280 px, as 15 bolas (15 × 28 + 14 × 4 = 476 px) cabem
  numa linha. Com 16 a 20 dezenas, ou em telas menores, as bolas quebram em mais linhas.
  - Alternativa descartada: manter os chips à direita só em telas largas (media query). O cartão da lista é estreito
    até em 1280 px, e a regra ficaria valendo só em monitores grandes.

## Risks / Trade-offs

- [Bolas um pouco menores que as de Meus jogos] → a diferença de 2 px não atrapalha a leitura e evita a quebra.

## Migration Plan

Nada a migrar.
