# Lotofácil – Site oficial CAIXA: regras, funcionalidades e obtenção de dados

- **Fonte:** [loterias.caixa.gov.br/Paginas/Lotofacil.aspx](https://loterias.caixa.gov.br/Paginas/Lotofacil.aspx)
- **Último concurso verificado:** 3805 (15/10/2026)
- **Data da análise:** 15/10/2026

## 1. Regras do jogo

- Volante com 25 dezenas (01–25); aposta de **15 a 20** dezenas.
- Sorteio de 15 dezenas. Premia 11, 12, 13, 14 e 15 acertos.
- **Surpresinha:** o sistema escolhe as dezenas.
- **Teimosinha:** mesma aposta por 2, 3, 4, 6, 8, 9, 12, 18 ou 24 concursos consecutivos.
- Sorteios de segunda a sábado (confirmado pelas datas do histórico; sem sorteio em feriados, ex.: 12/10). Horário não consta na página.
- Prêmios prescrevem em **90 dias** após o sorteio.

### Preços

Preço = C(n,15) × R$ 3,50 (valor da aposta mínima).

| Dezenas | Apostas equivalentes | Valor |
|---|---|---|
| 15 | 1 | R$ 3,50 |
| 16 | 16 | R$ 56,00 |
| 17 | 136 | R$ 476,00 |
| 18 | 816 | R$ 2.856,00 |
| 19 | 3.876 | R$ 13.566,00 |
| 20 | 15.504 | R$ 54.264,00 |

### Premiação

- Prêmio bruto = **43,79%** da arrecadação.
- Faixas fixas (pagas primeiro): 11 = R$ 7,00 | 12 = R$ 14,00 | 13 = R$ 35,00.
- Restante, concurso normal: 62% p/ 15 acertos; 13% p/ 14; 10% acumula p/ concursos final 0; 15% acumula p/ Lotofácil da Independência.
- Concurso final 0: 72% p/ 15; 13% p/ 14; 15% p/ Independência.
- Sem ganhador: acumula na faixa de 15 acertos do concurso seguinte.
- **Lotofácil da Independência (setembro):** 87% p/ 15, 13% p/ 14; recebe os acumulados (especial, final 0 e anterior). Sem ganhador de 15, rateia na faixa seguinte (14, 13…), não acumula.

### Probabilidade (1 em N) – tabela oficial

| Acertos | 15 dez. | 16 | 17 | 18 | 19 | 20 |
|---|---|---|---|---|---|---|
| 15 | 3.268.760 | 204.298 | 24.035 | 4.006 | 843 | 211 |
| 14 | 21.792 | 3.027 | 601 | 153 | 47 | 17 |
| 13 | 692 | 162 | 49 | 18 | 8 | 4,2 |
| 12 | 60 | 21 | 9 | 5 | 3,2 | 2,6 |
| 11 | 11 | 6 | 4 | 3 | 2,9 | 3,9 |

A página também traz a tabela de **quantidade de prêmios por aposta múltipla** (ex.: 16 dezenas acertando 15 = 1 prêmio de 15 + 15 de 14). Pode ser calculada: nº de prêmios de k acertos = C(acertos, k) × C(n − acertos, 15 − k).

### Bolão

| Dezenas | Cotas (mín–máx) | Cota mínima | Bolão mín. | Bolão máx. | Jogos por recibo |
|---|---|---|---|---|---|
| 15 | 2–7 | R$ 4,50 | R$ 14,00 | R$ 35,00 | 10 |
| 16 | 2–35 | R$ 4,50 | R$ 56,00 | R$ 560,00 | 10 |
| 17 | 2–40 | R$ 11,90 | R$ 476,00 | R$ 4.760,00 | 10 |
| 18 | 2–50 | R$ 57,12 | R$ 2.856,00 | R$ 28.560,00 | 10 |
| 19 | 2–85 | R$ 159,60 | R$ 13.566,00 | R$ 122.094,00 | 9 |
| 20 | 2–100 | R$ 542,64 | R$ 54.264,00 | R$ 217.056,00 | 4 |

- Todas as apostas do bolão com a mesma quantidade de dezenas.
- Cotas de bolões das lotéricas/online: tarifa de serviço de até 35%.
- Venda de bolões digitais encerra às 19h30 no dia do sorteio.

## 2. Funcionalidades da página

- Resultado do último concurso, navegação anterior/próximo e busca por número.
- Premiação por faixa (ganhadores e valor), ganhadores por cidade (detalhamento), arrecadação.
- Estimativa do próximo concurso, acumulado final 0 e acumulado da Independência.
- Seções informativas (regras, preços, probabilidade, bolão, repasses sociais).
- **Download de resultados** (XLSX com todo o histórico).
- Não oferece estatísticas (frequência, atraso etc.).

Tecnologia: SharePoint + AngularJS/jQuery. Os dados vêm de uma API REST pública; a URL base fica em `https://loterias.caixa.gov.br/Style%20Library/json/params.txt`:

```json
{ "urlapiloterias": "https://servicebus3.caixa.gov.br/portaldeloterias" }
```

## 3. Como obter os dados

### 3.1 Histórico completo – XLSX (carga inicial)

```
GET https://servicebus3.caixa.gov.br/portaldeloterias/api/resultados/download?modalidade=Lotofácil
```
(`modalidade` em URL encode: `Lotof%C3%A1cil`)

- Retorno: `application/xlsx`, ~385 KB, 1 planilha, 3.804 linhas (concurso 1 a D-1).
- Dezenas em **ordem crescente** (não traz ordem do sorteio).
- Colunas (ordem física): Concurso | Data Sorteio | Bola1–Bola15 | Ganhadores 15 | Cidade/UF | Rateio 15 | Ganhadores 14 | Rateio 14 | Ganhadores 13 | Rateio 13 | Ganhadores 12 | Rateio 12 | Ganhadores 11 | Rateio 11 | Acumulado 15 | Arrecadação Total | Estimativa Prêmio | Acumulado Independência | Observação.

**Formato conferido em 20/10/2026** (concursos 1 a 3.808, todos válidos):
- Uma única planilha em `xl/worksheets/sheet1.xml`, com prefixo de namespace `x:` (`<x:row>`, `<x:c>`, `<x:v>`).
- Cabeçalho (linha 1): células `t="s"` com atributo `r`, títulos em `xl/sharedStrings.xml`.
- Linhas de dados: **sem** atributo `r`, sempre 33 células, valores inline — `t="n"` para números (dezenas
  com zero à esquerda, `05`) e `t="str"` para data e valores em reais.
- `<dimension ref="A1:AG1">` não reflete as linhas; não usar.
- Títulos exatos das 33 colunas: `Concurso`, `Data Sorteio`, `Bola1` … `Bola15`, `Ganhadores 15 acertos`,
  `Cidade / UF`, `Rateio 15 acertos`, `Ganhadores 14 acertos`, `Rateio 14 acertos`, `Ganhadores 13 acertos`,
  `Rateio 13 acertos`, `Ganhadores 12 acertos`, `Rateio 12 acertos`, `Ganhadores 11 acertos`,
  `Rateio 11 acertos`, `Acumulado 15 acertos`, `Arrecadacao Total` (sem cedilha), `Estimativa Prêmio`,
  `Acumulado sorteio especial Lotofácil da Independência`, `Observação`.

**Cuidados no parser:**
- Células sem atributo de referência (`r`): ler por posição, não por letra de coluna. Algumas bibliotecas podem desalinhar.
- Valores monetários como texto (`R$1.388,95`, sem espaço após `R$`) e data como texto `dd/MM/yyyy`.
- Concursos antigos com arrecadação/estimativa = R$ 0,00 (dado ausente).
- Faixas fixas antigas diferentes (ex.: concurso 1: 11 = R$ 2,00; 12 = R$ 4,00; 13 = R$ 10,00).

### 3.2 Concurso individual – JSON (atualização diária)

```
GET https://servicebus3.caixa.gov.br/portaldeloterias/api/lotofacil/          → último concurso
GET https://servicebus3.caixa.gov.br/portaldeloterias/api/lotofacil/{numero}  → concurso específico
```
O host antigo `servicebus2.caixa.gov.br` também respondeu 200.

Campos úteis:

| Campo | Conteúdo |
|---|---|
| `numero`, `dataApuracao` | Concurso e data (`dd/MM/yyyy`) |
| `listaDezenas` | 15 dezenas em ordem crescente (strings "01"…"25") |
| `dezenasSorteadasOrdemSorteio` | Ordem real do sorteio (não existe no XLSX) |
| `listaRateioPremio[]` | `faixa`, `descricaoFaixa`, `numeroDeGanhadores`, `valorPremio` (numérico) |
| `listaMunicipioUFGanhadores[]` | Ganhadores de 15 por cidade/UF/canal |
| `valorArrecadado` | Arrecadação |
| `acumulado`, `valorAcumuladoProximoConcurso` | Acumulação |
| `valorEstimadoProximoConcurso`, `dataProximoConcurso`, `numeroConcursoProximo` | Próximo concurso |
| `valorAcumuladoConcurso_0_5`, `numeroConcursoFinal_0_5` | Acumulado final 0 |
| `valorAcumuladoConcursoEspecial` | Acumulado Independência |
| `ultimoConcurso` | `true` se for o mais recente |

**Conferido em 20/10/2026** (concurso 3808):
- `listaRateioPremio[].faixa` vai de 1 a 5: faixa 1 = `"15 acertos"`, 2 = `"14 acertos"` … 5 = `"11 acertos"`.
- Valores monetários vêm como número JSON (ex.: `1456047.72`); ler como decimal direto do texto para não perder
  centavos.
- Correspondência com as colunas do XLSX: `valorArrecadado` = "Arrecadacao Total";
  `valorAcumuladoProximoConcurso` = "Acumulado 15 acertos"; `valorEstimadoProximoConcurso` = "Estimativa
  Prêmio"; `valorAcumuladoConcursoEspecial` = "Acumulado sorteio especial Lotofácil da Independência". Os valores
  do 3808 são iguais nas duas fontes.
- Concurso que ainda não existe (ex.: o próximo, antes do sorteio, ou `99999`) responde **HTTP 500** com um JSON de
  erro, e não 404. Por isso a atualização só pede números até o `numero` devolvido por `/api/lotofacil/`.
- Um concurso recém-sorteado pode vir antes de o rateio ser divulgado; o sistema não grava concurso com
  `listaRateioPremio` vazia ou com todas as faixas sem ganhadores.

### 3.3 Preços

- Não há endpoint de preços na API (`/api/loterias` traz só textos).
- Preço está apenas no HTML da página. Opções:
  - **Tabela de configuração** (valor base R$ 3,50 + data de vigência) e cálculo C(n,15) × valor. Recomendado: muda raramente.
  - Scraping da seção "Tabela de preços" para detectar alteração.
- Histórico de preços não é fornecido; registrar vigências manualmente se necessário.

### 3.4 Restrições observadas

- Acesso via proxy da máquina local e do ambiente de nuvem foi bloqueado (403 no CONNECT); pelo navegador funcionou. Validar acesso a partir do servidor da solução (HttpClient .NET), incluindo `User-Agent` e cadeia SSL.
- API não documentada oficialmente: pode mudar sem aviso (host já mudou de servicebus2 para servicebus3). Ler a URL base de `params.txt` ou manter em configuração.
- Sem limite de requisições informado; usar intervalo entre chamadas.

### 3.5 Estratégia de sincronização sugerida

1. Carga inicial: baixar o XLSX e importar todos os concursos.
2. Rotina diária (após o sorteio): `GET /api/lotofacil/` → se `numero` > último gravado, buscar os concursos faltantes por número.
3. Opcional: preencher `ordem do sorteio` dos concursos antigos via API (1 chamada por concurso, ~3.800 chamadas, com intervalo).
4. Conferir lacunas periodicamente (sequência de concursos sem falhas).

## 4. Modelo de dados mínimo

| Tabela | Campos |
|---|---|
| `Concurso` | Numero (PK), Data, Arrecadacao, Acumulado15, EstimativaProximo, AcumuladoEspecial, EspecialIndependencia (bit), Observacao |
| `ConcursoDezena` | Numero, Dezena (1–25), OrdemSorteio (nullable) |
| `ConcursoRateio` | Numero, Faixa (11–15), Ganhadores, ValorPremio |
| `ConfigPreco` | QtdeDezenas, ValorBase, VigenciaInicio |

Alternativa para cálculo: coluna bitmask de 25 bits por concurso (ex.: `INT`), permite calcular acertos com `POPCOUNT(jogo & sorteio)` e acelerar backtests.

## 5. Comparação com a Lotodicas

- Lotodicas mostrava o concurso 3799 (07/10/2026) como último em 15/10; a CAIXA já tinha o 3805. Defasagem de 6 concursos.
- A fonte oficial fornece tudo o que a Lotodicas usa (dezenas e rateio) e mais (ordem do sorteio, ganhadores por cidade).
- Estatísticas devem ser calculadas pela própria solução a partir da base local.

## 6. Requisitos para a solução de apoio à escolha de dezenas

**Base**
- Importação e sincronização automática (seção 3).
- Estatísticas da análise da Lotodicas (frequência, atraso, perfis) com valor teórico ao lado do observado.

**Apoio à escolha**
- Gerador com filtros: dezenas fixas/excluídas, pares/ímpares, primos, múltiplos de 3, Fibonacci, soma, repetidas do anterior, moldura/miolo.
- Suporte a 15–20 dezenas com custo calculado (C(n,15) × preço).
- Fechamentos/desdobramentos com garantia.
- Conferidor: jogo x histórico (quantas vezes faria 11–15) e x último concurso.
- Backtest de critérios de escolha contra o histórico.
- Retorno esperado da aposta usando prêmios fixos + rateios médios históricos de 14 e 15.

**Premissa**
- Cada combinação de 15 dezenas tem a mesma probabilidade (1 em 3.268.760). A solução organiza e filtra jogos; não aumenta a chance de acerto. O único efeito real disponível é evitar combinações populares (menos divisão de prêmio).
