## MODIFIED Requirements

### Requirement: Acesso à página Configurações
A aplicação SHALL oferecer a página "Configurações" na rota `/configuracoes`, acessível por um item "Configurações"
no menu lateral, o último da lista, logo abaixo de "Teimosinhas". A página MUST ter o título "Configurações" e o
subtítulo "Valores usados nos cálculos".

#### Scenario: Abrir pelo menu
- **WHEN** o usuário aciona o item "Configurações" do menu lateral
- **THEN** a página "Configurações" é exibida com o título "Configurações"
- **AND** o item "Configurações" do menu aparece como selecionado
