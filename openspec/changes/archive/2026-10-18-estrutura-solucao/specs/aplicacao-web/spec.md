## Purpose

Define como a aplicação é executada e acessada pelo usuário: somente na própria máquina, com um layout principal
de navegação que recebe as telas das demais funcionalidades.

## ADDED Requirements

### Requirement: Execução somente local
A aplicação SHALL aceitar conexões apenas pelo endereço `localhost`, em uma porta fixa definida em configuração,
e MUST NOT ficar acessível por outros endereços de rede da máquina.

#### Scenario: Acesso pela própria máquina
- **WHEN** o usuário inicia a aplicação e abre `http://localhost:<porta configurada>` no navegador
- **THEN** a página inicial é exibida

#### Scenario: Acesso por outro endereço da máquina
- **WHEN** alguém tenta acessar a aplicação pelo IP da máquina na rede local
- **THEN** a conexão é recusada

### Requirement: Layout principal
Toda página da aplicação SHALL ser exibida dentro do layout principal, composto por uma barra superior com o
título "Lotofácil" e um menu lateral de navegação. O menu MUST listar apenas as páginas já disponíveis.

#### Scenario: Layout exibido na página inicial
- **WHEN** o usuário abre a página inicial
- **THEN** a barra superior mostra o título "Lotofácil"
- **AND** o menu lateral mostra o item "Painel"

#### Scenario: Recolher o menu lateral
- **WHEN** o usuário aciona o botão de menu da barra superior
- **THEN** o menu lateral é recolhido, e acioná-lo de novo volta a exibi-lo

### Requirement: Página inicial
A rota raiz (`/`) SHALL exibir a página "Painel", que nesta versão contém apenas o seu título.

#### Scenario: Abrir a raiz
- **WHEN** o usuário navega para `/`
- **THEN** a página "Painel" é exibida com o título "Painel"
- **AND** o item "Painel" do menu aparece como selecionado

### Requirement: Página não encontrada
Uma rota inexistente SHALL exibir, dentro do layout principal, a mensagem "Página não encontrada" com um link de
volta para o Painel.

#### Scenario: Rota inexistente
- **WHEN** o usuário navega para uma rota que não existe
- **THEN** a mensagem "Página não encontrada" é exibida dentro do layout principal
- **AND** há um link que leva ao Painel

### Requirement: Registro de erros em arquivo
A aplicação SHALL gravar em arquivo, na pasta `log` (configurável, relativa à pasta da aplicação), as mensagens de
nível Erro ou mais grave, com data e hora, origem, mensagem e detalhes da exceção. Mensagens de nível inferior a
Erro MUST NOT ser gravadas. Deve existir um arquivo por dia, e a pasta SHALL ser criada automaticamente se não
existir.

#### Scenario: Erro gravado
- **WHEN** ocorre um erro registrado pela aplicação, com ou sem exceção
- **THEN** o arquivo do dia na pasta `log` recebe uma linha com data e hora, nível, origem e mensagem
- **AND** se houver exceção, o tipo, a mensagem e a pilha de chamadas são gravados logo abaixo

#### Scenario: Mensagem informativa ou de aviso
- **WHEN** a aplicação registra uma mensagem de nível Informação ou Aviso
- **THEN** nada é gravado no arquivo de log

#### Scenario: Pasta de log inexistente
- **WHEN** a aplicação registra um erro e a pasta `log` ainda não existe
- **THEN** a pasta é criada e o erro é gravado
