## 1. Ícone

- [x] 1.1 `instalador/icone/gerar-icone.ps1` (desenho próprio, sem marca da Caixa) gerando `lotofacil.ico` (16 a
  256 px) e `wwwroot/favicon.png`; verificar mostrando a prévia ao usuário e só seguir com a aprovação dele

## 2. Caminhos absolutos

- [x] 2.1 `InicializarBancoDadosAsync` cria a pasta da base antes das migrations; verificar com teste de integração:
  base num caminho absoluto em pasta inexistente (pasta temporária) é criada vazia com a estrutura atual
- [x] 2.2 Log com pasta absoluta; verificar com teste de integração: erro registrado com `ConfiguracaoLog` numa pasta
  absoluta inexistente grava o arquivo do dia nela

## 3. Modo área de trabalho

- [x] 3.1 `Lotofacil.Web` e os dois projetos de teste em `net10.0-windows`, `UseWindowsForms` no Web sem os `using`
  globais do Windows Forms, `OutputType` `WinExe` com `Distribuicao=true`, `ApplicationIcon`, favicon no `App.razor` e
  `appsettings.Development.json` fora da publicação; verificar com build sem avisos e testes atuais passando
- [x] 3.2 `FalhaInicializacao` com as mensagens de porta em uso e de outras falhas; verificar com testes de unidade
  (porta 5080 em uso, pasta sem permissão, exceção interna)
- [x] 3.3 `InstanciaUnica`, `Navegador`, `IconeBandeja` e o `Program.cs` com o modo área de trabalho fora de
  `Development` (raiz de conteúdo na pasta do executável, início com `StartAsync`, navegador, bandeja, "Sair" com
  `StopAsync`, mensagem de falha); verificar com o app publicado rodando em `Production` numa cópia da base: sem
  console, navegador no Painel, ícone com "Abrir o Lotofácil"/"Sair", segunda abertura só abre o navegador, porta
  ocupada mostra a mensagem, e `dotnet run` em `Development` igual a antes

## 4. Instalador

- [x] 4.1 Instalar o Inno Setup 6 (`winget install JRSoftware.InnoSetup`); `<Version>1.0.0</Version>` no
  `Directory.Build.props`; `instalador/Lotofacil.iss` (assistente em português, pasta de instalação por usuário, página
  da pasta dos dados, atalhos, `appsettings.Production.json`, `AppMutex`, `[UninstallDelete]`, "Abrir o Lotofácil");
  `instalador/gerar-instalador.ps1`; `artefatos/` no `.gitignore`; verificar gerando
  `artefatos/Lotofacil-Setup-1.0.0.exe` com `git status` limpo dos gerados
- [x] 4.2 Verificação manual do instalador nesta máquina: instalar com as pastas padrão e abrir; instalar de novo por
  cima (pastas sugeridas, dados mantidos); instalar com o app aberto (pede para fechar); pasta dos dados numa pasta de
  teste com um arquivo alheio e uma base existente; desinstalar (programa, atalhos e dados do app apagados, arquivo
  alheio mantido); versão e ícone no "Adicionar ou remover programas"

## 5. Documentação e verificação final

- [x] 5.1 `CLAUDE.md` (Inno Setup como ferramenta autorizada, alvo `net10.0-windows`, decisões da distribuição) e
  `README.md` (gerar o instalador, instalar, aviso do SmartScreen, trocar a porta); verificar relendo os trechos
- [x] 5.2 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
