## ADDED Requirements

### Requirement: Abrir o cadastro pelo endereço
Quando a página Meus jogos for aberta pelo endereço `/meus-jogos?novo=true` (destino do atalho "Novo jogo" do
Painel), ela SHALL abrir o cadastro de jogo logo após carregar, igual ao botão "Novo jogo" da página. Depois de
aberto, o endereço MUST voltar a ser `/meus-jogos`, para que recarregar a página não abra o cadastro de novo. Ao
salvar ou cancelar, o usuário MUST continuar na página Meus jogos.

#### Scenario: Abrir pelo atalho do Painel
- **WHEN** o usuário abre `/meus-jogos?novo=true` com o 3808 como último concurso gravado
- **THEN** a página Meus jogos é exibida com o cadastro aberto e o campo "Concurso" com 3809
- **AND** o endereço passa a ser `/meus-jogos`

#### Scenario: Salvar pelo atalho
- **WHEN** o usuário salva um jogo no cadastro aberto pelo atalho
- **THEN** a página exibe "Jogo salvo para o concurso <número>." e o jogo aparece na lista de Meus jogos

#### Scenario: Abrir sem o parâmetro
- **WHEN** o usuário abre `/meus-jogos`
- **THEN** a página é exibida sem abrir o cadastro
