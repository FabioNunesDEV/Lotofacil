## Context

A seção "Seus jogos no concurso" do Painel tem `.linha-jogo` com `grid-template-columns: 28px 1fr auto`: o número, as
bolas e `.resultado-jogo` (chip de dezenas, chip de acertos e `.premio` com `min-width: 96px`). Em 390 px a coluna
`auto` empurra a linha 8 px além da tela. Meus jogos tinha a mesma grade e foi corrigido com uma regra
`@media (max-width: 600px)`.

## Goals / Non-Goals

**Goals:**
- Sem rolagem horizontal no Painel em 390 px, com o mesmo arranjo da linha de Meus jogos.

**Non-Goals:**
- Extrair um componente de linha comum às duas páginas. A grade é pequena, e o projeto mantém o CSS isolado por
  página.

## Decisions

- Mesma regra de `MeusJogos.razor.css`, abaixo de 600 px: `.linha-jogo` com `28px 1fr` e `align-items: start`;
  `.resultado-jogo` em `grid-column: 2` com quebra; `.premio` sem largura mínima e alinhado à esquerda.

## Risks / Trade-offs

- [Regra repetida nas duas páginas] → aceitável enquanto forem só duas; se uma terceira página precisar da mesma
  linha, vale extrair um componente.

## Migration Plan

Nada a migrar.
