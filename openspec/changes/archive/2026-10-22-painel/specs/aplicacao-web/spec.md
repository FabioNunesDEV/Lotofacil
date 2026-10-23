## MODIFIED Requirements

### Requirement: Página inicial
A rota raiz (`/`) SHALL exibir a página "Painel", com o título "Painel" e o resumo da base local e dos jogos
definido na capacidade `painel`.

#### Scenario: Abrir a raiz
- **WHEN** o usuário navega para `/`
- **THEN** a página "Painel" é exibida com o título "Painel"
- **AND** o item "Painel" do menu aparece como selecionado
