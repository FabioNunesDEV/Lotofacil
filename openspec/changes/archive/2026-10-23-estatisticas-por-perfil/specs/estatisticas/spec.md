## ADDED Requirements

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
