## Purpose

Mostra estatísticas por dezena calculadas a partir da base local — frequência numa janela de concursos, desvio em
relação ao esperado e atraso sobre todo o histórico — em rankings, mapa de calor e tabela, como critério de escolha
de dezenas, sem apresentá-las como previsão.

## ADDED Requirements

### Requirement: Acesso à página Estatísticas
A aplicação SHALL oferecer a página "Estatísticas" na rota `/estatisticas`, acessível por um item "Estatísticas" no
menu lateral, logo abaixo de "Resultados". A página MUST exibir, sempre, o aviso: "As estatísticas descrevem o
passado. Cada combinação de 15 dezenas tem a mesma chance (1 em 3.268.760) em todo concurso; os números servem como
critério de escolha, não como previsão."

#### Scenario: Abrir pelo menu
- **WHEN** o usuário aciona o item "Estatísticas" do menu lateral
- **THEN** a página "Estatísticas" é exibida com o título "Estatísticas" e o aviso acima
- **AND** o item "Estatísticas" do menu aparece como selecionado

### Requirement: Janela da análise
A página SHALL oferecer as janelas "Todos (<total>)", "Últimos 10", "Últimos 20", "Últimos 50" e "Últimos 100",
com "Todos" selecionada ao abrir. A janela "Últimos N" MUST considerar os N concursos de maior número gravados na
base (todos, se a base tiver menos que N). A página MUST informar o intervalo considerado no formato "Concursos
<primeiro> a <último> · <quantidade> concursos". A janela afeta as frequências, o desvio e o valor esperado; o
atraso MUST NOT depender da janela.

#### Scenario: Abertura com todo o histórico
- **WHEN** o usuário abre a página e a base tem os concursos 1 a 3808
- **THEN** a janela "Todos (3.808)" está selecionada
- **AND** a página informa "Concursos 1 a 3808 · 3.808 concursos"

#### Scenario: Últimos 10
- **WHEN** o usuário escolhe "Últimos 10" com a base nos concursos 1 a 3808
- **THEN** as estatísticas consideram os concursos 3799 a 3808
- **AND** a página informa "Concursos 3799 a 3808 · 10 concursos"

#### Scenario: Base menor que a janela
- **WHEN** a base tem 7 concursos e o usuário escolhe "Últimos 10"
- **THEN** as estatísticas consideram os 7 concursos

### Requirement: Estatísticas por dezena
Para cada dezena de 01 a 25, sendo `n` a quantidade de concursos na janela, o sistema SHALL calcular:
- **sorteios**: em quantos concursos da janela a dezena saiu;
- **percentual**: sorteios / `n` × 100;
- **esperado**: `n` × 0,6 (15 de 25 dezenas por concurso), igual para todas as dezenas;
- **diferença do esperado**: sorteios − esperado;
- **desvio (σ)**: diferença / √(`n` × 0,6 × 0,4);
- **atraso**: número do último concurso gravado − número do último concurso, em todo o histórico, em que a dezena
  saiu (0 quando saiu no último concurso);
- **última vez**: número do último concurso, em todo o histórico, em que a dezena saiu.

Uma dezena que nunca saiu na base MUST ter atraso e última vez indefinidos, exibidos como "—", e ser tratada como a
de maior atraso.

#### Scenario: Frequência sobre todo o histórico
- **WHEN** a janela tem 3.808 concursos e a dezena 20 saiu em 2.312 deles
- **THEN** a dezena 20 tem 2.312 sorteios, 60,71%, esperado 2.284,8, diferença +27,2 e desvio +0,9σ

#### Scenario: Frequência nos últimos 10
- **WHEN** a janela tem 10 concursos e a dezena 05 saiu em 7 deles
- **THEN** a dezena 05 tem 7 sorteios, 70,00%, esperado 6,0, diferença +1,0 e desvio +0,6σ

#### Scenario: Atraso independe da janela
- **WHEN** o último concurso gravado é o 3808, a dezena 13 saiu pela última vez no 3790 e a janela é "Últimos 10"
- **THEN** a dezena 13 tem atraso 18 e última vez 3790, com 0 sorteios na janela

#### Scenario: Dezena do último concurso
- **WHEN** a dezena 01 saiu no último concurso gravado, o 3808
- **THEN** a dezena 01 tem atraso 0 e última vez 3808

### Requirement: Rankings
A página SHALL mostrar quatro rankings com 5 dezenas cada:
- "Mais sorteadas": maior número de sorteios primeiro, com o valor `<sorteios>` (ex.: "2.312");
- "Menos sorteadas": menor número de sorteios primeiro, com o mesmo formato;
- "Há mais tempo sem sair": maior atraso primeiro, com o valor "<atraso> conc." (ou "nunca saiu");
- "Há menos tempo sem sair": menor atraso primeiro, com o valor "último concurso" para atraso 0 e
  "<atraso> conc." nos demais.

Empates MUST ser desfeitos pela dezena menor primeiro. Quando outras dezenas, além das 5 exibidas, tiverem o mesmo
valor da 5ª, o ranking MUST indicar "+<quantidade> empatadas com <valor>", em que `<valor>` é "<sorteios>
sorteios" nos rankings de frequência e "atraso <atraso>" nos de atraso.

#### Scenario: Mais sorteadas
- **WHEN** as dezenas 20, 11, 13, 24 e 10 têm, nessa ordem, os maiores números de sorteios, sem empate com a 6ª
- **THEN** o ranking "Mais sorteadas" mostra 20, 11, 13, 24 e 10 com os seus sorteios
- **AND** não mostra indicação de empate

#### Scenario: Há menos tempo sem sair
- **WHEN** as 15 dezenas do último concurso têm atraso 0
- **THEN** o ranking "Há menos tempo sem sair" mostra as 5 menores dentre elas com "último concurso"
- **AND** indica "+10 empatadas com atraso 0"

#### Scenario: Empate desfeito pela dezena
- **WHEN** as dezenas 07 e 03 têm o mesmo número de sorteios e estão entre as mais sorteadas
- **THEN** a dezena 03 aparece antes da 07

### Requirement: Mapa de calor
A página SHALL mostrar as 25 dezenas em grade de 5 por 5, na ordem do volante (01 a 05 na primeira linha), com uma
métrica escolhida entre "Frequência" (padrão), "Atraso" e "Desvio". Cada célula MUST mostrar a dezena e o valor da
métrica — sorteios ("2.312"), atraso ("18 conc." ou "—") ou desvio ("+0,9σ") — e ter a cor proporcional ao valor:
mais intensa para mais sorteios ou maior atraso; no desvio, verde acima e vermelho abaixo do esperado, mais
intensa quanto maior a distância. Abaixo da grade, a página MUST explicar a métrica:
- Frequência: "Quantidade de concursos em que a dezena saiu. Mais escuro = mais sorteada.";
- Atraso: "Concursos desde a última vez que a dezena saiu. Mais escuro = mais atrasada.";
- Desvio: "Distância da frequência esperada em desvios-padrão. Verde acima, vermelho abaixo do esperado.".

#### Scenario: Trocar a métrica
- **WHEN** o usuário escolhe "Atraso" no mapa de calor
- **THEN** cada célula passa a mostrar o atraso da dezena e a explicação do atraso é exibida

#### Scenario: Desvio negativo
- **WHEN** o mapa mostra o desvio e a dezena 16 está 1,3 desvio abaixo do esperado
- **THEN** a célula da dezena 16 mostra "-1,3σ" na cor de abaixo do esperado

### Requirement: Tabela das dezenas
A página SHALL listar as 25 dezenas, em ordem de 01 a 25, com as colunas: Dezena, Sorteios, % dos concursos,
Diferença do esperado, Desvio (σ), Atraso e Última vez, e informar acima dela "Esperado por dezena: <esperado> (60%
dos concursos)". Formatos:
- sorteios com separador de milhar; percentual com 2 casas decimais e "%";
- diferença e desvio com 1 casa decimal e sinal "+" quando positivos ("0,0" sem sinal), destacados como acima ou
  abaixo do esperado;
- esperado com 1 casa decimal;
- atraso e última vez como números, ou "—" quando a dezena nunca saiu.

#### Scenario: Linha de uma dezena
- **WHEN** a janela tem 3.808 concursos e a dezena 20 saiu em 2.312, pela última vez no 3805, com o último concurso
  gravado sendo o 3808
- **THEN** a linha da dezena 20 mostra 2.312, 60,71%, +27,2, +0,9, 3 e 3805

#### Scenario: Esperado por dezena
- **WHEN** a janela tem 3.808 concursos
- **THEN** a página informa "Esperado por dezena: 2.284,8 (60% dos concursos)"

### Requirement: Base vazia
Quando a base local não tiver nenhum concurso, a página SHALL exibir, no lugar das estatísticas, a mensagem
"Nenhum concurso gravado na base local. Use “Importar histórico” na página Resultados.", mantendo o aviso fixo.

#### Scenario: Abrir sem concursos
- **WHEN** o usuário abre a página e a base está vazia
- **THEN** a mensagem de base vazia é exibida e não há rankings, mapa nem tabela
