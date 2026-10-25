## Why

Hoje o Lotofácil só roda na máquina de quem tem o código-fonte e o SDK do .NET, com `dotnet run`. O usuário quer
distribuir o programa para amigos, que não são programadores, instalarem no Windows com um instalador comum, escolhendo
na instalação o que for preciso configurar.

## What Changes

- Instalador para Windows 64 bits (`Lotofacil-Setup-<versão>.exe`), gerado com o Inno Setup, sem pedir senha de
  administrador (instalação só para o usuário). O .NET vai junto: o amigo não precisa instalar nada antes.
- Assistente da instalação com as configurações do app: pasta de instalação, pasta dos dados (base e log), atalho na
  Área de trabalho e, no fim, "Abrir o Lotofácil". O instalador grava as escolhas na configuração do app.
- Reinstalar (versão nova) traz as pastas escolhidas antes e mantém os dados. Com o app aberto, o instalador pede para
  fechá-lo.
- Desinstalar remove o programa, os atalhos e os dados do app (base e log), sem apagar outros arquivos da pasta
  escolhida.
- App instalado roda em segundo plano, sem janela de console: ao abrir, inicia o servidor local e abre o navegador;
  um ícone na bandeja (perto do relógio) tem "Abrir o Lotofácil" e "Sair". Abrir de novo com o app já aberto só abre o
  navegador. Falha ao iniciar (porta ocupada, pasta sem permissão) aparece numa mensagem e no log.
- Ícone próprio do aplicativo (sem a marca da Caixa), usado no executável, nos atalhos, no instalador, na bandeja e na
  aba do navegador.
- Script para gerar o instalador a partir do código, com a versão do app no nome do arquivo.
- A base e a pasta de log aceitam caminho absoluto na configuração, além do caminho relativo à pasta do app.
- O modo de desenvolvimento (`dotnet run`) continua como hoje: console, base na pasta do projeto, sem ícone na
  bandeja.

## Capabilities

### New Capabilities
- `instalacao-windows`: instalador e desinstalador para Windows, configuração na instalação, execução em segundo
  plano com ícone na bandeja, instância única, ícone do aplicativo e geração do instalador.

### Modified Capabilities
- `aplicacao-web`: "Registro de erros em arquivo" passa a aceitar pasta de log com caminho absoluto.
- `concursos`: "Base local criada automaticamente" passa a aceitar caminho absoluto para o arquivo da base.

## Impact

- Web: `Lotofacil.Web` passa a mirar `net10.0-windows` com Windows Forms (ícone da bandeja e mensagens), e os dois
  projetos de teste, que o referenciam, também. Inicialização em modo área de trabalho fora do ambiente de
  desenvolvimento; ícone `.ico` e favicon.
- Instalador: pasta nova `instalador/` com o script do Inno Setup, o script de geração e o desenho do ícone; arquivos
  gerados fora do Git.
- Ferramenta nova, autorizada pelo usuário: Inno Setup (instalado na máquina de quem gera o instalador; não é pacote
  NuGet). Nenhum pacote novo.
- Documentação: `CLAUDE.md` (Inno Setup, alvo Windows, decisões da distribuição) e `README.md` (como gerar e como
  instalar, incluindo o aviso do SmartScreen por o instalador não ser assinado).
