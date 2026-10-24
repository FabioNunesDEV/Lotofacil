# estatisticas Specification

## Purpose

Mostra estatísticas calculadas a partir da base local, em duas abas: por dezena — frequência numa janela de
concursos, desvio em relação ao esperado e atraso sobre todo o histórico, em rankings, mapa de calor e tabela — e por
perfil do concurso — observado na janela × teórico e atraso de cada faixa de 7 perfis —, como critério de escolha de
dezenas, sem apresentá-las como previsão.

## Requirements

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

### Requirement: Abas da página Estatísticas
A página Estatísticas SHALL ter as abas "Por dezena" e "Por perfil do concurso", com "Por dezena" selecionada ao
abrir. A aba "Por dezena" MUST conter o mapa de calor, os rankings e a tabela das dezenas. A janela da análise, o
intervalo informado e o aviso fixo MUST ficar fora das abas e valer para as duas; trocar a janela MUST manter a aba
selecionada.

#### Scenario: Abrir a página
- **WHEN** o usuário abre a página Estatísticas
- **THEN** a aba "Por dezena" está selecionada e mostra o mapa de calor, os rankings e a tabela

#### Scenario: Trocar a janela na aba de perfis
- **WHEN** o usuário está na aba "Por perfil do concurso" e escolhe "Últimos 10"
- **THEN** a aba "Por perfil do concurso" continua selecionada, com os percentuais observados dos últimos 10 concursos

### Requirement: Perfis do concurso
A aba "Por perfil do concurso" SHALL mostrar um cartão para cada perfil, nesta ordem, com título, descrição e o
rótulo das linhas:

| Título | Descrição | Valor do concurso | Rótulo da linha |
|---|---|---|---|
| Pares e ímpares | "12 pares e 13 ímpares no volante" | dezenas pares | "<k> pares" |
| Números primos | "02 03 05 07 11 13 17 19 23" | dezenas primas | "<k> primos" |
| Múltiplos de 3 | "03 06 09 12 15 18 21 24" | múltiplos de 3 | "<k> múltiplos" |
| Fibonacci | "01 02 03 05 08 13 21" | dezenas da sequência | "<k> de Fibonacci" |
| Soma das dezenas | "Soma das 15 dezenas, em faixas de 15" | soma das dezenas | "<mínimo>–<máximo>" |
| Repetidas do concurso anterior | "Dezenas que também saíram no concurso anterior" | dezenas repetidas | "<k> repetidas" |
| Moldura e miolo | "Moldura: as 16 dezenas da borda; miolo: 07 08 09 12 13 14 17 18 19" | dezenas da moldura | "<k> na moldura" |

As faixas de soma MUST ser 120–134, 135–149, 150–164, 165–179, 180–194, 195–209, 210–224, 225–239, 240–254 e
255–270. O concurso anterior é o concurso gravado imediatamente antes; o primeiro concurso da base MUST NOT entrar no
perfil de repetidas.

#### Scenario: Cartões na ordem
- **WHEN** o usuário abre a aba "Por perfil do concurso"
- **THEN** a aba mostra, nesta ordem, os cartões "Pares e ímpares", "Números primos", "Múltiplos de 3", "Fibonacci",
  "Soma das dezenas", "Repetidas do concurso anterior" e "Moldura e miolo"

#### Scenario: Valores de um concurso
- **WHEN** um concurso sorteou 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 e o anterior sorteou 11 a 25
- **THEN** esse concurso conta como 7 pares, 6 primos, 5 múltiplos, 6 de Fibonacci, soma 120–134, 5 repetidas e
  9 na moldura

### Requirement: Linhas com as pontas agrupadas
Cada cartão SHALL ter uma linha por quantidade possível (ou faixa de soma), em ordem crescente. As quantidades (ou
faixas) das pontas cuja chance teórica seja menor que 1% MUST ser somadas à linha vizinha mais próxima com chance de
pelo menos 1%, que passa a ter o rótulo "≤ <máximo> ..." na ponta de baixo ou "≥ <mínimo> ..." na ponta de cima (na
soma, "≤ <máximo>" e "≥ <mínimo>"). Linhas sem ponta agrupada mantêm o rótulo simples.

#### Scenario: Linhas de pares
- **WHEN** o usuário vê o cartão "Pares e ímpares"
- **THEN** as linhas são "≤ 4 pares", "5 pares", "6 pares", "7 pares", "8 pares", "9 pares" e "≥ 10 pares"

#### Scenario: Linhas da soma
- **WHEN** o usuário vê o cartão "Soma das dezenas"
- **THEN** as linhas são "≤ 164", "165–179", "180–194", "195–209", "210–224" e "≥ 225"

#### Scenario: Ponta sem agrupamento
- **WHEN** o usuário vê o cartão "Fibonacci"
- **THEN** as linhas são "≤ 2 de Fibonacci", "3 de Fibonacci", "4 de Fibonacci", "5 de Fibonacci", "6 de Fibonacci" e
  "7 de Fibonacci"

### Requirement: Observado e teórico
Para cada linha, a página SHALL mostrar:
- **observado**: concursos da janela cujo valor cai na linha / concursos da janela considerados × 100 (no perfil de
  repetidas, sem contar o primeiro concurso da base);
- **teórico**: chance de um jogo de 15 dezenas cair na linha, pela distribuição hipergeométrica
  P(k) = C(K, k) × C(25 − K, 15 − k) / C(25, 15), com K = 12 (pares), 9 (primos), 8 (múltiplos de 3), 7 (Fibonacci),
  15 (repetidas: as dezenas do concurso anterior) e 16 (moldura); na soma, pela contagem das 3.268.760 combinações
  em cada faixa;
- uma barra com o observado e uma marca no teórico, na mesma escala dentro do cartão.

Percentuais MUST usar duas casas, como "31,18%". O teórico MUST NOT depender da janela nem da base.

#### Scenario: Teórico de pares
- **WHEN** o usuário vê o cartão "Pares e ímpares"
- **THEN** a linha "7 pares" mostra teórico 31,18%, "≤ 4 pares" mostra 1,27% e "≥ 10 pares" mostra 2,87%

#### Scenario: Teórico da soma
- **WHEN** o usuário vê o cartão "Soma das dezenas"
- **THEN** "180–194" mostra teórico 29,07%, "195–209" mostra 29,67%, "≤ 164" mostra 4,55% e "≥ 225" mostra 5,13%

#### Scenario: Observado na janela
- **WHEN** a janela tem 10 concursos e 3 deles tiveram 7 pares
- **THEN** a linha "7 pares" mostra observado 30,00%

#### Scenario: Repetidas sem o primeiro concurso
- **WHEN** a base tem os concursos 1 a 10 e a janela é "Todos"
- **THEN** o perfil de repetidas considera os concursos 2 a 10, e os observados das linhas somam 100% sobre 9 concursos

### Requirement: Atraso do perfil
Cada linha SHALL mostrar o atraso da faixa: número do último concurso gravado − número do último concurso, em todo o
histórico, cujo valor caiu na linha. O atraso MUST NOT depender da janela. Atraso 0 MUST aparecer como "último";
faixa que nunca saiu, como "—".

#### Scenario: Faixa do último concurso
- **WHEN** o último concurso gravado, o 3810, teve 8 pares
- **THEN** a linha "8 pares" mostra o atraso "último"

#### Scenario: Faixa antiga
- **WHEN** o último concurso gravado é o 3810 e o último concurso com 10 ou mais pares foi o 3768
- **THEN** a linha "≥ 10 pares" mostra o atraso 42, também com a janela "Últimos 10"

#### Scenario: Faixa que nunca saiu
- **WHEN** nenhum concurso da base teve 10 ou mais pares
- **THEN** a linha "≥ 10 pares" mostra "—"
