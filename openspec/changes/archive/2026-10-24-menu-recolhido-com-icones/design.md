## Context

A motivação está em `proposal.md` e o comportamento, em `specs/aplicacao-web/spec.md`. Estado atual relevante:

- `MainLayout.razor` usa `MudDrawer @bind-Open="this.menuAberto" ClipMode="DrawerClipMode.Always"` sem `Variant`,
  ou seja, `DrawerVariant.Responsive`: em telas largas ele empurra o conteúdo quando aberto e some quando fechado;
  abaixo do breakpoint (`Md`, 960 px) vira gaveta temporária, por cima da página.
- `MainLayout.razor.cs` guarda `menuAberto` (começa `true`) e alterna em `AlternarMenu`.
- Os itens são `MudNavLink` com ícone e texto. Os testes bUnit procuram `.mud-drawer a.mud-nav-link` e conferem a
  classe `mud-drawer--open`.

## Goals / Non-Goals

**Goals:**
- Menu recolhido só com ícones em telas largas, com dica do nome, sem mudar o celular.

**Non-Goals:**
- Lembrar o estado do menu entre recargas, expandir ao passar o mouse, trocar ícones ou textos.

## Decisions

- **`Variant="DrawerVariant.Mini"`** no `MudDrawer`. Pela documentação do MudBlazor 9, ele fica estreito (só ícones)
  quando fechado em telas largas e "se comporta como `Temporary` abaixo do breakpoint". Isso já é o comportamento
  pedido para o celular. O breakpoint fica o padrão (`Md`, 960 px), o mesmo do menu atual. `OpenMiniOnHover` fica
  desligado.
  - Alternativa descartada: escolher a variante pela largura da tela com `MudBreakpointProvider`. O próprio `Mini`
    já faz isso.
- **Dica:** cada `MudNavLink` fica dentro de um `MudTooltip` com o nome da página (`Placement.Right`) e
  `Disabled="this.menuAberto"`, para só aparecer com o menu recolhido. Os nomes ficam no próprio markup, como hoje.
- `menuAberto` continua começando `true` e alternando no botão; nada muda no `.razor.cs`.
- **Testes bUnit:** os atuais devem continuar valendo, porque os links continuam em `.mud-drawer a.mud-nav-link` e a
  classe de aberto é a mesma. Entram testes novos: o drawer usa a variante mini; cada item tem a dica com o nome;
  a dica fica desabilitada com o menu aberto e habilitada depois de recolher.

## Risks / Trade-offs

- [Estado inicial no celular com a variante mini] → o menu atual começa aberto e o MudBlazor fecha a gaveta em tela
  estreita ao iniciar. É preciso conferir na verificação manual que a variante mini faz o mesmo. Se não fizer, o
  ajuste é começar fechado abaixo do breakpoint, com uma tarefa nova.
- [Conteúdo com o menu aberto em telas largas] → conferir que o mini aberto continua empurrando a página, como hoje,
  em vez de ficar por cima.

Conferido na verificação manual: em 390 px o menu mini começa fechado, como gaveta temporária fora da tela; em
1536 px, aberto, ele empurra a página (240 px) e, recolhido, fica com 56 px.

## Migration Plan

Nada a migrar.
