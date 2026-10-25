# aplicacao-web Specification

## Purpose

Define como a aplicação é executada e acessada pelo usuário: somente na própria máquina, com um layout principal
de navegação que recebe as telas das demais funcionalidades.

## Requirements

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

O botão de menu da barra superior SHALL alternar o menu lateral entre aberto e recolhido, e o menu MUST começar
aberto ao carregar a página. Em telas com 960 px de largura ou mais, o menu recolhido MUST continuar visível como
uma faixa estreita, só com o ícone de cada página, e cada ícone MUST continuar levando à sua página; passar o mouse
sobre um ícone do menu recolhido MUST mostrar uma dica com o nome da página, e com o menu aberto a dica MUST NOT
aparecer. Em telas mais estreitas, o menu recolhido MUST sumir por inteiro, e o menu aberto aparece por cima da
página.

#### Scenario: Layout exibido na página inicial
- **WHEN** o usuário abre a página inicial
- **THEN** a barra superior mostra o título "Lotofácil"
- **AND** o menu lateral mostra o item "Painel"

#### Scenario: Recolher o menu lateral
- **WHEN** o usuário aciona o botão de menu da barra superior
- **THEN** o menu lateral é recolhido, e acioná-lo de novo volta a exibi-lo

#### Scenario: Menu recolhido mostra só os ícones
- **WHEN** a janela tem 1536 px de largura e o usuário recolhe o menu
- **THEN** o menu continua visível como uma faixa estreita, com o ícone de cada página e sem os textos
- **AND** acionar o ícone de "Meus jogos" abre a página Meus jogos

#### Scenario: Dica com o nome no menu recolhido
- **WHEN** o menu está recolhido e o usuário passa o mouse sobre o ícone de "Estatísticas"
- **THEN** aparece a dica "Estatísticas"

#### Scenario: Sem dica com o menu aberto
- **WHEN** o menu está aberto e o usuário passa o mouse sobre o item "Estatísticas"
- **THEN** nenhuma dica aparece

#### Scenario: Celular
- **WHEN** a janela tem 390 px de largura e o menu está recolhido
- **THEN** o menu não aparece, e a página ocupa toda a largura

### Requirement: Página inicial
A rota raiz (`/`) SHALL exibir a página "Painel", com o título "Painel" e o resumo da base local e dos jogos
definido na capacidade `painel`.

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
A aplicação SHALL gravar em arquivo, na pasta `log` (configurável, relativa à pasta da aplicação ou com caminho
absoluto), as mensagens de nível Erro ou mais grave, com data e hora, origem, mensagem e detalhes da exceção.
Mensagens de nível inferior a Erro MUST NOT ser gravadas. Deve existir um arquivo por dia, e a pasta SHALL ser criada
automaticamente se não existir.

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

#### Scenario: Pasta de log com caminho absoluto
- **WHEN** a configuração aponta a pasta de log para `D:\Dados\Lotofacil\log` e a aplicação registra um erro
- **THEN** o erro é gravado nessa pasta, e não na pasta da aplicação
