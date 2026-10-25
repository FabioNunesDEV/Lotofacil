## Context

Ver proposal.md (Why). Estado atual e o que o diagnóstico mostrou:

- `Lotofacil.Web.csproj` define `OutputType` `WinExe` quando `Distribuicao=true`, propriedade passada só pelo
  `instalador/gerar-instalador.ps1`.
- O `blazor.web.js` vem do pacote implícito `Microsoft.AspNetCore.App.Internal.Assets` (10.0.13). O target
  `_AddBlazorFrameworkStaticWebAssets` dele tem `Condition="'$(OutputType)' == 'Exe'"`; com `WinExe` o script não entra
  nos static web assets nem no `Lotofacil.Web.staticwebassets.endpoints.json` da publicação, e `/_framework/blazor.web.js`
  responde 404.
- No SDK, o subsistema do executável vem de `_UseWindowsGraphicalUserInterface`, calculado em
  `_GetAppHostCreationConfiguration` como `true` quando o `OutputType` é `WinExe` (RID Windows); a propriedade só é
  atribuída nesse caso, então um `true` vindo do projeto se mantém com `Exe`.
- Validação já feita numa cópia da publicação em `Production`: com `OutputType` `Exe` e
  `_UseWindowsGraphicalUserInterface=true`, o executável sai com subsistema GUI (2), `/_framework/blazor.web.js`
  responde 200 e "Importar histórico" conclui; na publicação atual, 404 e o clique não faz nada.

## Goals / Non-Goals

**Goals:**
- App instalado interativo e ainda sem janela de console.
- Impedir que um instalador sem o script do Blazor volte a ser gerado sem aviso.

**Non-Goals:**
- Mudar o modo de execução (bandeja, instância única, navegador) ou o Inno Setup.
- Teste automatizado da publicação no `dotnet test`: publicar com .NET incluído é lento e já fica coberto pela
  conferência do script de geração.

## Decisions

### `Exe` com o executável marcado como gráfico
No grupo da `Distribuicao`, trocar `<OutputType>WinExe</OutputType>` por
`<_UseWindowsGraphicalUserInterface>true</_UseWindowsGraphicalUserInterface>`, com um comentário dizendo por que o
`OutputType` fica `Exe`. Uma linha, sem cópia de lógica do pacote, e o resultado é o mesmo executável sem console.

Alternativas consideradas:
- **Copiar o target do pacote para o projeto** (acrescentar o `blazor.web.js` com `DefineStaticWebAssets` quando for
  `WinExe`): ~30 linhas de MSBuild que repetem detalhes internos do pacote (`BlazorFrameworkStaticWebAssetRoot`,
  tarefas e metadados); mais frágil que uma propriedade.
- **Esconder o console em tempo de execução** (`FreeConsole` no início): a janela pisca ao abrir pelo atalho e exige
  P/Invoke no `Program.cs`.
- **Trocar o `OutputType` durante o build** (target que muda para `Exe` antes do target do pacote): a condição de um
  target é avaliada antes dos `BeforeTargets`, o que obriga a se encaixar na lista de dependências do SDK; difícil de
  entender e manter.
- **Copiar o `blazor.web.js` para o `wwwroot`**: fica preso à versão do arquivo e duplica o que o framework já entrega.

### Conferência no script de geração
Depois do `dotnet publish` e antes do Inno Setup, o `gerar-instalador.ps1` confere se existe
`<publicação>\wwwroot\_framework\blazor.web.js`; se não existir, lança erro com o caminho que faltou. O script já usa
`$ErrorActionPreference = 'Stop'` e `throw` nas outras falhas, então segue o mesmo padrão. A conferência pelo arquivo
físico basta: o manifesto de endpoints é gerado a partir dos mesmos static web assets.

### Versão 1.0.1
`Directory.Build.props` passa para 1.0.1, para o instalador corrigido ter outro nome e outra versão no "Adicionar ou
remover programas". Reinstalar por cima já mantém pastas e dados (requisito "Reinstalação e atualização").

## Risks / Trade-offs

- [`_UseWindowsGraphicalUserInterface` é uma propriedade interna do SDK e pode mudar de nome] → O sintoma seria só a
  volta da janela de console, visível ao abrir o app; o comentário no `csproj` aponta a causa, e o `global.json` fixa o
  SDK 10.
- [O pacote pode mudar a condição ou o caminho do script numa versão futura] → A conferência do script de geração para
  a geração do instalador em vez de gerar um app sem interatividade.
- [Quem já instalou a 1.0.0 continua com o app quebrado] → Reinstalar com o 1.0.1 por cima, mantendo os dados.

## Migration Plan

Gerar `Lotofacil-Setup-1.0.1.exe` e instalar por cima da 1.0.0 nas máquinas onde ela foi instalada. Rollback não se
aplica: a 1.0.0 não funciona instalada.
