## Purpose

Gera jogos para um concurso futuro a partir das estatísticas das dezenas, com a estratégia, a janela, as dezenas
fixas e excluídas escolhidas pelo usuário, e grava os jogos escolhidos pelo mesmo caminho do cadastro manual, sem
apresentar a geração como aumento da chance de acerto.

## ADDED Requirements

### Requirement: Acesso à página Gerar jogos
A aplicação SHALL oferecer a página "Gerar jogos" na rota `/gerar-jogos`, acessível por um item "Gerar jogos" no
menu lateral, logo abaixo de "Estatísticas". A página MUST exibir, sempre, o aviso: "Nenhuma estratégia aumenta a
chance de acerto: cada jogo de 15 dezenas tem 1 chance em 3.268.760 em todo concurso. As estratégias só organizam a
escolha das dezenas pelo critério escolhido." Sem concursos na base, a página MUST mostrar "Nenhum concurso gravado
na base local. Use “Importar histórico” na página Resultados para carregar os concursos da Caixa." no lugar do
formulário.

#### Scenario: Abrir pelo menu
- **WHEN** o usuário aciona o item "Gerar jogos" do menu lateral
- **THEN** a página "Gerar jogos" é exibida com o título "Gerar jogos" e o aviso acima
- **AND** o item "Gerar jogos" do menu aparece como selecionado

#### Scenario: Base vazia
- **WHEN** o usuário abre a página sem concursos na base
- **THEN** a mensagem de base vazia é exibida e o formulário não aparece

### Requirement: Parâmetros da geração
O formulário SHALL ter:
- "Concurso", preenchido com o último concurso gravado + 1;
- "Quantidade de jogos", de 1 a 100, com 10 ao abrir;
- "Dezenas por jogo", com as opções 15 a 20 e 15 ao abrir;
- "Estratégia", com as opções "Aleatória (Surpresinha)", "Mais sorteadas", "Menos sorteadas", "Mais atrasadas" e
  "Equilibrada", "Aleatória (Surpresinha)" ao abrir, e a descrição da estratégia escolhida logo abaixo;
- "Janela da análise", com "Todos (<total>)", "Últimos 10", "Últimos 20", "Últimos 50" e "Últimos 100", "Todos" ao
  abrir; a janela vale só para a frequência, como na página Estatísticas;
- o custo da geração: "<quantidade> jogos de <n> dezenas · <custo>", em que o custo é quantidade × C(n, 15) × preço
  da aposta mínima da configuração.

As descrições das estratégias MUST ser:
- Aleatória (Surpresinha): "Todas as dezenas livres têm a mesma chance.";
- Mais sorteadas: "Mais chance para as dezenas mais sorteadas na janela.";
- Menos sorteadas: "Mais chance para as dezenas menos sorteadas na janela.";
- Mais atrasadas: "Mais chance para as dezenas há mais tempo sem sair (todo o histórico).";
- Equilibrada: "Metade das dezenas livres pelas mais sorteadas na janela, metade pelas mais atrasadas.".

#### Scenario: Valores ao abrir
- **WHEN** o último concurso gravado é o 3810 e o usuário abre a página, com preço da aposta de R$ 3,50
- **THEN** o formulário mostra Concurso 3811, Quantidade 10, Dezenas por jogo 15, estratégia "Aleatória
  (Surpresinha)" e janela "Todos"
- **AND** o custo mostra "10 jogos de 15 dezenas · R$ 35,00"

#### Scenario: Custo de apostas múltiplas
- **WHEN** o usuário escolhe 5 jogos de 17 dezenas, com preço da aposta de R$ 3,50
- **THEN** o custo mostra "5 jogos de 17 dezenas · R$ 2.380,00"

### Requirement: Dezenas fixas e excluídas
O formulário SHALL ter um volante com as dezenas 01 a 25 em que cada clique alterna a dezena entre livre, fixa e
excluída, nessa ordem, mostrando a contagem "Fixas: <quantidade> · Excluídas: <quantidade>" e um botão "Limpar" que
volta todas para livres. Dezenas fixas MUST estar em todos os jogos gerados; excluídas MUST NOT estar em nenhum. As
fixas podem chegar à quantidade de dezenas por jogo.

#### Scenario: Alternar uma dezena
- **WHEN** o usuário clica três vezes na dezena 07
- **THEN** a dezena passa de livre para fixa, de fixa para excluída e de excluída para livre

#### Scenario: Fixas e excluídas respeitadas
- **WHEN** o usuário fixa 05 e 13, exclui 01 e 25 e gera 10 jogos
- **THEN** todos os jogos contêm 05 e 13 e nenhum contém 01 ou 25

### Requirement: Estratégias por sorteio ponderado
Cada jogo SHALL conter as fixas e completar as dezenas por jogo sorteando, uma a uma e sem repetição, entre as
dezenas livres (nem fixas nem excluídas), com chance proporcional ao peso de cada dezena na estratégia:
- Aleatória (Surpresinha): peso 1 para todas;
- Mais sorteadas: peso = 1 + quantidade de dezenas, entre as 25, com menos sorteios na janela;
- Menos sorteadas: peso = 1 + quantidade de dezenas com mais sorteios na janela;
- Mais atrasadas: peso = 1 + quantidade de dezenas com atraso menor (sobre todo o histórico; dezena que nunca saiu
  tem o maior atraso);
- Equilibrada: das dezenas a sortear, a metade arredondada para cima é sorteada pelos pesos de "Mais sorteadas" e o
  restante pelos pesos de "Mais atrasadas", entre as livres ainda não escolhidas.

Dezenas com o mesmo valor MUST ter o mesmo peso. As dezenas de cada jogo MUST ser exibidas em ordem crescente.

#### Scenario: Pesos de Mais sorteadas
- **WHEN** na janela a dezena 20 é a única com mais sorteios e a dezena 04 a única com menos
- **THEN** na estratégia "Mais sorteadas" a dezena 20 tem peso 25 e a 04 tem peso 1

#### Scenario: Empate tem o mesmo peso
- **WHEN** as dezenas 03 e 07 têm o mesmo número de sorteios na janela
- **THEN** as duas têm o mesmo peso em "Mais sorteadas" e em "Menos sorteadas"

#### Scenario: Equilibrada com 15 dezenas e 3 fixas
- **WHEN** o usuário gera um jogo de 15 dezenas pela estratégia "Equilibrada" com 3 fixas
- **THEN** o jogo tem as 3 fixas, 6 dezenas sorteadas pelos pesos de "Mais sorteadas" e 6 pelos de "Mais atrasadas"

### Requirement: Regras da geração
Ao acionar "Gerar jogos", o sistema SHALL gerar a quantidade pedida de jogos distintos entre si, ou recusar com a
mensagem:
- mais fixas que dezenas por jogo: "Há <fixas> dezenas fixas, mais que as <n> dezenas por jogo.";
- dezenas livres insuficientes: "Faltam dezenas livres: com <fixas> fixas e <excluídas> excluídas sobram <livres>
  livres, e cada jogo precisa de mais <n − fixas>.";
- base sem concursos: a mensagem de base vazia da página.

Quando as fixas e excluídas permitirem menos jogos distintos que a quantidade pedida (C(livres, n − fixas)), o
sistema MUST gerar todos os jogos possíveis e avisar "Só existem <possíveis> jogos diferentes com essas fixas e
excluídas; foram gerados <possíveis>." ("1 jogo diferente" e "foi gerado 1" no singular). Uma nova geração MUST
substituir a lista anterior.

#### Scenario: Gerar 10 jogos
- **WHEN** o usuário gera 10 jogos de 15 dezenas sem fixas nem excluídas
- **THEN** a lista mostra 10 jogos distintos, cada um com 15 dezenas distintas entre 01 e 25

#### Scenario: Jogo inteiro fixo
- **WHEN** o usuário fixa 15 dezenas e pede 10 jogos de 15 dezenas
- **THEN** a lista mostra um único jogo com as 15 fixas
- **AND** a página avisa "Só existe 1 jogo diferente com essas fixas e excluídas; foi gerado 1."

#### Scenario: Poucas combinações
- **WHEN** o usuário fixa 13 dezenas, exclui 8 e pede 10 jogos de 15 dezenas
- **THEN** a lista mostra os 6 jogos possíveis (C(4, 2))
- **AND** a página avisa "Só existem 6 jogos diferentes com essas fixas e excluídas; foram gerados 6."

#### Scenario: Fixas demais
- **WHEN** o usuário fixa 16 dezenas e gera jogos de 15 dezenas
- **THEN** nenhum jogo é gerado e a página exibe "Há 16 dezenas fixas, mais que as 15 dezenas por jogo."

#### Scenario: Faltam dezenas livres
- **WHEN** o usuário fixa 5 dezenas, exclui 12 e gera jogos de 15 dezenas
- **THEN** nenhum jogo é gerado e a página exibe "Faltam dezenas livres: com 5 fixas e 12 excluídas sobram 8 livres,
  e cada jogo precisa de mais 10."

### Requirement: Lista dos jogos gerados
A página SHALL mostrar o cartão "Jogos gerados" com o subtítulo "Nenhum jogo gerado ainda" antes da primeira geração
e "<quantidade> jogos · <custo>" (ou "1 jogo · <custo>") depois. Cada jogo MUST aparecer numerado ("01", "02", ...),
com as dezenas (as fixas em destaque) e o perfil: "<pares> pares · <ímpares> ímpares", "Soma <soma>" e "<repetidas>
repetidas do <último concurso gravado>" (dezenas do jogo que saíram no último concurso), além de "<n> dezenas" quando
tiver mais de 15. Cada jogo MUST ter a ação de remover, sem confirmação, que o tira da lista e renumera os demais. O
cartão MUST ter os botões "Gerar novamente", que repete a geração com os parâmetros atuais, e "Salvar jogos",
habilitado só com algum jogo na lista.

#### Scenario: Perfil de um jogo
- **WHEN** um jogo gerado tem as dezenas 01 a 15 e o último concurso gravado, 3810, sorteou 01, 05, 06, 07, 10, 11,
  14, 15, 16, 17, 18, 19, 20, 23 e 24
- **THEN** o jogo mostra "7 pares · 8 ímpares", "Soma 120" e "8 repetidas do 3810"

#### Scenario: Remover um jogo
- **WHEN** a lista tem 10 jogos e o usuário remove o jogo 03
- **THEN** a lista passa a ter 9 jogos, numerados de 01 a 09, e o subtítulo mostra "9 jogos · <custo de 9 jogos>"

#### Scenario: Gerar novamente
- **WHEN** o usuário aciona "Gerar novamente"
- **THEN** a lista é substituída por uma nova geração com os parâmetros atuais do formulário

### Requirement: Salvar os jogos gerados
"Salvar jogos" SHALL gravar os jogos da lista para o concurso do formulário pelas mesmas regras do cadastro manual.
Antes de gravar, o sistema MUST recusar tudo, com a mesma mensagem do cadastro, quando a base estiver vazia ou o
concurso for igual ou anterior ao último gravado. Jogos com as mesmas dezenas de um jogo já cadastrado para o
concurso MUST ser recusados e os demais gravados. Ao final, a página MUST exibir:
- todos gravados: "<quantidade> jogos salvos para o concurso <número>." ("1 jogo salvo ..." no singular);
- parte duplicada: "<gravados> jogos salvos para o concurso <número>; <duplicados> já estavam cadastrados." (com
  singular em cada parte);
- todos duplicados: "Nenhum jogo salvo: todos já estavam cadastrados para o concurso <número>.".

Quando algum jogo for gravado, a lista MUST ser esvaziada.

#### Scenario: Salvar todos
- **WHEN** o último concurso gravado é o 3810 e o usuário salva 10 jogos gerados para o 3811
- **THEN** os 10 jogos são gravados, a página exibe "10 jogos salvos para o concurso 3811." e a lista fica vazia
- **AND** os jogos aparecem na página Meus jogos no grupo do concurso 3811

#### Scenario: Parte já cadastrada
- **WHEN** 2 dos 10 jogos da lista já estão cadastrados para o concurso 3811
- **THEN** 8 jogos são gravados e a página exibe "8 jogos salvos para o concurso 3811; 2 já estavam cadastrados."

#### Scenario: Concurso já sorteado
- **WHEN** o usuário troca o concurso para 3810, que é o último gravado, e aciona "Salvar jogos"
- **THEN** nenhum jogo é gravado, a página exibe "Só é possível cadastrar jogos para concursos depois do 3810." e a
  lista continua
