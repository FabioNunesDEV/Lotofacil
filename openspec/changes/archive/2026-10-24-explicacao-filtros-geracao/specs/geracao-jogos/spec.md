## ADDED Requirements

### Requirement: Explicação dos filtros
A seção "Filtros de perfil" SHALL mostrar, logo abaixo do aviso de que os filtros não mudam a chance de acerto, a
frase "Mín. e Máx.: quantas dezenas do jogo entram em cada grupo. Deixe vazio para não limitar.". Cada linha de filtro
SHALL mostrar, embaixo do nome e sempre visível, a explicação do perfil:
- Pares: "12 pares no volante";
- Primos: "02 03 05 07 11 13 17 19 23";
- Múltiplos de 3: "03 06 09 12 15 18 21 24";
- Fibonacci: "01 02 03 05 08 13 21";
- Soma: "Soma das dezenas do jogo (de <menor> a <maior>)", com a menor e a maior soma possíveis para as dezenas por
  jogo escolhidas (de 120 a 270 com 15 dezenas);
- Repetidas: "Dezenas do jogo que saíram no <último concurso gravado>";
- Na moldura: "As 16 dezenas da borda do volante (miolo: 07 08 09 12 13 14 17 18 19)".

Com 15 dezenas por jogo, a explicação MUST terminar com " · mais comum: <faixa> (<percentual>)". A faixa é o par de
valores vizinhos do perfil com a maior parte das 3.268.760 combinações de 15 dezenas, escrito "<a> ou <b>"; na Soma,
o par de faixas vizinhas de 15 da aba "Por perfil" das Estatísticas, escrito "<menor> a <maior>". O percentual é a
parte das combinações dentro da faixa, arredondado para inteiro. Com 16 a 20 dezenas por jogo, o valor mais comum MUST
NOT aparecer. As explicações não mudam a geração nem os limites aceitos.

#### Scenario: Frase do topo
- **WHEN** o usuário abre a seção "Filtros de perfil"
- **THEN** ela mostra "Mín. e Máx.: quantas dezenas do jogo entram em cada grupo. Deixe vazio para não limitar."

#### Scenario: Explicações com 15 dezenas
- **WHEN** o último concurso gravado é o 3813 e o usuário abre os filtros com 15 dezenas por jogo
- **THEN** as linhas mostram, nesta ordem:
  - "12 pares no volante · mais comum: 7 ou 8 (57%)";
  - "02 03 05 07 11 13 17 19 23 · mais comum: 5 ou 6 (60%)";
  - "03 06 09 12 15 18 21 24 · mais comum: 4 ou 5 (60%)";
  - "01 02 03 05 08 13 21 · mais comum: 4 ou 5 (62%)";
  - "Soma das dezenas do jogo (de 120 a 270) · mais comum: 180 a 209 (59%)";
  - "Dezenas do jogo que saíram no 3813 · mais comum: 8 ou 9 (56%)";
  - "As 16 dezenas da borda do volante (miolo: 07 08 09 12 13 14 17 18 19) · mais comum: 9 ou 10 (60%)".

#### Scenario: Apostas múltiplas sem o mais comum
- **WHEN** o usuário escolhe 18 dezenas por jogo
- **THEN** a linha Pares mostra só "12 pares no volante"
- **AND** a linha Soma mostra "Soma das dezenas do jogo (de 171 a 297)"

#### Scenario: Explicação sem limites
- **WHEN** nenhum limite está preenchido
- **THEN** as explicações aparecem do mesmo jeito, e nenhum filtro conta como ligado
