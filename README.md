# Lotofacil
Projeto para auxilio de sorteio de numero para apostas na lotofacil

O Lotofácil roda só no seu computador: baixa os resultados da Caixa quando você pede, mostra estatísticas, gera e
guarda jogos e confere os prêmios. Estatística não aumenta a chance de acerto; cada sorteio é independente.

## Instalar (Windows 64 bits)

1. Execute `Lotofacil-Setup-<versão>.exe`. Não é preciso instalar o .NET nem ter senha de administrador.
2. O Windows pode mostrar "O Windows protegeu o computador", porque o instalador não tem assinatura digital. Clique em
   **Mais informações** e depois em **Executar assim mesmo**.
3. No assistente, escolha:
   - a pasta do programa (padrão `%LOCALAPPDATA%\Programs\Lotofacil`);
   - a pasta dos dados, onde ficam seus jogos, os resultados e o registro de erros (padrão `%LOCALAPPDATA%\Lotofacil`).
     Se a pasta já tiver um `lotofacil.db`, ele será usado;
   - se quer o atalho na Área de trabalho.
4. Ao abrir, o Lotofácil abre o navegador e fica com um ícone perto do relógio (às vezes escondido na seta ˄). Pelo
   ícone: **Abrir o Lotofácil** e **Sair**. Fechar o navegador não fecha o programa.
5. Na primeira vez, a base está vazia: em **Resultados**, importe o histórico da Caixa.

Para atualizar, instale a versão nova por cima: as pastas escolhidas e os dados continuam. Desinstalar (em
"Adicionar ou remover programas") apaga o programa e os dados do Lotofácil (base e log); outros arquivos da pasta dos
dados ficam.

O programa usa o endereço `http://localhost:5080`. Se outro programa já usar a porta 5080, aparece uma mensagem ao
abrir. Para trocar a porta, acrescente ao `appsettings.Production.json`, na pasta do programa, a linha
`"Kestrel": { "Endpoints": { "Http": { "Url": "http://localhost:5081" } } },` logo depois da `{` inicial. O instalador
regrava esse arquivo: repita a troca depois de instalar uma versão nova.

## Desenvolvimento

- .NET 10 (SDK fixado pelo `global.json`), Blazor Server com MudBlazor, EF Core com SQLite.
- Executar: `dotnet run --project src/Lotofacil.Web` (ambiente `Development`: console, base em
  `src/Lotofacil.Web/lotofacil.db`, sem ícone na bandeja).
- Compilar e testar: `dotnet build src/Lotofacil.slnx` e `dotnet test --solution src/Lotofacil.slnx`.
- Regras do projeto: `CLAUDE.md`. Mudanças passam pelo OpenSpec (`openspec/`).

## Gerar o instalador

1. Instale o Inno Setup 6 uma vez: `winget install JRSoftware.InnoSetup`.
2. Ajuste a versão em `<Version>` no `Directory.Build.props`.
3. Execute `pwsh instalador/gerar-instalador.ps1`. O instalador sai em `artefatos/Lotofacil-Setup-<versão>.exe`
   (a pasta `artefatos/` fica fora do Git).

O ícone é gerado por `pwsh instalador/icone/gerar-icone.ps1`, que grava `src/Lotofacil.Web/lotofacil.ico` e
`src/Lotofacil.Web/wwwroot/favicon.png`.
