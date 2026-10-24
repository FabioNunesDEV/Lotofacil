## ADDED Requirements

### Requirement: Filtros de perfil
O formulário SHALL ter, abaixo do volante de fixas e excluídas, a seção recolhível "Filtros de perfil", fechada ao
abrir a página. O cabeçalho mostra "Nenhum filtro ligado", "1 filtro ligado" ou "<k> filtros ligados". A seção MUST
ter uma linha por perfil, nesta ordem: "Pares", "Primos", "Múltiplos de 3", "Fibonacci", "Soma", "Repetidas do
<último concurso gravado>" e "Na moldura". Cada linha tem os campos "Mín." e "Máx.", vazios ao abrir. Campo vazio
significa sem limite daquele lado. Um perfil com pelo menos um limite preenchido conta como filtro ligado.

O valor de um jogo em cada perfil MUST ser calculado sobre todas as dezenas do jogo, como na aba "Por perfil" das
Estatísticas:
- Pares, Primos, Múltiplos de 3, Fibonacci e Na moldura: quantas dezenas do jogo pertencem ao conjunto do perfil
  (primos 02 03 05 07 11 13 17 19 23; múltiplos de 3 03 06 09 12 15 18 21 24; Fibonacci 01 02 03 05 08 13 21;
  moldura: as 16 dezenas da borda do volante);
- Soma: a soma das dezenas do jogo;
- Repetidas: quantas dezenas do jogo saíram no último concurso gravado.

Os limites de quantidade MUST aceitar de 0 a n, sendo n as dezenas por jogo. Os de soma vão de n × (n + 1) / 2 a
n × (51 − n) / 2 (120 a 270 com 15 dezenas; 210 a 310 com 20). A seção MUST ter o botão "Limpar filtros", que
esvazia todos os limites e desliga "Evitar padrões populares". Também MUST exibir o texto: "Os filtros só escolhem o
perfil dos jogos; não mudam a chance de acerto."

#### Scenario: Valores ao abrir
- **WHEN** o usuário abre a página Gerar jogos
- **THEN** a seção "Filtros de perfil" está fechada e mostra "Nenhum filtro ligado"
- **AND** ao abrir a seção, todos os campos "Mín." e "Máx." estão vazios e "Evitar padrões populares" está desligado

#### Scenario: Contagem de filtros ligados
- **WHEN** o usuário preenche Pares com mínimo 7 e máximo 8 e Soma só com máximo 209
- **THEN** o cabeçalho da seção mostra "2 filtros ligados"

#### Scenario: Jogos respeitam os filtros
- **WHEN** o usuário limita Pares de 7 a 8 e Soma de 180 a 209 e gera 10 jogos de 15 dezenas
- **THEN** todos os jogos gerados têm 7 ou 8 dezenas pares e soma entre 180 e 209

#### Scenario: Repetidas do último concurso
- **WHEN** o último concurso gravado é o 3810, o usuário limita "Repetidas do 3810" de 8 a 10 e gera jogos
- **THEN** cada jogo gerado tem de 8 a 10 dezenas que saíram no concurso 3810

#### Scenario: Filtros em jogos de 18 dezenas
- **WHEN** o usuário escolhe 18 dezenas por jogo, limita Pares de 9 a 10 e gera jogos
- **THEN** cada jogo tem 18 dezenas, das quais 9 ou 10 são pares

#### Scenario: Limpar filtros
- **WHEN** há 3 filtros ligados e "Evitar padrões populares" ligado, e o usuário aciona "Limpar filtros"
- **THEN** todos os limites ficam vazios, "Evitar padrões populares" fica desligado e o cabeçalho mostra
  "Nenhum filtro ligado"

### Requirement: Evitar padrões populares
A seção "Filtros de perfil" SHALL ter a opção "Evitar padrões populares", desligada ao abrir. Quando ligada, mostra o
campo "Maior sequência permitida", com 9 ao ligar e valores de 1 a n, e conta como um filtro ligado. Com a opção
ligada, um jogo MUST NOT ter mais dezenas seguidas (consecutivas, como 04 05 06) que o limite. Abaixo do campo, a
página MUST exibir: "Jogos com sequências longas, como 01 a 15, são muito escolhidos; evitá-los só pode reduzir a
divisão do prêmio se o jogo for premiado." Linhas e colunas completas do volante e datas MUST NOT ser tratadas como
padrões populares.

#### Scenario: Limite padrão
- **WHEN** o usuário liga "Evitar padrões populares" e gera jogos de 15 dezenas
- **THEN** o campo "Maior sequência permitida" mostra 9
- **AND** nenhum jogo gerado tem 10 ou mais dezenas seguidas

#### Scenario: Limite mais apertado
- **WHEN** o usuário liga a opção, muda a maior sequência para 4 e gera jogos
- **THEN** nenhum jogo gerado tem 5 ou mais dezenas seguidas

#### Scenario: Fixas que já formam a sequência
- **WHEN** o usuário fixa as dezenas 01 a 10, liga a opção com maior sequência 9 e gera jogos
- **THEN** nenhum jogo é gerado e a página exibe a mensagem de nenhum jogo atendendo aos filtros

### Requirement: Dica de chance teórica dos filtros
Com 15 dezenas por jogo, cada perfil com algum limite SHALL mostrar, na sua linha, "Chance teórica: <percentual>".
O percentual é a parte das 3.268.760 combinações de 15 dezenas cujo valor no perfil fica entre o mínimo e o máximo
informados, com duas casas decimais. Um lado vazio usa o menor ou o maior valor possível do perfil. Para Repetidas,
conta-se quantas dezenas de cada combinação estão entre as 15 do último concurso. A dica MUST NOT aparecer com 16 a
20 dezenas por jogo nem em "Maior sequência permitida".

#### Scenario: Dica de pares
- **WHEN** o usuário limita Pares de 7 a 8 com 15 dezenas por jogo
- **THEN** a linha Pares mostra "Chance teórica: 57,17%"

#### Scenario: Dica de soma
- **WHEN** o usuário limita Soma de 180 a 209 com 15 dezenas por jogo
- **THEN** a linha Soma mostra "Chance teórica: 58,73%"

#### Scenario: Sem dica com apostas múltiplas
- **WHEN** o usuário limita Pares de 9 a 10 e escolhe 18 dezenas por jogo
- **THEN** a linha Pares não mostra a dica de chance teórica

## MODIFIED Requirements

### Requirement: Regras da geração
Ao acionar "Gerar jogos", o sistema SHALL gerar a quantidade pedida de jogos distintos entre si, todos atendendo aos
filtros ligados, ou recusar com a mensagem:
- mais fixas que dezenas por jogo: "Há <fixas> dezenas fixas, mais que as <n> dezenas por jogo.";
- dezenas livres insuficientes: "Faltam dezenas livres: com <fixas> fixas e <excluídas> excluídas sobram <livres>
  livres, e cada jogo precisa de mais <n − fixas>.";
- mínimo maior que o máximo num filtro: "No filtro <nome do filtro>, o mínimo (<mínimo>) é maior que o máximo
  (<máximo>).", com os nomes "de pares", "de primos", "de múltiplos de 3", "de Fibonacci", "de soma", "de
  repetidas" e "da moldura";
- nenhum jogo encontrado que atenda aos filtros: "Nenhum jogo atendeu aos filtros. Afrouxe os limites ou revise as
  fixas e excluídas.";
- base sem concursos: a mensagem de base vazia da página.

Quando as fixas e excluídas permitirem menos jogos distintos que a quantidade pedida (C(livres, n − fixas)), o
sistema MUST examinar todos os jogos possíveis e gerar os que atendem aos filtros. O aviso é "Só existem <possíveis>
jogos diferentes com essas fixas e excluídas; foram gerados <possíveis>." sem filtros ligados, e "Só existem
<possíveis> jogos diferentes com essas fixas, excluídas e filtros; foram gerados <possíveis>." com filtros ligados
("1 jogo diferente" e "foi gerado 1" no singular). Nos dois casos, <possíveis> conta só os jogos que atendem aos
filtros.

Fora desse caso, quando os filtros não deixarem achar a quantidade pedida dentro do limite de tentativas da
geração, o sistema MUST mostrar os jogos achados e avisar "Foram gerados <gerados> dos <pedidos> jogos pedidos:
poucos jogos atendem aos filtros." ("Foi gerado 1 dos <pedidos> jogos pedidos: ..." no singular). Uma nova geração
MUST substituir a lista anterior.

#### Scenario: Gerar 10 jogos
- **WHEN** o usuário gera 10 jogos de 15 dezenas sem fixas, sem excluídas e sem filtros
- **THEN** a lista mostra 10 jogos distintos, cada um com 15 dezenas distintas entre 01 e 25

#### Scenario: Jogo inteiro fixo
- **WHEN** o usuário fixa 15 dezenas e pede 10 jogos de 15 dezenas, sem filtros
- **THEN** a lista mostra um único jogo com as 15 fixas
- **AND** a página avisa "Só existe 1 jogo diferente com essas fixas e excluídas; foi gerado 1."

#### Scenario: Poucas combinações
- **WHEN** o usuário fixa 13 dezenas, exclui 8 e pede 10 jogos de 15 dezenas, sem filtros
- **THEN** a lista mostra os 6 jogos possíveis (C(4, 2))
- **AND** a página avisa "Só existem 6 jogos diferentes com essas fixas e excluídas; foram gerados 6."

#### Scenario: Poucas combinações com filtros
- **WHEN** o usuário fixa 13 dezenas, exclui 8, liga um filtro que só 2 dos 6 jogos possíveis atendem e pede 10
  jogos de 15 dezenas
- **THEN** a lista mostra os 2 jogos que atendem ao filtro
- **AND** a página avisa "Só existem 2 jogos diferentes com essas fixas, excluídas e filtros; foram gerados 2."

#### Scenario: Filtros que quase nada atende
- **WHEN** os filtros ligados deixam passar tão poucos jogos que a geração acha só 3 dos 10 pedidos
- **THEN** a lista mostra os 3 jogos achados
- **AND** a página avisa "Foram gerados 3 dos 10 jogos pedidos: poucos jogos atendem aos filtros."

#### Scenario: Nenhum jogo atende
- **WHEN** o usuário limita Pares com mínimo 13 em jogos de 15 dezenas (o volante só tem 12 pares) e gera jogos
- **THEN** nenhum jogo é gerado e a página exibe "Nenhum jogo atendeu aos filtros. Afrouxe os limites ou revise as
  fixas e excluídas."

#### Scenario: Mínimo maior que o máximo
- **WHEN** o usuário limita Soma com mínimo 210 e máximo 180 e gera jogos
- **THEN** nenhum jogo é gerado e a página exibe "No filtro de soma, o mínimo (210) é maior que o máximo (180)."

#### Scenario: Fixas demais
- **WHEN** o usuário fixa 16 dezenas e gera jogos de 15 dezenas
- **THEN** nenhum jogo é gerado e a página exibe "Há 16 dezenas fixas, mais que as 15 dezenas por jogo."

#### Scenario: Faltam dezenas livres
- **WHEN** o usuário fixa 5 dezenas, exclui 12 e gera jogos de 15 dezenas
- **THEN** nenhum jogo é gerado e a página exibe "Faltam dezenas livres: com 5 fixas e 12 excluídas sobram 8 livres,
  e cada jogo precisa de mais 10."
