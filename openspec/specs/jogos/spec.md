# jogos Specification

## Purpose

Permite ao usuário cadastrar os jogos que vai apostar em concursos futuros e, depois do sorteio, conferir os
acertos e o prêmio de cada jogo pelo rateio gravado do próprio concurso.

## Requirements

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

### Requirement: Regras de um jogo de 15 a 20 dezenas
Um jogo SHALL ter o número do concurso a que se destina (maior que zero) e de 15 a 20 dezenas distintas, cada uma
entre 01 e 25, guardadas em ordem crescente. Um jogo que não atenda a essas regras MUST ser rejeitado.

Um jogo de n dezenas SHALL valer C(n, 15) apostas de 15 dezenas (15 → 1, 16 → 16, 17 → 136, 18 → 816, 19 → 3.876,
20 → 15.504), e o seu custo MUST ser essa quantidade de apostas × o preço da aposta mínima da configuração.

#### Scenario: Dezenas fora de ordem
- **WHEN** um jogo é criado com as dezenas 25, 03, 14, ... (fora de ordem)
- **THEN** o jogo guarda as dezenas em ordem crescente

#### Scenario: Quantidade fora do intervalo
- **WHEN** um jogo é criado com 14 ou 21 dezenas
- **THEN** a criação é rejeitada

#### Scenario: Aposta múltipla
- **WHEN** um jogo é criado com 17 dezenas distintas
- **THEN** o jogo é aceito e vale 136 apostas de 15 dezenas

#### Scenario: Custo da aposta
- **WHEN** o preço da aposta mínima é R$ 3,50
- **THEN** um jogo de 15 dezenas custa R$ 3,50, um de 16 custa R$ 56,00 e um de 20 custa R$ 54.264,00

#### Scenario: Dezena repetida ou fora do volante
- **WHEN** um jogo é criado com uma dezena repetida, ou com a dezena 0 ou 26
- **THEN** a criação é rejeitada

### Requirement: Cadastro de jogo
A página SHALL ter o botão "Novo jogo", que abre o cadastro com:
- o campo "Concurso", já preenchido com o número do último concurso gravado + 1;
- o campo "Dezenas por jogo", ao lado de "Concurso", com as opções 15 a 20 e 15 selecionado;
- um volante com as dezenas 01 a 25, em que cada clique marca ou desmarca a dezena, sem permitir marcar mais que o
  valor de "Dezenas por jogo";
- a contagem "<marcadas> de <dezenas por jogo> dezenas · <custo da aposta>";
- o botão "Salvar", habilitado somente quando a quantidade de dezenas marcadas for igual a "Dezenas por jogo".

Ao reduzir "Dezenas por jogo" para menos que as dezenas já marcadas, as marcações MUST ser mantidas, "Salvar" fica
desabilitado e o volante só permite desmarcar até chegar à nova quantidade. Salvar MUST NOT pedir confirmação
adicional, qualquer que seja o custo.

Ao salvar, o sistema MUST gravar o jogo e exibir "Jogo salvo para o concurso <número>.", ou recusar com a mensagem:
- base sem concursos: "Importe o histórico na página Resultados antes de cadastrar jogos.";
- concurso igual ou anterior ao último gravado: "Só é possível cadastrar jogos para concursos depois do <último
  gravado>.";
- mesmo concurso com as mesmas dezenas já cadastrado: "Este jogo já está cadastrado para o concurso <número>.".

#### Scenario: Concurso sugerido
- **WHEN** o último concurso gravado é o 3808 e o usuário aciona "Novo jogo"
- **THEN** o campo "Concurso" vem com 3809 e o campo "Dezenas por jogo" vem com 15

#### Scenario: Marcar dezenas
- **WHEN** o usuário marca 14 dezenas no volante com "Dezenas por jogo" em 15 e preço da aposta de R$ 3,50
- **THEN** a contagem mostra "14 de 15 dezenas · R$ 3,50" e "Salvar" fica desabilitado
- **AND** ao marcar a 15ª, "Salvar" é habilitado e as demais dezenas não podem ser marcadas

#### Scenario: Escolher aposta de 17 dezenas
- **WHEN** o usuário escolhe 17 em "Dezenas por jogo" e marca 12 dezenas, com preço da aposta de R$ 3,50
- **THEN** a contagem mostra "12 de 17 dezenas · R$ 476,00" e "Salvar" fica desabilitado
- **AND** ao marcar a 17ª, "Salvar" é habilitado e as demais dezenas não podem ser marcadas

#### Scenario: Reduzir a quantidade com dezenas marcadas
- **WHEN** o usuário marcou 17 dezenas com "Dezenas por jogo" em 17 e muda para 15
- **THEN** as 17 dezenas continuam marcadas, a contagem mostra "17 de 15 dezenas · R$ 3,50" e "Salvar" fica
  desabilitado
- **AND** nenhuma dezena nova pode ser marcada; ao desmarcar duas, "Salvar" é habilitado

#### Scenario: Salvar jogo
- **WHEN** o usuário salva 15 dezenas para o concurso 3809, com o 3808 como último gravado
- **THEN** o jogo é gravado, a página exibe "Jogo salvo para o concurso 3809." e o jogo aparece no grupo do
  concurso 3809

#### Scenario: Salvar aposta de 20 dezenas
- **WHEN** o usuário salva 20 dezenas para o concurso 3809, com o 3808 como último gravado
- **THEN** o jogo é gravado sem pedido de confirmação e a página exibe "Jogo salvo para o concurso 3809."

#### Scenario: Concurso já sorteado
- **WHEN** o usuário tenta salvar um jogo para o concurso 3808, que é o último gravado
- **THEN** o jogo não é gravado e a página exibe "Só é possível cadastrar jogos para concursos depois do 3808."

#### Scenario: Jogo duplicado
- **WHEN** o usuário salva para o concurso 3809 as mesmas dezenas de um jogo já cadastrado para o 3809
- **THEN** o jogo não é gravado e a página exibe "Este jogo já está cadastrado para o concurso 3809."

#### Scenario: Jogo contido em outro do mesmo concurso
- **WHEN** há um jogo de 16 dezenas para o concurso 3809 e o usuário salva para o 3809 um jogo de 15 dezenas
  formado por 15 delas
- **THEN** o jogo é gravado normalmente

#### Scenario: Mesmas dezenas em outro concurso
- **WHEN** o usuário salva para o concurso 3810 as mesmas dezenas de um jogo do concurso 3809
- **THEN** o jogo é gravado normalmente

#### Scenario: Base vazia
- **WHEN** a base não tem concursos e o usuário tenta salvar um jogo
- **THEN** o jogo não é gravado e a página exibe "Importe o histórico na página Resultados antes de cadastrar
  jogos."

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

### Requirement: Conferência do jogo
Quando o concurso de um jogo estiver gravado na base, o sistema SHALL conferir o jogo: os acertos são as dezenas do
jogo que estão entre as 15 sorteadas. Um jogo de n dezenas com a acertos MUST receber, para cada faixa k de 11 a
15, C(a, k) × C(n − a, 15 − k) prêmios de k acertos, cada um pago pelo valor do prêmio da faixa k no **rateio gravado
desse concurso**; o prêmio do jogo é a soma desses prêmios. Com menos de 11 acertos, não há prêmio. Para um jogo de
15 dezenas, isso equivale a um único prêmio na faixa dos acertos. A conferência MUST NOT usar valores fixos da
configuração. Enquanto o concurso não estiver na base, o jogo MUST ficar "Aguardando sorteio" e passar a conferido
assim que o concurso for gravado, sem ação do usuário.

#### Scenario: Jogo premiado
- **WHEN** um jogo de 15 dezenas do concurso 3809 acerta 13 dezenas e o rateio gravado do 3809 paga R$ 35,00 na
  faixa de 13
- **THEN** o jogo mostra 13 acertos e prêmio de R$ 35,00

#### Scenario: Faixa variável
- **WHEN** um jogo de 15 dezenas acerta 14 dezenas e o rateio gravado do concurso paga R$ 1.920,13 na faixa de 14
- **THEN** o jogo mostra prêmio de R$ 1.920,13

#### Scenario: Aposta múltipla premiada em várias faixas
- **WHEN** um jogo de 16 dezenas acerta 13 dezenas e o rateio gravado do concurso paga R$ 35,00 na faixa de 13 e
  R$ 14,00 na faixa de 12
- **THEN** o jogo recebe 3 prêmios de 13 acertos e 13 prêmios de 12 acertos
- **AND** mostra 13 acertos e prêmio de R$ 287,00

#### Scenario: Aposta múltipla com 15 acertos
- **WHEN** um jogo de 16 dezenas acerta as 15 dezenas sorteadas
- **THEN** o jogo recebe 1 prêmio de 15 acertos e 15 prêmios de 14 acertos, pelos valores do rateio gravado

#### Scenario: Aposta de 20 dezenas com 11 acertos
- **WHEN** um jogo de 20 dezenas acerta 11 dezenas e o rateio gravado do concurso paga R$ 7,00 na faixa de 11
- **THEN** o jogo recebe 126 prêmios de 11 acertos e mostra prêmio de R$ 882,00

#### Scenario: Sem prêmio
- **WHEN** um jogo de qualquer tamanho acerta 10 dezenas
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
- se aguardando: "Custo <soma dos custos dos jogos do grupo>"; se conferido: "Prêmio <soma dos prêmios>" ou "Sem
  prêmio";
- se conferido: o resultado oficial (as 15 dezenas sorteadas) e "Prescreve em <data do sorteio + 90 dias>".

Cada jogo MUST mostrar suas dezenas e, se tiver mais de 15, a quantidade "<n> dezenas"; e também: "Aguardando", se o
concurso não foi sorteado; ou "<acertos> acertos" e o prêmio total ("—" sem prêmio), destacando as dezenas
acertadas, se conferido. Valores em reais no formato `R$ 1.234,56`.

#### Scenario: Grupo aguardando
- **WHEN** há 3 jogos de 15 dezenas para o concurso 3810, ainda não sorteado, com preço da aposta de R$ 3,50
- **THEN** o grupo mostra "Concurso 3810", "Aguardando sorteio", "3 jogos" e "Custo R$ 10,50"

#### Scenario: Grupo aguardando com aposta múltipla
- **WHEN** há 2 jogos de 15 dezenas e 1 jogo de 16 dezenas para o concurso 3810, com preço da aposta de R$ 3,50
- **THEN** o grupo mostra "3 jogos" e "Custo R$ 63,00"
- **AND** o jogo de 16 dezenas mostra suas 16 dezenas e "16 dezenas"

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
- "Investido": soma dos custos de todos os jogos, e "1 aposta de 15 dezenas" ou "<quantidade> apostas de 15
  dezenas", em que a quantidade é a soma das apostas de 15 dezenas que cada jogo vale;
- "Premiados": quantidade de jogos com prêmio;
- "Total em prêmios": soma dos prêmios dos jogos conferidos.

O preço da aposta mínima MUST ser o preço atual da página Configurações (o gravado ou, sem preço gravado, o da
configuração da aplicação, R$ 3,50 por padrão), o mesmo para todos os jogos.

#### Scenario: Indicadores
- **WHEN** há 3 jogos de 15 dezenas aguardando o concurso 3810 e 2 jogos de 15 dezenas conferidos no 3808, um deles
  com R$ 7,00
- **THEN** os indicadores mostram 5 jogos salvos em 2 concursos, R$ 17,50 investido, 1 premiado e R$ 7,00 em prêmios

#### Scenario: Indicadores com aposta múltipla
- **WHEN** há 2 jogos de 15 dezenas e 1 jogo de 16 dezenas, com preço da aposta de R$ 3,50
- **THEN** "Jogos salvos" mostra 3 e "Investido" mostra "R$ 63,00" e "18 apostas de 15 dezenas"

#### Scenario: Investido com preço alterado
- **WHEN** há 2 jogos de 15 dezenas e 1 jogo de 16 dezenas e o preço salvo em Configurações é R$ 4,00
- **THEN** "Investido" mostra "R$ 72,00"

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
- Sorteio (data), Aposta ("15 dezenas", ou "<n> dezenas · <apostas equivalentes> apostas" para 16 a 20), Acertos
  ("<acertos> de 15"), Faixa ("<acertos> acertos" ou "Não premiado"), Prêmio (valor total ou "—") e Prescreve em
  (data do sorteio + 90 dias).

Para um jogo de 16 a 20 dezenas premiado, a janela MUST mostrar também os prêmios por faixa, da maior para a menor,
somente as faixas com prêmio, cada uma como "<quantidade> × <faixa> acertos · <valor do prêmio da faixa>".

Jogos aguardando sorteio MUST NOT abrir a janela.

#### Scenario: Conferir um jogo premiado
- **WHEN** o usuário clica no jogo 02 do concurso 3808, de 15 dezenas, que acertou 11 dezenas com prêmio de R$ 7,00
- **THEN** a janela "Jogo 02 · concurso 3808" mostra "15 dezenas", "11 de 15", "11 acertos", "R$ 7,00" e a
  prescrição
- **AND** o volante destaca as 11 dezenas acertadas, as 4 marcadas que não saíram e as 4 sorteadas não marcadas
- **AND** a janela não mostra os prêmios por faixa

#### Scenario: Conferir uma aposta múltipla premiada
- **WHEN** o usuário clica num jogo de 16 dezenas que acertou 13, com o rateio gravado pagando R$ 35,00 na faixa de 13
  e R$ 14,00 na faixa de 12
- **THEN** a janela mostra "16 dezenas · 16 apostas", "13 de 15", "13 acertos" e "R$ 287,00"
- **AND** mostra os prêmios por faixa "3 × 13 acertos · R$ 35,00" e "13 × 12 acertos · R$ 14,00"
- **AND** o volante destaca as 13 dezenas acertadas, as 3 marcadas que não saíram e as 2 sorteadas não marcadas

### Requirement: Exclusão de jogo
Cada jogo SHALL ter a ação de excluir, que pede confirmação com "Excluir o jogo <número do jogo> do concurso
<número>?". Confirmada, o jogo MUST ser removido e a página MUST exibir "Jogo excluído."; cancelada, nada muda.

#### Scenario: Excluir com confirmação
- **WHEN** o usuário exclui o jogo 01 do concurso 3810 e confirma
- **THEN** o jogo deixa de aparecer, os demais jogos do grupo são renumerados e os indicadores são recalculados

#### Scenario: Cancelar exclusão
- **WHEN** o usuário aciona excluir e cancela a confirmação
- **THEN** o jogo continua cadastrado

### Requirement: Exportar os jogos de um concurso
Cada grupo de concurso da página Meus jogos SHALL ter, no cabeçalho, o botão "Exportar". Ele abre o diálogo
"Exportar · concurso <número>" com o texto dos jogos do concurso, somente leitura, e os botões "Copiar", "Baixar .txt"
e "Fechar". O texto MUST ter, nesta ordem e uma informação por linha:
1. `Lotofácil · concurso <número>`;
2. `<quantidade> · <custo>`: a quantidade como no grupo ("1 jogo" ou "<n> jogos") e o custo total dos jogos do
   concurso (soma das apostas de 15 de cada jogo × preço da aposta mínima), no formato `R$ 1.234,56`;
3. uma linha em branco;
4. um jogo por linha, na ordem de cadastro: `<número do jogo com dois dígitos>) ` seguido das dezenas em ordem
   crescente, com dois dígitos e separadas por um espaço; nas apostas de 16 a 20 dezenas, ` (<n> dezenas)` no final.

O texto MUST NOT trazer conferência (acertos, prêmio ou resultado), mesmo quando o concurso já foi sorteado, e MUST
conter todos os jogos do concurso. "Copiar" MUST copiar o texto para a área de transferência e exibir "Jogos
copiados."; se o navegador recusar a cópia, MUST exibir "Não foi possível copiar. Selecione o texto e copie
manualmente.". "Baixar .txt" MUST baixar o arquivo `concurso-<número>.txt` com o mesmo texto, em UTF-8. "Fechar"
fecha o diálogo.

#### Scenario: Texto dos jogos
- **WHEN** o concurso 3813 tem, nesta ordem de cadastro, os jogos 01 02 04 05 07 10 11 13 14 18 20 21 22 24 25,
  02 03 05 06 08 09 12 15 16 17 19 20 21 23 25 e um de 16 dezenas 01 03 04 06 07 08 10 11 12 14 15 17 18 22 24 25,
  com preço da aposta de R$ 3,50, e o usuário aciona "Exportar" no grupo do 3813
- **THEN** o diálogo "Exportar · concurso 3813" mostra o texto:
  ```
  Lotofácil · concurso 3813
  3 jogos · R$ 63,00

  01) 01 02 04 05 07 10 11 13 14 18 20 21 22 24 25
  02) 02 03 05 06 08 09 12 15 16 17 19 20 21 23 25
  03) 01 03 04 06 07 08 10 11 12 14 15 17 18 22 24 25 (16 dezenas)
  ```

#### Scenario: Um jogo
- **WHEN** o concurso 3814 tem um único jogo de 15 dezenas e o usuário o exporta
- **THEN** a segunda linha do texto é "1 jogo · R$ 3,50"

#### Scenario: Concurso já sorteado
- **WHEN** o usuário exporta os jogos do concurso 3808, já conferido, com um jogo premiado
- **THEN** o texto traz só o cabeçalho e as dezenas, sem acertos, prêmio nem resultado

#### Scenario: Copiar
- **WHEN** o usuário aciona "Copiar" no diálogo
- **THEN** o texto vai para a área de transferência e a página exibe "Jogos copiados."

#### Scenario: Cópia recusada
- **WHEN** o navegador recusa a cópia para a área de transferência
- **THEN** a página exibe "Não foi possível copiar. Selecione o texto e copie manualmente." e o diálogo continua aberto

#### Scenario: Baixar
- **WHEN** o usuário aciona "Baixar .txt" no diálogo do concurso 3813
- **THEN** o navegador baixa o arquivo "concurso-3813.txt" com o mesmo texto do diálogo

#### Scenario: Fechar
- **WHEN** o usuário aciona "Fechar"
- **THEN** o diálogo fecha e a lista continua como estava
