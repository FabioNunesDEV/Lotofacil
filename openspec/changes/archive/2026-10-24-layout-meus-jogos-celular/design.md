## Context

A linha do jogo em Meus jogos (`.linha-jogo`) é um grid `28px 1fr auto`: o número, as dezenas (`.dezenas-jogo`, com
as bolas e a marca da Teimosinha) e o resultado (`.resultado-jogo`: chip de acertos ou "Aguardando", prêmio com
`min-width: 96px`, e o botão de excluir). Num jogo conferido, a coluna `auto` mede cerca de 213 px; em 390 px a linha
tem uns 326 px, e a coluna `1fr` das dezenas fica com uns 46 px, uma bola por linha.

## Goals / Non-Goals

**Goals:**
- Em telas estreitas, as dezenas usam a largura da linha, e o resultado fica embaixo delas.

**Non-Goals:**
- Mudar o desktop, os textos ou a marcação (os testes bUnit continuam valendo).

## Decisions

- **Ponto de quebra:** `max-width: 600px`, o mesmo de `app.css` e de `DialogoConferencia.razor.css` para ajustes de
  celular.
- **Abaixo dele:** `.linha-jogo` passa a `grid-template-columns: 28px 1fr`, e `.resultado-jogo` vai para
  `grid-column: 2`, com quebra de linha permitida. Assim, o resultado desce para baixo das dezenas, alinhado com
  elas. O número continua na primeira coluna, alinhado ao topo da linha.
- **Prêmio:** perde a largura mínima e o alinhamento à direita, que só servem para alinhar os valores em coluna no
  desktop.
- **Alternativa descartada:** diminuir as bolas. Ainda faltaria espaço com o resultado ao lado, e as bolas ficariam
  pequenas para tocar.

## Risks / Trade-offs

- [Linha mais alta no celular] → aceitável: é o que deixa as dezenas legíveis.

## Migration Plan

Nada a migrar.
