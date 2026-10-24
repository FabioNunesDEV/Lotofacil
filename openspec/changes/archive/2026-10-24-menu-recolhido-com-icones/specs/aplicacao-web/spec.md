## MODIFIED Requirements

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
