## MODIFIED Requirements

### Requirement: Cabeçalho e atalhos do Painel
A página Painel SHALL mostrar o título "Painel", o subtítulo "Resumo da base local e dos seus jogos" e os botões
"Atualizar resultados", "Gerar jogos" e "Novo jogo". "Gerar jogos" MUST levar à página Gerar jogos. "Novo jogo"
MUST levar à página Meus jogos com o cadastro de jogo já aberto.

#### Scenario: Abrir o Painel
- **WHEN** o usuário abre a rota `/`
- **THEN** a página mostra o título "Painel", o subtítulo e os botões "Atualizar resultados", "Gerar jogos" e "Novo
  jogo"

#### Scenario: Atalho Gerar jogos
- **WHEN** o usuário aciona "Gerar jogos" no Painel
- **THEN** a página Gerar jogos é exibida

#### Scenario: Atalho Novo jogo
- **WHEN** o usuário aciona "Novo jogo" no Painel
- **THEN** a página Meus jogos é exibida com o cadastro de jogo aberto
