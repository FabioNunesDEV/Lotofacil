# jogos Specification

## Purpose

Permite ao usuário cadastrar os jogos que vai apostar em concursos futuros e, depois do sorteio, conferir os
acertos e o prêmio de cada jogo pelo rateio gravado do próprio concurso.

## Requirements

### Requirement: Acesso à página Meus jogos
A aplicação SHALL oferecer a página "Meus jogos" na rota `/meus-jogos`, acessível por um item "Meus jogos" no menu
lateral, logo abaixo de "Estatísticas". Sem nenhum jogo cadastrado, a página MUST exibir "Nenhum jogo cadastrado.
Use “Novo jogo” para cadastrar o primeiro." no lugar da lista.

#### Scenario: Abrir pelo menu
- **WHEN** o usuário aciona o item "Meus jogos" do menu lateral
- **THEN** a página "Meus jogos" é exibida com o título "Meus jogos"
- **AND** o item "Meus jogos" do menu aparece como selecionado

#### Scenario: Nenhum jogo cadastrado
- **WHEN** o usuário abre a página sem jogos cadastrados
- **THEN** a mensagem de nenhum jogo cadastrado é exibida

### Requirement: Regras de um jogo
Um jogo SHALL ter o número do concurso a que se destina (maior que zero) e exatamente 15 dezenas distintas, cada uma
entre 01 e 25, guardadas em ordem crescente. Um jogo que não atenda a essas regras MUST ser rejeitado.

#### Scenario: Dezenas fora de ordem
- **WHEN** um jogo é criado com as dezenas 25, 03, 14, ... (fora de ordem)
- **THEN** o jogo guarda as dezenas em ordem crescente

#### Scenario: Quantidade diferente de 15
- **WHEN** um jogo é criado com 14 ou 16 dezenas
- **THEN** a criação é rejeitada

#### Scenario: Dezena repetida ou fora do volante
- **WHEN** um jogo é criado com uma dezena repetida, ou com a dezena 0 ou 26
- **THEN** a criação é rejeitada

### Requirement: Cadastro de jogo
A página SHALL ter o botão "Novo jogo", que abre o cadastro com:
- o campo "Concurso", já preenchido com o número do último concurso gravado + 1;
- um volante com as dezenas 01 a 25, em que cada clique marca ou desmarca a dezena, sem permitir marcar mais de 15;
- a contagem "<marcadas> de 15 dezenas";
- o botão "Salvar", habilitado somente com 15 dezenas marcadas.

Ao salvar, o sistema MUST gravar o jogo e exibir "Jogo salvo para o concurso <número>.", ou recusar com a mensagem:
- base sem concursos: "Importe o histórico na página Resultados antes de cadastrar jogos.";
- concurso igual ou anterior ao último gravado: "Só é possível cadastrar jogos para concursos depois do <último
  gravado>.";
- mesmo concurso com as mesmas 15 dezenas já cadastrado: "Este jogo já está cadastrado para o concurso <número>.".

#### Scenario: Concurso sugerido
- **WHEN** o último concurso gravado é o 3808 e o usuário aciona "Novo jogo"
- **THEN** o campo "Concurso" vem com 3809

#### Scenario: Marcar dezenas
- **WHEN** o usuário marca 14 dezenas no volante
- **THEN** a contagem mostra "14 de 15 dezenas" e "Salvar" fica desabilitado
- **AND** ao marcar a 15ª, "Salvar" é habilitado e as demais dezenas não podem ser marcadas

#### Scenario: Salvar jogo
- **WHEN** o usuário salva 15 dezenas para o concurso 3809, com o 3808 como último gravado
- **THEN** o jogo é gravado, a página exibe "Jogo salvo para o concurso 3809." e o jogo aparece no grupo do
  concurso 3809

#### Scenario: Concurso já sorteado
- **WHEN** o usuário tenta salvar um jogo para o concurso 3808, que é o último gravado
- **THEN** o jogo não é gravado e a página exibe "Só é possível cadastrar jogos para concursos depois do 3808."

#### Scenario: Jogo duplicado
- **WHEN** o usuário salva para o concurso 3809 as mesmas 15 dezenas de um jogo já cadastrado para o 3809
- **THEN** o jogo não é gravado e a página exibe "Este jogo já está cadastrado para o concurso 3809."

#### Scenario: Mesmas dezenas em outro concurso
- **WHEN** o usuário salva para o concurso 3810 as mesmas dezenas de um jogo do concurso 3809
- **THEN** o jogo é gravado normalmente

#### Scenario: Base vazia
- **WHEN** a base não tem concursos e o usuário tenta salvar um jogo
- **THEN** o jogo não é gravado e a página exibe "Importe o histórico na página Resultados antes de cadastrar
  jogos."

### Requirement: Conferência do jogo
Quando o concurso de um jogo estiver gravado na base, o sistema SHALL conferir o jogo: os acertos são as dezenas do
jogo que estão entre as 15 sorteadas; com 11 ou mais acertos, o prêmio é o valor do prêmio da faixa
correspondente no **rateio gravado desse concurso**; com menos de 11, não há prêmio. A conferência MUST NOT usar
valores fixos da configuração. Enquanto o concurso não estiver na base, o jogo MUST ficar "Aguardando sorteio" e
passar a conferido assim que o concurso for gravado, sem ação do usuário.

#### Scenario: Jogo premiado
- **WHEN** um jogo do concurso 3809 acerta 13 dezenas e o rateio gravado do 3809 paga R$ 35,00 na faixa de 13
- **THEN** o jogo mostra 13 acertos e prêmio de R$ 35,00

#### Scenario: Faixa variável
- **WHEN** um jogo acerta 14 dezenas e o rateio gravado do concurso paga R$ 1.920,13 na faixa de 14
- **THEN** o jogo mostra prêmio de R$ 1.920,13

#### Scenario: Sem prêmio
- **WHEN** um jogo acerta 10 dezenas
- **THEN** o jogo mostra 10 acertos e nenhum prêmio

#### Scenario: Concurso ainda não sorteado
- **WHEN** o concurso do jogo não está na base
- **THEN** o jogo aparece como "Aguardando"
- **AND** depois que a atualização grava o concurso, ao abrir a página o jogo aparece conferido

### Requirement: Lista de jogos por concurso
A página SHALL listar os jogos agrupados por concurso, do concurso de maior número para o menor. Dentro de cada
grupo, os jogos MUST aparecer na ordem de cadastro, numerados "01", "02", .... Cada grupo MUST mostrar:
- "Concurso <número>", seguido de "· <data do sorteio>" quando conferido;
- a situação: "Aguardando sorteio" ou "Conferido";
- a quantidade: "1 jogo" ou "<quantidade> jogos";
- se aguardando: "Custo <quantidade × preço da aposta>"; se conferido: "Prêmio <soma dos prêmios>" ou "Sem prêmio";
- se conferido: o resultado oficial (as 15 dezenas sorteadas) e "Prescreve em <data do sorteio + 90 dias>".

Cada jogo MUST mostrar suas 15 dezenas e: "Aguardando", se o concurso não foi sorteado; ou "<acertos> acertos" e o
prêmio ("—" sem prêmio), destacando as dezenas acertadas, se conferido. Valores em reais no formato `R$ 1.234,56`.

#### Scenario: Grupo aguardando
- **WHEN** há 3 jogos para o concurso 3810, ainda não sorteado, com preço da aposta de R$ 3,50
- **THEN** o grupo mostra "Concurso 3810", "Aguardando sorteio", "3 jogos" e "Custo R$ 10,50"

#### Scenario: Grupo conferido
- **WHEN** há 2 jogos para o concurso 3808, sorteado em 19/10/2026, e só um deles ganhou R$ 7,00
- **THEN** o grupo mostra "Concurso 3808 · 19/10/2026", "Conferido", "2 jogos", "Prêmio R$ 7,00", as dezenas
  sorteadas e "Prescreve em 17/01/2027"

#### Scenario: Ordem dos grupos
- **WHEN** há jogos para os concursos 3808 e 3810
- **THEN** o grupo do 3810 aparece antes do grupo do 3808

### Requirement: Filtros e indicadores
A página SHALL oferecer os filtros "Todos" (padrão), "Aguardando sorteio", "Conferidos" e "Premiados" (grupos com
algum jogo premiado), e mostrar os indicadores, sempre sobre todos os jogos:
- "Jogos salvos": total de jogos e "1 concurso" ou "<quantidade> concursos";
- "Investido": total de jogos × preço da aposta, e "1 aposta de 15 dezenas" ou "<quantidade> apostas de 15
  dezenas";
- "Premiados": quantidade de jogos com prêmio;
- "Total em prêmios": soma dos prêmios dos jogos conferidos.

O preço da aposta mínima MUST vir da configuração (padrão R$ 3,50).

#### Scenario: Indicadores
- **WHEN** há 3 jogos aguardando o concurso 3810 e 2 jogos conferidos no 3808, um deles com R$ 7,00
- **THEN** os indicadores mostram 5 jogos salvos em 2 concursos, R$ 17,50 investido, 1 premiado e R$ 7,00 em prêmios

#### Scenario: Filtrar premiados
- **WHEN** o usuário escolhe "Premiados"
- **THEN** só os grupos com algum jogo premiado são exibidos
- **AND** os indicadores continuam considerando todos os jogos

#### Scenario: Filtrar aguardando
- **WHEN** o usuário escolhe "Aguardando sorteio"
- **THEN** só os grupos de concursos ainda não sorteados são exibidos

### Requirement: Janela de conferência
Ao clicar em um jogo conferido, a página SHALL abrir a janela "Jogo <número do jogo> · concurso <número>" com:
- o volante de 01 a 25 indicando, para cada dezena: acertou (marcada e sorteada), marcada e não sorteada, sorteada e
  não marcada, ou nenhuma das duas, com a legenda dessas situações;
- Sorteio (data), Acertos ("<acertos> de 15"), Faixa ("<acertos> acertos" ou "Não premiado"), Prêmio (valor ou
  "—") e Prescreve em (data do sorteio + 90 dias).

Jogos aguardando sorteio MUST NOT abrir a janela.

#### Scenario: Conferir um jogo premiado
- **WHEN** o usuário clica no jogo 02 do concurso 3808, que acertou 11 dezenas com prêmio de R$ 7,00
- **THEN** a janela "Jogo 02 · concurso 3808" mostra "11 de 15", "11 acertos", "R$ 7,00" e a prescrição
- **AND** o volante destaca as 11 dezenas acertadas, as 4 marcadas que não saíram e as 4 sorteadas não marcadas

### Requirement: Exclusão de jogo
Cada jogo SHALL ter a ação de excluir, que pede confirmação com "Excluir o jogo <número do jogo> do concurso
<número>?". Confirmada, o jogo MUST ser removido e a página MUST exibir "Jogo excluído."; cancelada, nada muda.

#### Scenario: Excluir com confirmação
- **WHEN** o usuário exclui o jogo 01 do concurso 3810 e confirma
- **THEN** o jogo deixa de aparecer, os demais jogos do grupo são renumerados e os indicadores são recalculados

#### Scenario: Cancelar exclusão
- **WHEN** o usuário aciona excluir e cancela a confirmação
- **THEN** o jogo continua cadastrado
