## MODIFIED Requirements

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
