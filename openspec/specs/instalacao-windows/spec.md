# instalacao-windows Specification

## Purpose

Permite distribuir o Lotofácil para outras pessoas instalarem no Windows com um instalador comum, configurando na
instalação as pastas do programa e dos dados, e usar o app instalado em segundo plano, com um ícone na bandeja.

## Requirements

### Requirement: Instalador para Windows
O sistema SHALL ser distribuído como um único arquivo `Lotofacil-Setup-<versão>.exe` para Windows 64 bits, que
instala o programa só para o usuário atual, sem pedir senha de administrador. O programa instalado MUST rodar sem
que o usuário instale o .NET ou qualquer outro componente antes.

#### Scenario: Instalação numa máquina sem .NET
- **WHEN** um usuário sem o .NET instalado executa o instalador e conclui o assistente
- **THEN** o Lotofácil é instalado e abre sem pedir a instalação de outro componente

#### Scenario: Sem senha de administrador
- **WHEN** um usuário comum executa o instalador
- **THEN** o assistente não pede a senha de administrador

### Requirement: Configuração na instalação
O assistente SHALL perguntar, nesta ordem:
- a pasta de instalação, com o padrão `%LOCALAPPDATA%\Programs\Lotofacil`;
- a pasta dos dados, com o padrão `%LOCALAPPDATA%\Lotofacil`, explicada como "onde ficam seus jogos, os resultados
  baixados e o registro de erros";
- se cria o atalho na Área de trabalho (marcado por padrão).

O atalho no Menu Iniciar SHALL ser criado sempre. Ao terminar, o assistente SHALL oferecer "Abrir o Lotofácil"
(marcado por padrão). O programa instalado MUST guardar a base em `lotofacil.db` e o registro de erros na subpasta
`log`, ambos dentro da pasta dos dados escolhida. Se a pasta dos dados já tiver um `lotofacil.db`, essa base MUST ser
usada, sem ser apagada nem substituída.

#### Scenario: Pastas padrão
- **WHEN** o usuário conclui o assistente sem trocar as pastas
- **THEN** o programa fica em `%LOCALAPPDATA%\Programs\Lotofacil`
- **AND** ao abrir o app, a base é criada em `%LOCALAPPDATA%\Lotofacil\lotofacil.db`

#### Scenario: Pasta dos dados escolhida
- **WHEN** o usuário escolhe `D:\Loteria` como pasta dos dados e registra um jogo no app
- **THEN** o jogo fica gravado em `D:\Loteria\lotofacil.db`, e os erros, em `D:\Loteria\log`

#### Scenario: Base existente na pasta escolhida
- **WHEN** a pasta dos dados escolhida já tem um `lotofacil.db` com concursos e jogos
- **THEN** o app abre com esses concursos e jogos

#### Scenario: Sem atalho na Área de trabalho
- **WHEN** o usuário desmarca o atalho na Área de trabalho
- **THEN** só o atalho do Menu Iniciar é criado

### Requirement: Reinstalação e atualização
Ao executar o instalador numa máquina onde o Lotofácil já está instalado, o assistente SHALL sugerir a pasta de
instalação e a pasta dos dados escolhidas antes, e a instalação MUST manter a base e o registro de erros. Com o app
aberto, o instalador SHALL pedir que ele seja fechado antes de continuar.

#### Scenario: Versão nova por cima
- **WHEN** o usuário instala uma versão nova sobre a anterior, sem trocar as pastas
- **THEN** o app abre com os mesmos concursos, jogos, Teimosinhas e configurações de antes

#### Scenario: App aberto durante a instalação
- **WHEN** o usuário executa o instalador com o app aberto
- **THEN** o instalador avisa que o Lotofácil está aberto e pede para fechá-lo antes de continuar

### Requirement: Desinstalação
A desinstalação, pelo "Adicionar ou remover programas" do Windows ou pelo Menu Iniciar, SHALL remover o programa e os
atalhos e MUST apagar, sem perguntar, os dados do app na pasta dos dados: a base (`lotofacil.db` e os seus arquivos
auxiliares) e a subpasta `log`. Outros arquivos da pasta dos dados MUST NOT ser apagados; a pasta só SHALL ser removida
se ficar vazia. Com o app aberto, o desinstalador SHALL pedir que ele seja fechado antes de continuar.

#### Scenario: Desinstalar com a pasta padrão
- **WHEN** o usuário desinstala o Lotofácil instalado com as pastas padrão
- **THEN** as pastas `%LOCALAPPDATA%\Programs\Lotofacil` e `%LOCALAPPDATA%\Lotofacil` deixam de existir
- **AND** os atalhos somem do Menu Iniciar e da Área de trabalho

#### Scenario: Outros arquivos na pasta dos dados
- **WHEN** a pasta dos dados é `D:\Loteria`, que também tem `planilha.xlsx`, e o usuário desinstala o Lotofácil
- **THEN** a base e a subpasta `log` são apagadas
- **AND** `D:\Loteria\planilha.xlsx` e a pasta `D:\Loteria` continuam existindo

### Requirement: Execução em segundo plano com ícone na bandeja
O app instalado SHALL abrir sem janela de console: ao ser aberto, inicia o servidor local e abre o navegador padrão no
endereço local do app. As páginas do app instalado MUST ser interativas como no ambiente de desenvolvimento: botões,
campos, menus e diálogos respondem e as operações mostram o andamento e o desfecho. Enquanto estiver aberto, SHALL
mostrar um ícone na área de notificação do Windows (bandeja), com a dica "Lotofácil" e um menu com "Abrir o
Lotofácil", que abre o navegador no endereço do app, e "Sair", que encerra o servidor e remove o ícone. O clique duplo
no ícone SHALL abrir o navegador. Fechar o navegador MUST NOT encerrar o app.

#### Scenario: Abrir o app
- **WHEN** o usuário abre o Lotofácil pelo atalho
- **THEN** nenhuma janela de console aparece
- **AND** o navegador abre no Painel
- **AND** o ícone do Lotofácil aparece na bandeja

#### Scenario: Importar o histórico no app instalado
- **WHEN** o usuário, no app instalado com a base vazia, clica em "Importar histórico" na página Resultados
- **THEN** a página mostra o andamento da importação
- **AND** ao terminar, mostra a mensagem do desfecho e lista os concursos gravados

#### Scenario: Reabrir pelo ícone
- **WHEN** o usuário fecha o navegador e escolhe "Abrir o Lotofácil" no ícone da bandeja
- **THEN** o navegador abre de novo no Painel

#### Scenario: Sair
- **WHEN** o usuário escolhe "Sair" no ícone da bandeja
- **THEN** o ícone some e o endereço local deixa de responder

### Requirement: Instância única
O app instalado SHALL rodar uma só vez por usuário. Abrir o Lotofácil com ele já aberto MUST só abrir o navegador no
endereço do app, sem iniciar outro servidor nem outro ícone na bandeja.

#### Scenario: Abrir de novo
- **WHEN** o app já está aberto e o usuário clica de novo no atalho
- **THEN** o navegador abre no Painel
- **AND** continua existindo um só ícone do Lotofácil na bandeja

### Requirement: Falha ao iniciar
Quando o app instalado não conseguir iniciar, SHALL mostrar uma mensagem com o motivo, registrar o erro no log
(quando a pasta de log puder ser gravada) e encerrar, sem deixar ícone na bandeja.

#### Scenario: Porta ocupada
- **WHEN** outro programa já usa a porta configurada do app e o usuário abre o Lotofácil
- **THEN** aparece a mensagem "Não foi possível iniciar o Lotofácil: a porta 5080 já está em uso por outro programa."
- **AND** o app encerra

#### Scenario: Pasta dos dados sem permissão
- **WHEN** a pasta dos dados não pode ser gravada e o usuário abre o Lotofácil
- **THEN** aparece uma mensagem "Não foi possível iniciar o Lotofácil: " seguida do motivo
- **AND** o app encerra

### Requirement: Ícone do aplicativo
O Lotofácil SHALL ter um ícone próprio, usado no executável, nos atalhos, no instalador, no "Adicionar ou remover
programas", na bandeja e na aba do navegador. O ícone MUST NOT usar a marca, o logotipo ou o trevo da Caixa.

#### Scenario: Ícone nos lugares do Windows
- **WHEN** o Lotofácil é instalado e aberto
- **THEN** o mesmo ícone aparece no atalho, na bandeja e na aba do navegador

### Requirement: Modo de desenvolvimento inalterado
Executada no ambiente de desenvolvimento, a aplicação SHALL funcionar como antes desta mudança: com console, base e log
nos caminhos da configuração de desenvolvimento, sem ícone na bandeja, sem verificar instância única e sem abrir o
navegador por conta própria.

#### Scenario: dotnet run
- **WHEN** o desenvolvedor executa a aplicação com o ambiente `Development`
- **THEN** as mensagens aparecem no console e nenhum ícone aparece na bandeja

### Requirement: Geração do instalador
O repositório SHALL ter um comando que, a partir do código, publica o app para Windows 64 bits com o .NET incluído e
gera `Lotofacil-Setup-<versão>.exe`, onde `<versão>` é a versão do app. O instalador mostra essa versão no
"Adicionar ou remover programas". Os arquivos gerados MUST NOT ser versionados no Git. Quando a publicação não tiver
os arquivos de que a interface precisa para ser interativa no navegador, o comando MUST parar com uma mensagem de erro
que diga o que falta, sem gerar o instalador.

#### Scenario: Gerar a versão 1.0.0
- **WHEN** a versão do app é 1.0.0 e o desenvolvedor executa o comando de geração
- **THEN** é gerado `Lotofacil-Setup-1.0.0.exe`
- **AND** o `git status` não mostra os arquivos gerados

#### Scenario: Publicação sem o script da interface
- **WHEN** a publicação feita pelo comando de geração não tem o script que torna as páginas interativas
- **THEN** o comando termina com erro, dizendo qual arquivo falta na publicação
- **AND** nenhum instalador novo é gerado
