## Context

Repositório sem código. A máquina tem os SDKs 9, 10.0.302 e 11 RC instalados; sem `global.json`, o `dotnet`
escolheria o 11 RC. As regras de arquitetura, nomenclatura, StyleCop e dependências estão no `CLAUDE.md`. O
protótipo em `prototipo/` define a aparência e a navegação que as telas reais vão seguir.

## Goals / Non-Goals

**Goals:**
- Solução compilando sem nenhum aviso, com as regras de qualidade valendo para todos os projetos desde o primeiro
  arquivo.
- Testes de unidade e de integração rodando com `dotnet test` a partir da raiz.
- Arquitetura protegida por teste: uma referência indevida entre camadas quebra o build de testes.

**Non-Goals:**
- Banco de dados, EF Core e migrations (change `concursos-base`).
- Acesso à API da Caixa e importação do XLSX (changes seguintes).
- Telas além do Painel vazio; itens de menu só entram quando a página existir.
- Pipeline de CI.

## Decisions

### Estrutura de pastas

```
/ (raiz)
+-- global.json  Directory.Build.props  Directory.Packages.props  stylecop.json  .editorconfig
+-- src/
    +-- Lotofacil.slnx
    +-- Lotofacil.Dominio/            (sem referências)
    +-- Lotofacil.Aplicacao/          -> Dominio
    +-- Lotofacil.Infraestrutura/     -> Dominio, Aplicacao
    +-- Lotofacil.Web/                -> Aplicacao, Infraestrutura
    +-- Lotofacil.Testes.Unidade/     -> Dominio, Aplicacao, Web
    +-- Lotofacil.Testes.Integracao/  -> Dominio, Aplicacao, Infraestrutura, Web
```

Projetos e testes ficam todos em `src/` (decisão do usuário). As configurações compartilhadas ficam na raiz, onde o
MSBuild e os analisadores as encontram para qualquer projeto abaixo dela.

### `global.json`
SDK `10.0.302` com `rollForward: latestFeature`. Aceita atualizações do SDK 10 (10.0.3xx, 10.0.4xx...) e nunca
usa o 11. *Alternativa:* `latestPatch`, que quebraria o build se o SDK 10.0.3xx fosse removido numa atualização do
Visual Studio. Também define `test.runner` = `Microsoft.Testing.Platform` (ver Testes).

### `Directory.Build.props` (regras comuns a todos os projetos)
- `TargetFramework` `net10.0`, `Nullable` e `ImplicitUsings` habilitados.
- `TreatWarningsAsErrors` = `true`: "build sem avisos" passa a ser garantido pelo compilador.
- `GenerateDocumentationFile` = `true`: necessário para o StyleCop validar os `<summary>` (sem ele o StyleCop emite
  SA0001) e faz o compilador cobrar documentação de membros públicos.
- `stylecop.json` incluído como `AdditionalFiles`. O pacote `StyleCop.Analyzers` entra em todos os projetos como
  `GlobalPackageReference` no `Directory.Packages.props`, que já o trata como analisador privado.

### `Directory.Packages.props` (gerenciamento central de versões)
Cada pacote tem a versão declarada uma única vez; os `.csproj` referenciam sem versão. Evita versões diferentes do
mesmo pacote entre os dois projetos de teste. *Alternativa:* versão em cada `.csproj`, mais simples no início, mas
sujeita a divergências a cada atualização.

### StyleCop
- `.editorconfig`: SA1623 e SA1642 desativadas (exigem texto em inglês), conforme o `CLAUDE.md`; demais regras no
  padrão. As regras padrão já combinam com as convenções do projeto: SA1101 (`this.`), SA1309 (sem `_`), SA1600
  (documentação).
- Versão `1.2.0-beta.556`: a última estável (1.1.118, de 2019) não reconhece a sintaxe atual do C# (namespace de
  arquivo, `record`, `new()`) e geraria falsos positivos; os betas 1.3.x saíram há poucos dias e ainda mudam a cada
  versão.
- `using` fora do namespace (`usingDirectivesPlacement: outsideNamespace`), combinando com o namespace de arquivo.
- SA1633 (cabeçalho de arquivo) permanece ativa. `stylecop.json` define `companyName` = "Fábio Nunes" (titular do
  `LICENSE`) e o texto em português: `Copyright (c) Fábio Nunes. Todos os direitos reservados.`
- As regras valem também para os projetos de teste: o `CLAUDE.md` exige `<summary>` em toda classe e todo método.
- Código gerado pelo Razor não é analisado; classes de código (`.razor.cs`, `Program.cs`) são.

### Aplicação Web
- Criada com o template `blazor` (interatividade Server, modo vazio). Remove-se do template tudo o que for Bootstrap
  (arquivos em `wwwroot/lib`, referências no `App.razor` e classes nos componentes).
- MudBlazor: `AddMudServices()`, `MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider`,
  `MudSnackbarProvider` no layout; CSS e JS do MudBlazor referenciados no `App.razor`.
- `MainLayout` com `MudLayout` + `MudAppBar` (título e botão de menu) + `MudDrawer` com `MudNavMenu`.
  Ícones do próprio MudBlazor (Material Icons).
- Criada com interatividade global (`--all-interactive`): o botão que recolhe o menu fica no layout, e o layout só
  responde a eventos com interatividade global.
- Páginas com nomes em português: `Painel` (`/`), `PaginaNaoEncontrada` (`/nao-encontrada`) e `Erro` (`/erro`, usada
  pelo `UseExceptionHandler` fora do desenvolvimento). Textos do template, inclusive da janela de reconexão,
  traduzidos.
- Somente `localhost`: endpoint do Kestrel em `appsettings.json` (`http://localhost:5080`), `AllowedHosts` =
  `localhost` e o mesmo endereço no `launchSettings.json`. Vale tanto para `dotnet run` quanto para o executável publicado. Só HTTP: por ser local,
  não precisa de certificado de desenvolvimento; removem-se `UseHttpsRedirection` e `UseHsts` do template.
- Composição da injeção de dependência no `Program.cs`. Nesta change não há serviços próprios para registrar.

### Log de erros (Serilog)
- Pacotes: `Serilog.Extensions.Logging` (liga o Serilog ao `ILogger` do .NET) e `Serilog.Sinks.File` (grava em
  arquivo). O Serilog entra como **mais um destino** do log (`builder.Logging.AddSerilog`), e o console padrão do
  .NET continua mostrando as mensagens informativas.
  *Descartado na implementação:* `Serilog.Extensions.Hosting`, cujo `AddSerilog` substitui todo o log do .NET; com
  o nível mínimo Erro, o console ficaria mudo, sem nem a mensagem do endereço em que a aplicação escuta.
  *Também descartados:* `Serilog.AspNetCore`, que traz log de requisições HTTP sem uso aqui, e
  `Serilog.Settings.Configuration`, pois a configuração cabe em poucas linhas de código e só a pasta vem do
  `appsettings.json`.
- Todo o código continua usando `ILogger<T>`; o Serilog só substitui o destino das mensagens. As exceções não
  tratadas do circuito Blazor já são registradas pelo framework como Erro e caem no arquivo sem código extra.
- Nível mínimo `Error` (inclui `Fatal`). Arquivo `log/lotofacil-AAAAMMDD.log`, um por dia, mantendo os 31 mais
  recentes (padrão do Serilog). Texto simples:
  `2026-10-18 21:15:03.123 -03:00 [ERR] Lotofacil.Web.Origem: mensagem` + exceção na linha seguinte.
- Pasta configurável em `appsettings.json` (`Log:Pasta`, padrão `log`), resolvida a partir do `ContentRootPath`.
  No `dotnet run` fica em `src/Lotofacil.Web/log`; no executável publicado, ao lado dele.
- A montagem da configuração fica numa classe estática da Web (ex.: `ConfiguracaoLog`), que recebe a pasta e
  devolve a `LoggerConfiguration`. Assim o teste de integração usa exatamente a mesma configuração do `Program.cs`,
  apontando para uma pasta temporária.

### Testes
- **xUnit v3** (pacote `xunit.v3`, que é a versão atual do `xunit`), `NSubstitute` e `bunit`.
- Execução pela **Microsoft.Testing.Platform (MTP)**: o `xunit.v3` 4.x já é um executor MTP, e o SDK 10 recusa
  rodá-lo pelo modo antigo (VSTest). A MTP é ligada no `global.json`, e o comando passa a ser
  `dotnet test --solution src/Lotofacil.slnx` (ou `--project`). `Microsoft.NET.Test.Sdk` e
  `xunit.runner.visualstudio`, adaptadores do VSTest, não são usados. O Visual Studio 2022 17.14+ e o 2026 executam
  testes MTP no Gerenciador de Testes.
  *Alternativa descartada:* voltar ao `xunit.v3` 3.x com VSTest, o que exigiria os dois pacotes e uma versão antiga.
- Os projetos de teste têm `using Xunit` global no `.csproj`, como no template do xUnit.
- `Testes.Unidade`: testes bUnit do layout, do Painel e da página não encontrada, com `AddMudServices()` e
  JSInterop em modo `Loose` (o MudBlazor chama JavaScript que o bUnit não executa). Sem JavaScript, o `MudDrawer`
  responsivo recebe a largura de tela padrão e começa fechado; por isso o teste do botão de menu verifica que cada
  clique inverte o estado, sem supor o estado inicial.
- `Testes.Integracao`: teste de arquitetura que carrega os assemblies e verifica, por
  `Assembly.GetReferencedAssemblies()`, que nenhuma camada referencia outra fora da tabela acima (ex.: `Dominio`
  não referencia nada do projeto; `Aplicacao` não referencia `Infraestrutura`). Também cobre o log: grava, com a
  configuração real, um Erro e uma Informação numa pasta temporária inexistente e confere que só o Erro está no
  arquivo. Os testes de SQLite e de cliente HTTP falso entram nas próximas changes.
- O requisito "Execução somente local" é verificado pela configuração do Kestrel e manualmente; um teste
  automatizado exigiria subir o servidor real (pacote `Microsoft.AspNetCore.Mvc.Testing`, não autorizado).

## Risks / Trade-offs

- [Teste de arquitetura só enxerga referências usadas no código compilado; um `ProjectReference` sem uso não
  aparece] → Aceitável: o que importa é o uso real. A tabela de referências nos `.csproj` é revisada no PR.
- [`TreatWarningsAsErrors` atrasa a prototipagem rápida] → É a regra do projeto; desativar uma regra específica só
  com justificativa registrada no `.editorconfig`.
- [O template do .NET 10 pode trazer arquivos novos (ex.: `ReconnectModal`) que usam CSS próprio] → Mantidos se
  não dependerem de Bootstrap; verificar na implementação.
- [Porta 5080 ocupada por outro programa] → Porta em `appsettings.json`, fácil de trocar.
- [Executar o build em modo Production sem `publish` (`dotnet run --no-launch-profile`) deixa CSS e JS dos pacotes
  sem resposta] → Comportamento normal do .NET: em desenvolvimento usar o perfil `http` do `launchSettings.json`;
  para uso fora do Visual Studio, `dotnet publish`, que copia todos os arquivos estáticos.
- [Erro durante a inicialização, antes de o host montar o Serilog, não chega ao arquivo] → Aceitável numa aplicação
  local: esse erro aparece no console de quem executou. Se fizer falta, envolver o `Program.cs` em `try/catch`
  com um logger de inicialização numa change futura.
- [Arquivo de log aberto pelo usuário enquanto a aplicação escreve] → O `Sinks.File` grava com compartilhamento
  de leitura; abrir no editor não bloqueia a gravação.
