# Análise: Lotodicas – Estatísticas da Lotofácil

- **Fonte:** [lotodicas.com.br/lotofacil/estatisticas](https://www.lotodicas.com.br/lotofacil/estatisticas)
- **Base analisada:** concurso 3799 (atualizado em 14/10/2026), 3.799 concursos
- **Data da análise:** 15/10/2026

## Visão geral

- Página única com 9 blocos estatísticos, gratuita.
- Filtro de período "Considerar": todos, últimos 10, 20, 50 ou 100 concursos.
- Dados renderizados no servidor e embutidos na página em uma variável JS (`dados_estatisticas`); o JS (`estatisticas.min.js`, jQuery 2.1.4) só reordena e aplica mapa de calor.
- Página monetizada por anúncios; demais ferramentas são Premium (R$ 39,90/ano).

## Blocos estatísticos

### Por dezena (25 dezenas, grade 5x5)
Opções: ordenar por número/quantidade, crescente/decrescente, mapa de calor.

| Bloco | Métrica | Exemplo (todos os concursos) |
|---|---|---|
| Dezenas mais sorteadas | Nº de concursos em que a dezena saiu | 20 = 2368 (maior); 16 = 2174 (menor) |
| Média das dezenas | Frequência real − esperada (esperada = N × 15/25 = 2279,4) | 20 = +89; 16 = −105 |
| Dezenas mais atrasadas | Concursos desde a última aparição | 04 = 3; 05 e 10 = 2 |

### Por perfil do concurso
Colunas comuns: valor/faixa, ocorrências, %, média ("1 em X"), concurso da última ocorrência, atraso (concursos desde a última).

| Bloco | Conjunto de referência | Faixas mais comuns (observado) |
|---|---|---|
| Pares e ímpares | 12 pares / 13 ímpares | 7P-8I 31,35%; 8P-7I 25,45%; 6P-9I 20,51% |
| Números primos | 2,3,5,7,11,13,17,19,23 (9) | 5 = 31,01%; 6 = 29,14%; 4 = 17,27% |
| Múltiplos de 3 | 3,6,…,24 (8) | 5 = 33,82%; 4 = 26,98%; 6 = 19,27% |
| Fibonacci | 1,2,3,5,8,13,21 (7) | 4 = 34,30%; 5 = 27,88%; 3 = 19,66% |
| Soma das dezenas | Faixas de 15 (120–270) | 180–194 30,03%; 195–209 28,53% |
| Repetidas do anterior | Dezenas iguais ao concurso anterior | 9 = 32,38%; 8 = 24,61%; 10 = 21,98% |

## Observado x teórico

Cálculo teórico pela distribuição hipergeométrica: P(k) = C(K,k) × C(25−K, 15−k) / C(25,15). Soma calculada por enumeração das 3.268.760 combinações.

| Estatística | Faixa | Observado | Teórico |
|---|---|---|---|
| Pares | 7 | 31,35% | 31,18% |
| Pares | 8 | 25,45% | 25,99% |
| Primos | 5 | 31,01% | 30,87% |
| Múltiplos de 3 | 5 | 33,82% | 33,32% |
| Fibonacci | 4 | 34,30% | 34,08% |
| Soma | 180–194 | 30,03% | 29,07% |
| Soma | 195–209 | 28,53% | 29,67% |
| Repetidas | 9 | 32,38% | 32,15% |

**Conclusão:** os percentuais históricos coincidem com a probabilidade combinatória. As estatísticas descrevem a quantidade de combinações de cada perfil, não tendência do sorteio. Servem como filtros de perfil, sem ganho de probabilidade por jogo (cada combinação de 15 dezenas = 1 em 3.268.760).

Desvio por dezena: o desvio padrão esperado por dezena é ≈ 30 (binomial, N = 3.799, p = 0,6). A maioria das dezenas está dentro de ±2 desvios, mas os extremos não: 20 = +2,9 e 16 = −3,5 desvios. Teste qui-quadrado sobre as 25 frequências: χ² ≈ 56 (24 g.l.), p ≈ 0,0002, ou seja, desvio estatisticamente significativo em relação à distribuição uniforme.
- Causa não identificada (possíveis: fases antigas do sorteio/equipamento, erro de dados). Verificar por período na solução.
- Significância histórica não prova previsibilidade do próximo concurso; exigiria validação fora da amostra (backtest).

## Estrutura de dados identificada

Variável `dados_estatisticas` (JSON):

| Chave | Formato |
|---|---|
| `frequencia` | `[{d: dezena, q: quantidade}]` |
| `media` | `[{d, m: diferença}]` |
| `atraso` | `[{d, a: atraso}]` |
| `paridade`, `primos`, `multiplos3`, `fibonacci`, `somas`, `repetidos` | `[ {faixa: ocorrências}, {faixa: último concurso} ]` |
| `qtd_concursos`, `numero_concursos`, `dezenas_possiveis` | metadados |

Atraso e "1 em X" são derivados: atraso = último concurso − concurso da última ocorrência; média = N / ocorrências.

## Outras ferramentas do site (Lotofácil)

| Ferramenta | Função | Acesso |
|---|---|---|
| Gerador | Gera 100/1000 jogos aleatórios ou lista própria e filtra por: contém/não contém dezenas, pares, ímpares, primos, Fibonacci, múltiplos de 3, soma mín./máx. Exporta .txt | Premium |
| Grupos de dezenas | Duplas, trios, quadras, grupos de 5, 13 e 14 que mais saem juntos (ex.: dupla 11-20 = 38,07%) | Premium |
| Tabelas (18) | Ciclos, sequências e saltos, ausentes, irmãos, duques, trincas, padrões, tabela binária etc. | Premium |
| Matrizes | 376 desdobramentos (16–25 dezenas, 15–18 por cartão, garantia 3–14 pontos) | Premium |
| Palpites | 3 jogos por atrasadas, 3 por mais sorteadas, 5 aleatórios | Premium |
| Probabilidades | Chances por qtde de dezenas (15–18) | Livre |
| Conferidor, Simulador, Resultados, Mapa de ganhadores, Surpresinha | Utilitários | — |
| API v2 | `GET /api/v2/lotofacil/results/{last|n}?token=` (1 req/10 s, token por e-mail) | Gratuita |

## Pontos fracos observados

- Não mostra o valor teórico ao lado do observado; induz a leitura de tendência.
- Faixas de soma fixas, sem média/desvio.
- Período limitado a 10/20/50/100 ou todos; sem intervalo customizado.
- Não há moldura/miolo, linhas/colunas do volante, sequências nem ciclos na página gratuita.
- Sem gráficos; apenas tabelas e mapa de calor.

## Aproveitamento para a solução própria

**Dados**
- Histórico completo de concursos (fonte oficial Caixa; API Lotodicas como alternativa). Endpoint e formato da Caixa a validar.
- Tabela única de concursos (nº, data, 15 dezenas, rateio) e cálculo das estatísticas a partir dela.

**Estatísticas a implementar (mínimo, equivalente ao site)**
- Frequência, desvio da esperada e atraso por dezena.
- Distribuição por perfil: pares/ímpares, primos, múltiplos de 3, Fibonacci, soma, repetidas do anterior.
- Filtro de período (últimos N e intervalo customizado).

**Diferenciais possíveis**
- Coluna "teórico" ao lado do observado em todas as tabelas.
- Teste de aderência (qui-quadrado) por período e backtest de qualquer critério de escolha.
- Moldura/miolo, linhas/colunas, sequências, ciclos, duplas/trios (grupos).
- Gerador com filtros de perfil + dezenas fixas/excluídas + exportação.
- Fechamentos/desdobramentos com garantia.
- Conferidor de jogos contra o histórico.

**Premissa a manter**
- Ferramenta de filtragem e organização de jogos, não de previsão. Nenhum filtro altera a probabilidade de acerto de uma combinação.
