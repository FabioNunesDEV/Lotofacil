## MODIFIED Requirements

### Requirement: Acesso à página Meus jogos
A aplicação SHALL oferecer a página "Meus jogos" na rota `/meus-jogos`, acessível por um item "Meus jogos" no menu
lateral, logo abaixo de "Gerar jogos". Sem nenhum jogo cadastrado, a página MUST exibir "Nenhum jogo cadastrado.
Use “Novo jogo” para cadastrar o primeiro." no lugar da lista.

#### Scenario: Abrir pelo menu
- **WHEN** o usuário aciona o item "Meus jogos" do menu lateral
- **THEN** a página "Meus jogos" é exibida com o título "Meus jogos"
- **AND** o item "Meus jogos" do menu aparece como selecionado

#### Scenario: Nenhum jogo cadastrado
- **WHEN** o usuário abre a página sem jogos cadastrados
- **THEN** a mensagem de nenhum jogo cadastrado é exibida
