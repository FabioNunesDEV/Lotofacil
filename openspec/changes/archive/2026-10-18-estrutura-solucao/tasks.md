## 0. Dependências autorizadas

- [x] 0.1 Incluir `Serilog.Extensions.Logging` e `Serilog.Sinks.File` na lista de dependências autorizadas do `CLAUDE.md` (autorizados pelo usuário) e registrar que o xUnit roda pela Microsoft.Testing.Platform; verificar lendo a seção "Dependências"

## 1. Configurações da raiz

- [x] 1.1 Criar `global.json` com SDK `10.0.302`, `rollForward: latestFeature` e `test.runner` = `Microsoft.Testing.Platform`; verificar que `dotnet --version` na raiz mostra 10.0.x
- [x] 1.2 Criar `Directory.Build.props` (`net10.0`, `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `GenerateDocumentationFile`, StyleCop e `stylecop.json` como `AdditionalFiles`)
- [x] 1.3 Criar `Directory.Packages.props` com as versões estáveis mais recentes de `MudBlazor`, `Serilog.Extensions.Logging`, `Serilog.Sinks.File`, `xunit.v3`, `NSubstitute` e `bunit`, e `StyleCop.Analyzers` 1.2.0-beta.556 como `GlobalPackageReference`; verificar com `dotnet restore` sem avisos
- [x] 1.4 Criar `stylecop.json` (empresa "Fábio Nunes", cabeçalho em português) e `.editorconfig` (SA1623 e SA1642 desativadas); verificar abrindo um arquivo `.cs` sem cabeçalho e confirmando que o build acusa SA1633

## 2. Solução e projetos de biblioteca

- [x] 2.1 Criar `src/Lotofacil.slnx` e os projetos `Lotofacil.Dominio`, `Lotofacil.Aplicacao` e `Lotofacil.Infraestrutura` (sem a classe de exemplo do template); verificar com `dotnet build src/Lotofacil.slnx` sem avisos
- [x] 2.2 Adicionar as referências entre projetos conforme a tabela do `design.md`; verificar com `dotnet build` sem avisos

## 3. Aplicação Web

- [x] 3.1 Criar `Lotofacil.Web` pelo template `blazor` (Server, vazio), adicionar à solução e referenciar `Aplicacao` e `Infraestrutura`; verificar com `dotnet build` sem avisos
- [x] 3.2 Remover Bootstrap (arquivos, referências e classes) e o redirecionamento HTTPS/HSTS; verificar que não há ocorrências de `bootstrap` em `src/Lotofacil.Web`
- [x] 3.3 Configurar o Kestrel para `http://localhost:5080` em `appsettings.json` e `launchSettings.json`; verificar que `dotnet run` responde em `http://localhost:5080` e não pelo IP da máquina
- [x] 3.4 Adicionar MudBlazor (serviços, providers, CSS e JS) e criar o `MainLayout` com `MudAppBar` (título "Lotofácil" e botão de menu) e `MudDrawer` com o item "Painel"; verificar no navegador o layout e o recolhimento do menu
- [x] 3.5 Criar a página `Painel` na rota `/` e a página "Página não encontrada" com link para o Painel; verificar no navegador as duas rotas
- [x] 3.6 Criar `ConfiguracaoLog` (nível mínimo Erro, arquivo diário `lotofacil-AAAAMMDD.log` na pasta informada) e ligá-la no `Program.cs` com a pasta lida de `Log:Pasta` (padrão `log`) relativa ao `ContentRootPath`; verificar com `dotnet build` sem avisos

## 4. Testes

- [x] 4.1 Criar `Lotofacil.Testes.Unidade` (xUnit v3, NSubstitute, bUnit) referenciando `Dominio`, `Aplicacao` e `Web`; verificar com `dotnet test` sem testes falhando
- [x] 4.2 Escrever testes bUnit: layout mostra "Lotofácil" e o item "Painel"; Painel exibe o título; página não encontrada exibe a mensagem e o link; verificar com `dotnet test`
- [x] 4.3 Criar `Lotofacil.Testes.Integracao` (xUnit v3) referenciando todos os projetos de produção e escrever o teste de arquitetura das dependências entre camadas; verificar com `dotnet test` e, temporariamente, provocar uma referência indevida para confirmar que o teste falha
- [x] 4.4 Escrever o teste de log: com `ConfiguracaoLog` apontando para uma pasta temporária inexistente, registrar um Erro com exceção e uma Informação; verificar com `dotnet test` que a pasta foi criada e o arquivo contém só o Erro, com tipo e mensagem da exceção

## 5. Fechamento

- [x] 5.1 Executar `dotnet build src/Lotofacil.slnx` (zero avisos) e `dotnet test --solution src/Lotofacil.slnx` (todos passando) a partir da raiz
- [x] 5.2 Conferir que `.gitignore` cobre `bin/`, `obj/` e os arquivos `*.log` da pasta `log` (`git status` não lista artefatos de build nem logs após executar a aplicação)
