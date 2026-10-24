## 1. Layout

- [x] 1.1 `MudDrawer` com `Variant="DrawerVariant.Mini"` e cada `MudNavLink` dentro de um `MudTooltip` com o nome
  da página, desabilitado com o menu aberto; verificar com testes bUnit: variante mini, dica com o nome em cada item,
  dica desabilitada aberto e habilitada recolhido, testes atuais do layout passando

## 2. Verificação final

- [x] 2.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 2.2 Verificação manual: em 1536 px, recolher (só ícones, dica ao passar o mouse, ícone navega), abrir de novo
  (texto volta e empurra a página, sem dica); em 390 px, menu começa fechado, abre por cima e some ao fechar
