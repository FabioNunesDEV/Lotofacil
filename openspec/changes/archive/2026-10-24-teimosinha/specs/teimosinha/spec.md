## Purpose

Permite repetir automaticamente, em todo concurso, um grupo de jogos com nome (a Teimosinha), até o usuário
removê-lo, criando os jogos de cada concurso em Meus jogos para serem conferidos como os demais.

## ADDED Requirements

### Requirement: Regras da Teimosinha
Uma Teimosinha SHALL ter um nome (1 a 40 caracteres, sem espaços nas pontas) e um ou mais jogos de 15 a 20 dezenas.
Cada jogo da Teimosinha MUST guardar o concurso a partir do qual vale. Entre as Teimosinhas ativas, o nome MUST ser
único, sem diferenciar maiúsculas e minúsculas, e uma Teimosinha MUST NOT ter dois jogos com as mesmas dezenas. Uma
Teimosinha removida deixa de ser ativa e libera o nome.

#### Scenario: Nome repetido em outra caixa
- **WHEN** existe a Teimosinha ativa "Minhas fixas" e o usuário informa "minhas fixas"
- **THEN** os jogos são acrescentados à Teimosinha "Minhas fixas", e nenhuma outra é criada

#### Scenario: Jogo repetido na Teimosinha
- **WHEN** o usuário acrescenta à Teimosinha "Minhas fixas" um jogo com as mesmas dezenas de um jogo que ela já tem
- **THEN** o jogo não é acrescentado

### Requirement: Marcar Teimosinha no cadastro manual
O diálogo "Novo jogo" SHALL ter o switch "Teimosinha (repetir em todo concurso)", desligado ao abrir, e, com ele
ligado, o campo "Nome da Teimosinha", que sugere os nomes das Teimosinhas ativas e aceita um nome novo. Com o switch
ligado, "Salvar" MUST exigir o nome e MUST aplicar as regras atuais do cadastro ao concurso informado (concurso
futuro, jogo duplicado recusado). Aceito o jogo, ele MUST ser gravado para o concurso informado como jogo da
Teimosinha, e o jogo MUST entrar na Teimosinha com esse concurso como início. Um nome novo cria a Teimosinha; o nome
de uma ativa acrescenta o jogo a ela. A mensagem de sucesso MUST ser "Jogo salvo para o concurso <número> na
Teimosinha <nome>.". Com o switch desligado, o cadastro continua como hoje.

#### Scenario: Nova Teimosinha pelo cadastro
- **WHEN** o próximo concurso é o 3813 e o usuário marca um jogo, liga o switch, informa "Minhas fixas" e salva
- **THEN** a página exibe "Jogo salvo para o concurso 3813 na Teimosinha Minhas fixas."
- **AND** a Teimosinha "Minhas fixas" passa a existir com esse jogo, desde o concurso 3813, e o jogo aparece em Meus
  jogos no concurso 3813

#### Scenario: Nome obrigatório
- **WHEN** o usuário liga o switch e deixa o nome vazio
- **THEN** "Salvar" fica desabilitado

#### Scenario: Recusa mantém a Teimosinha como estava
- **WHEN** o jogo já está cadastrado para o concurso informado
- **THEN** o cadastro é recusado com a mensagem atual e a Teimosinha não muda

### Requirement: Marcar Teimosinha em Gerar jogos
A lista de jogos gerados SHALL ter, junto de "Salvar jogos", o switch "Teimosinha (repetir em todo concurso)",
desligado ao abrir, e, com ele ligado, o campo "Nome da Teimosinha", com as mesmas regras do cadastro manual. Com o
switch ligado, "Salvar jogos" MUST exigir o nome, gravar para o concurso do formulário os jogos aceitos (duplicados no
concurso continuam recusados, assim como os que já fazem parte da Teimosinha) e acrescentar os jogos gravados à
Teimosinha, com esse concurso como início. Quando algum jogo for gravado, o resumo do salvamento MUST trazer " na
Teimosinha <nome>" logo depois do número do concurso (por exemplo, "3 jogos salvos para o concurso 3813 na
Teimosinha Geradas.").

#### Scenario: Gerar e salvar como Teimosinha
- **WHEN** o usuário gera 3 jogos para o concurso 3813, liga o switch, informa "Geradas" e salva
- **THEN** os 3 jogos são gravados para o concurso 3813 e a Teimosinha "Geradas" passa a ter os 3 jogos desde o
  3813

### Requirement: Jogos criados a cada concurso
Para cada Teimosinha ativa, o sistema SHALL criar em Meus jogos, para cada concurso desde o último criado por ela
até o próximo concurso (último gravado + 1), um jogo para cada jogo da Teimosinha cujo início seja até aquele
concurso, ligado à Teimosinha. Concursos pulados (quando a atualização grava vários concursos de uma vez) MUST ser
preenchidos, mesmo já sorteados: é a única exceção à regra de jogos só para concursos futuros, porque a Teimosinha
vale para todo concurso. Na atualização, a criação MUST acontecer antes do aviso de jogos premiados, para os jogos
criados dos concursos já sorteados entrarem nele. Um jogo com as mesmas dezenas já cadastrado para o concurso MUST ser pulado, sem erro. A criação SHALL
rodar ao iniciar a aplicação e depois de cada atualização ou importação dos resultados que gravar concursos, nas
páginas Resultados e Painel. Depois de uma atualização, quando algum jogo for criado, a página MUST exibir
"Teimosinhas: <quantidade> criados para o concurso <número>." ou, para vários concursos, "Teimosinhas: <quantidade>
criados para os concursos <primeiro> a <último>.", com a quantidade no formato "1 jogo" ou "<n> jogos". Os jogos
criados são jogos comuns: conferidos, incluídos no aviso de jogos premiados e na exportação, e podem ser excluídos
em Meus jogos sem afetar a Teimosinha.

#### Scenario: Próximo concurso
- **WHEN** a Teimosinha "Minhas fixas", com 2 jogos desde o 3813, já criou os jogos do 3813, e a atualização grava o
  concurso 3813
- **THEN** os 2 jogos são criados para o concurso 3814
- **AND** a página exibe "Teimosinhas: 2 jogos criados para o concurso 3814."

#### Scenario: Concursos pulados
- **WHEN** a Teimosinha com 1 jogo criou o jogo do 3813, e uma atualização grava os concursos 3813 a 3815
- **THEN** o jogo é criado para os concursos 3814, 3815 e 3816
- **AND** a página exibe "Teimosinhas: 3 jogos criados para os concursos 3814 a 3816."

#### Scenario: Jogo já cadastrado
- **WHEN** o usuário já cadastrou à mão, para o 3814, as mesmas dezenas de um jogo da Teimosinha
- **THEN** a criação do 3814 pula esse jogo e cria os demais

#### Scenario: Jogo acrescentado depois
- **WHEN** a Teimosinha criou os jogos até o 3814 e o usuário acrescenta um jogo a ela com início no 3816
- **THEN** a criação do 3815 não inclui esse jogo, e a do 3816 inclui

#### Scenario: Conferência como jogo comum
- **WHEN** o concurso 3814 é sorteado e um jogo da Teimosinha nele acerta 11 dezenas
- **THEN** o jogo aparece premiado em Meus jogos e no aviso de jogos premiados da atualização

### Requirement: Página Teimosinhas
A aplicação SHALL oferecer a página "Teimosinhas" na rota `/teimosinhas`, acessível por um item "Teimosinhas" no menu
lateral, logo abaixo de "Meus jogos". A página MUST listar as Teimosinhas ativas em ordem de nome, cada uma com o
nome, "Desde o concurso <número>" (o menor início entre os jogos), "<quantidade> · <custo> por concurso" (soma das
apostas de 15 de cada jogo × preço da aposta mínima atual) e os jogos com as dezenas. Sem Teimosinhas ativas, a
página MUST exibir "Nenhuma Teimosinha ativa. Ligue “Teimosinha” ao salvar jogos em Meus jogos ou em Gerar jogos.".

#### Scenario: Lista
- **WHEN** existe a Teimosinha "Minhas fixas" com 1 jogo de 15 e 1 de 16 dezenas desde o 3813 e o preço é R$ 3,50
- **THEN** a página mostra "Minhas fixas", "Desde o concurso 3813", "2 jogos · R$ 59,50 por concurso" e os dois jogos

#### Scenario: Nenhuma ativa
- **WHEN** não há Teimosinha ativa
- **THEN** a página exibe a mensagem de nenhuma Teimosinha ativa

### Requirement: Remover Teimosinha
Cada Teimosinha da página SHALL ter o botão "Remover", que pede confirmação com "Remover a Teimosinha <nome>? Os jogos
já criados continuam em Meus jogos.". Confirmada, a Teimosinha MUST deixar de ser ativa, nenhum jogo novo MUST ser
criado por ela, os jogos já criados MUST continuar em Meus jogos com a marca, e a página MUST exibir "Teimosinha
removida.". Cancelada, nada muda.

#### Scenario: Remover
- **WHEN** o usuário remove a Teimosinha "Minhas fixas", que criou os jogos até o 3814, e confirma
- **THEN** ela sai da lista, e a próxima atualização não cria jogos dela para o 3815
- **AND** os jogos do 3813 e do 3814 continuam em Meus jogos

### Requirement: Marca em Meus jogos
Em Meus jogos, cada jogo criado por uma Teimosinha SHALL mostrar a marca "Teimosinha · <nome>", também depois de a
Teimosinha ser removida.

#### Scenario: Marca
- **WHEN** o concurso 3813 tem um jogo da Teimosinha "Minhas fixas" e um jogo cadastrado à mão
- **THEN** só o primeiro mostra "Teimosinha · Minhas fixas"
