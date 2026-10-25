## Context

A motivação está em `proposal.md` e o comportamento, nas specs. Estado atual relevante:

- `Program.cs` (Web) monta o `WebApplication` com a raiz de conteúdo padrão (o diretório atual), lê `BancoDados:Arquivo`
  e `Log:Pasta` e os combina com `ContentRootPath` por `Path.Combine`, que já devolve o segundo caminho quando ele é
  absoluto. Depois aplica as migrations, carrega o preço e cria os jogos das Teimosinhas, e chama `app.Run()`.
- O SQLite cria o arquivo da base, mas não a pasta. O Serilog cria a pasta do log.
- `appsettings.json` fixa a porta em `http://localhost:5080`. Não há favicon nem ícone.
- `Directory.Build.props` (raiz) define `net10.0` e `TreatWarningsAsErrors` para todos os projetos. Os dois projetos de
  teste referenciam o `Lotofacil.Web`; nenhum teste executa o `Program`.
- A importação do histórico baixa da Caixa um único arquivo, então um amigo com a base vazia completa a base pelo
  próprio app.

## Goals / Non-Goals

**Goals:**
- Um instalador gerado por um comando, sem pacote NuGet novo, e o app instalado funcionando sem console.
- Desenvolvimento (`dotnet run`, testes) igual ao de hoje.

**Non-Goals:**
- Assinatura digital do instalador, atualização automática, Mac/Linux, instalação para todos os usuários da máquina.
- Escolher a porta na instalação (continua 5080; quem precisar troca no `appsettings.Production.json`), iniciar com o
  Windows, levar a base de quem gera o instalador.

## Decisions

- **Inno Setup para o instalador.** Gratuito e com assistente em português (`BrazilianPortuguese.isl`), páginas
  próprias, atalhos, desinstalador e versão no "Adicionar ou remover programas", tudo num script `.iss`.
  - Alternativas descartadas: ZIP portátil (sem assistente, atalhos nem desinstalador); WiX (XML extenso, pacote do SDK
    no projeto); MSIX (exige certificado confiável na máquina do amigo).
- **Bandeja dentro do próprio `Lotofacil.Web`, com Windows Forms.** O projeto passa a `net10.0-windows` com
  `UseWindowsForms`; os dois projetos de teste também, porque o referenciam. O `NotifyIcon` roda numa thread STA própria
  com um `ApplicationContext`; "Sair" para o `WebApplication` (`StopAsync`), que encerra de forma ordenada.
  - Os `using` globais que o Windows Forms acrescenta (`System.Drawing`, `System.Windows.Forms`) são removidos no
    `.csproj` (`<Using Remove=...>`), para não colidir com nomes do MudBlazor (`Color`, `Size`) nos componentes.
  - Alternativas descartadas: um executável lançador separado, que iniciaria o `Lotofacil.Web.exe` como processo filho
    (dois executáveis, encerrar matando o filho, esperar a porta responder); `Shell_NotifyIcon` por P/Invoke (muito
    código para o mesmo resultado).
- **Modo área de trabalho só fora do ambiente `Development`.** O `Program.cs` decide pelo ambiente:
  - fora de `Development`: raiz de conteúdo na pasta do executável (`AppContext.BaseDirectory`), para o
    `appsettings*.json` e o `wwwroot` serem achados qualquer que seja o diretório atual; instância única; início com
    `StartAsync`; navegador aberto no endereço do Kestrel; ícone na bandeja até "Sair";
  - em `Development`: tudo como hoje.
  - As peças ficam em `Lotofacil.Web/AreaDeTrabalho/`: `InstanciaUnica` (mutex nomeado `Lotofacil.Instancia` na sessão
    do usuário), `IconeBandeja`, `Navegador` (abre a URL pelo shell) e `FalhaInicializacao` (monta a mensagem a partir
    da exceção: porta em uso → "…a porta 5080 já está em uso por outro programa."; outras → "Não foi possível iniciar o
    Lotofácil: <motivo>").
- **Sem console no app instalado.** O `Lotofacil.Web.csproj` usa `OutputType` `WinExe` só quando a propriedade
  `Distribuicao` é `true`, passada pelo script de geração; o build normal continua `Exe`, com console. A propriedade
  global não afeta os projetos de biblioteca, que não a usam.
- **Configuração gravada pelo instalador em `appsettings.Production.json`**, na pasta do programa, com `BancoDados:Arquivo`
  e `Log:Pasta` absolutos dentro da pasta dos dados. `Production` é o ambiente padrão do app publicado, e o
  `Program.cs` já aceita caminhos absolutos; só falta criar a pasta da base antes das migrations (na Infraestrutura,
  em `InicializarBancoDadosAsync`).
  - A pasta dos dados é guardada pelo Inno Setup (`RegisterPreviousData`/`GetPreviousData`) para ser sugerida na
    reinstalação. A pasta de instalação já é lembrada pelo próprio Inno Setup.
  - Alternativa descartada: guardar a pasta no registro e o app ler de lá; seria um caminho a mais além do
    `appsettings`.
- **Desinstalação declarativa** em `[UninstallDelete]`, com a pasta dos dados resolvida na instalação
  (`{code:PastaDados}`): apaga `lotofacil.db`, `-wal`, `-shm` e `-journal`, a subpasta `log` e a pasta só se ficar vazia.
  `AppMutex=Lotofacil.Instancia` faz o instalador e o desinstalador pedirem para fechar o app aberto.
- **Resumo "Pronto pra instalar"** (`UpdateReadyMemo`) com a pasta dos dados logo depois da pasta de instalação, para o
  usuário conferir as duas antes de instalar.
- **Instalação por usuário:** `PrivilegesRequired=lowest`, `DefaultDirName={localappdata}\Programs\Lotofacil`, x64.
- **Publicação self-contained** `win-x64` (não arquivo único: o Inno Setup empacota a pasta), sem
  `appsettings.Development.json` (`CopyToPublishDirectory="Never"`) e só com os recursos de idioma em português
  (`SatelliteResourceLanguages`). O instalador fica com cerca de 47 MB.
- **Versão** em `<Version>` no `Directory.Build.props` (1.0.0); o script a lê e passa ao Inno Setup (`/DVersao=`), que
  a usa no nome do arquivo e no "Adicionar ou remover programas".
- **Script `instalador/gerar-instalador.ps1`:** lê a versão, publica em `artefatos/publicacao`, acha o `ISCC.exe`
  (pastas padrão do Inno Setup) e gera `artefatos/Lotofacil-Setup-<versão>.exe`. `artefatos/` vai para o `.gitignore`.
- **Ícone:** desenho próprio (volante estilizado com bolinhas marcadas, nas cores do app), gerado por
  `instalador/icone/gerar-icone.ps1` com `System.Drawing` do Windows, sem pacote. Saídas versionadas:
  `Lotofacil.Web/lotofacil.ico` (16 a 256 px; `ApplicationIcon`, bandeja e `SetupIconFile`) e
  `wwwroot/favicon.png` (link no `App.razor`). A prévia é aprovada pelo usuário antes do resto.

## Risks / Trade-offs

- [SmartScreen e antivírus avisam sobre instalador sem assinatura] → README com o passo "Mais informações" →
  "Executar assim mesmo".
- [Porta 5080 ocupada na máquina do amigo] → mensagem clara ao iniciar; README explica como trocar a porta no
  `appsettings.Production.json`.
- [Desinstalar apaga os jogos sem perguntar (decisão do usuário)] → só os arquivos do app são apagados; a reinstalação
  por cima não passa pelo desinstalador e mantém os dados.
- [Alvo `net10.0-windows` prende o código ao Windows] → o app já é só para Windows; a lógica continua no Domínio e na
  Aplicação, que não mudam de alvo.
- [Sem navegador padrão configurado] → a falha ao abrir o navegador não derruba o app; uma mensagem mostra o endereço
  para abrir à mão.

## Migration Plan

- Quem gera o instalador instala o Inno Setup 6 uma vez (`winget install JRSoftware.InnoSetup`).
- A base de desenvolvimento continua onde está; nada a migrar.
