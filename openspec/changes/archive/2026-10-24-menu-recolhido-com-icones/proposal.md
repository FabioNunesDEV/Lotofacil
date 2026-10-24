## Why

Hoje, ao fechar o menu lateral pelo botão da barra superior, o menu some por inteiro, e para trocar de página é
preciso abri-lo de novo. O usuário quer que, ao fechar, só os textos sumam e os ícones de cada página continuem à
vista, como atalhos.

## What Changes

- Em telas largas (a partir de 960 px), o menu fechado fica recolhido numa faixa estreita só com os ícones; aberto,
  continua com ícone e texto, como hoje.
- Com o menu recolhido, passar o mouse sobre um ícone mostra uma dica com o nome da página. Com o menu aberto, a dica
  não aparece.
- Em telas estreitas (abaixo de 960 px, celular), nada muda: fechado o menu some, aberto ele aparece por cima da
  página.
- O menu continua começando aberto ao carregar a página, sem lembrar o último estado.

## Capabilities

### New Capabilities
<!-- Nenhuma. -->

### Modified Capabilities
- `aplicacao-web`: no requisito "Layout principal", o cenário "Recolher o menu lateral" passa a dizer que, em telas
  largas, o menu recolhido mostra só os ícones, com a dica do nome, e que no celular ele some como hoje.

## Impact

- Web: `MainLayout.razor` (variante do menu e dicas nos itens). Sem mudança de código fora do layout.
- Testes bUnit do layout. Sem pacote novo.
