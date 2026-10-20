# importacao-historico Specification

## Purpose

Carrega na base local o histórico completo de concursos da Lotofácil a partir do arquivo XLSX oficial da Caixa,
validando todo o arquivo antes de gravar e gravando somente os concursos que ainda não estão na base.

## Requirements

### Requirement: Download do histórico sob demanda
O sistema SHALL baixar o arquivo XLSX do histórico somente quando o usuário pedir a importação, com uma única
requisição a `{URL base}/api/resultados/download?modalidade=Lotof%C3%A1cil`. A URL base (padrão
`https://servicebus3.caixa.gov.br/portaldeloterias`) e o tempo limite do download em segundos (padrão 60) MUST vir
da configuração. Quando o download falhar, a importação MUST ser interrompida sem gravar nada, com uma mensagem
que identifique a causa:
- sem conexão ou erro de rede: "Não foi possível conectar à Caixa para baixar o histórico.";
- resposta com código diferente de sucesso: "A Caixa respondeu com o código <código> ao baixar o histórico.";
- tempo limite excedido: "A Caixa não respondeu em <segundos> segundos ao baixar o histórico.".

#### Scenario: Endereço montado a partir da configuração
- **WHEN** a URL base configurada é `https://exemplo.local/portaldeloterias` e o usuário pede a importação
- **THEN** o arquivo é pedido em `https://exemplo.local/portaldeloterias/api/resultados/download?modalidade=Lotof%C3%A1cil`

#### Scenario: Caixa responde com erro
- **WHEN** a Caixa responde ao download com o código 503
- **THEN** a importação falha com a mensagem "A Caixa respondeu com o código 503 ao baixar o histórico."
- **AND** nenhum concurso é gravado

#### Scenario: Sem conexão
- **WHEN** não é possível conectar ao endereço da Caixa
- **THEN** a importação falha com a mensagem "Não foi possível conectar à Caixa para baixar o histórico."

#### Scenario: Tempo limite
- **WHEN** o tempo limite configurado é 60 segundos e a Caixa não responde nesse prazo
- **THEN** a importação falha com a mensagem "A Caixa não respondeu em 60 segundos ao baixar o histórico."

### Requirement: Leitura da planilha da Caixa
O sistema SHALL ler a primeira planilha do arquivo. A primeira linha é o cabeçalho; cada linha seguinte é um
concurso, com as colunas lidas **por posição**:

| Posição | Título no cabeçalho | Uso |
|---|---|---|
| 1 | Concurso | número do concurso |
| 2 | Data Sorteio | data, texto `dd/MM/aaaa` |
| 3 a 17 | Bola1 a Bola15 | as 15 dezenas |
| 18 | Ganhadores 15 acertos | ganhadores da faixa 15 |
| 19 | Cidade / UF | ignorada |
| 20 | Rateio 15 acertos | prêmio da faixa 15 |
| 21 / 22 | Ganhadores 14 acertos / Rateio 14 acertos | faixa 14 |
| 23 / 24 | Ganhadores 13 acertos / Rateio 13 acertos | faixa 13 |
| 25 / 26 | Ganhadores 12 acertos / Rateio 12 acertos | faixa 12 |
| 27 / 28 | Ganhadores 11 acertos / Rateio 11 acertos | faixa 11 |
| 29 | Acumulado 15 acertos | acumulado da faixa 15 |
| 30 | Arrecadacao Total | arrecadação total |
| 31 | Estimativa Prêmio | estimativa do próximo concurso |
| 32 | Acumulado sorteio especial Lotofácil da Independência | acumulado da Independência |
| 33 | Observação | ignorada |

Valores em reais MUST ser aceitos no formato da Caixa (`R$1.388,95`, `R$0,00`), com centavos preservados.
Números (concurso, dezenas, ganhadores) MUST ser aceitos com ou sem zeros à esquerda (`05` ou `5`). Os concursos
importados ficam sem ordem do sorteio. Linhas sem nenhuma célula preenchida SHALL ser ignoradas.

#### Scenario: Linha completa
- **WHEN** a planilha tem a linha do concurso 2, de 06/10/2003, dezenas 01, 04, 05, 06, 07, 09, 11, 12, 13, 15,
  16, 19, 20, 23 e 24, 1 ganhador de 15 acertos com prêmio `R$596.323,70` e 184 ganhadores de 14 acertos com
  prêmio `R$1.388,95`
- **THEN** o concurso 2 é importado com essa data, essas dezenas e esse rateio, com os valores exatos em reais

#### Scenario: Concurso antigo com valores zerados
- **WHEN** a linha de um concurso tem `R$0,00` em acumulado, arrecadação, estimativa e acumulado da Independência
- **THEN** o concurso é importado com esses valores iguais a zero

#### Scenario: Colunas ignoradas
- **WHEN** a linha de um concurso tem cidade/UF dos ganhadores e observação preenchidas
- **THEN** o concurso é importado sem esses dados e sem ordem do sorteio

### Requirement: Arquivo inválido não grava nada
Antes de gravar, o sistema MUST ler e validar todas as linhas do arquivo com as regras de validade do concurso.
Se qualquer problema for encontrado, a importação MUST falhar sem gravar nenhum concurso, com a mensagem do
primeiro problema:
- arquivo que não é uma planilha XLSX: "O arquivo recebido da Caixa não é uma planilha XLSX válida.";
- cabeçalho com menos de 33 colunas ou com título diferente do esperado em uma das colunas usadas: "O layout da
  planilha da Caixa mudou: a coluna <posição> deveria ser “<título esperado>” e é “<título encontrado>”.";
- planilha sem nenhum concurso: "A planilha da Caixa não tem nenhum concurso.";
- linha com valor que não pode ser lido, ou com concurso que não atende às regras de validade: "Linha <linha>
  (concurso <número>): <motivo>", em que `<linha>` é a linha da planilha (o cabeçalho é a linha 1); quando o
  próprio número do concurso não pode ser lido, a mensagem fica "Linha <linha>: <motivo>";
- número de concurso repetido na planilha: "Linha <linha>: o concurso <número> aparece mais de uma vez na
  planilha.".

#### Scenario: Dezena repetida em uma linha
- **WHEN** a linha 15 da planilha, do concurso 14, tem a dezena 07 duas vezes
- **THEN** a importação falha com uma mensagem que começa com "Linha 15 (concurso 14):" e informa a dezena
  repetida
- **AND** nenhum concurso da planilha é gravado, nem os das linhas válidas

#### Scenario: Valor em reais ilegível
- **WHEN** a coluna "Rateio 14 acertos" de uma linha tem o texto `abc`
- **THEN** a importação falha informando a linha, o concurso, a coluna "Rateio 14 acertos" e o valor `abc`

#### Scenario: Layout alterado
- **WHEN** a coluna 30 do cabeçalho tem o título "Arrecadação" em vez de "Arrecadacao Total"
- **THEN** a importação falha com a mensagem "O layout da planilha da Caixa mudou: a coluna 30 deveria ser
  “Arrecadacao Total” e é “Arrecadação”."

#### Scenario: Conteúdo que não é XLSX
- **WHEN** a Caixa responde com sucesso, mas o conteúdo não é uma planilha XLSX (por exemplo, uma página HTML)
- **THEN** a importação falha com a mensagem "O arquivo recebido da Caixa não é uma planilha XLSX válida."

#### Scenario: Concurso repetido
- **WHEN** o concurso 100 aparece nas linhas 101 e 102 da planilha
- **THEN** a importação falha com a mensagem "Linha 102: o concurso 100 aparece mais de uma vez na planilha."

### Requirement: Gravação somente dos concursos novos
Com o arquivo válido, o sistema SHALL gravar, num único lote, somente os concursos cujo número ainda não está na
base local. Os concursos já gravados MUST permanecer sem alteração. Se a gravação falhar, nenhum concurso do lote
SHALL ser gravado. Ao final, a importação MUST informar quantos concursos o arquivo tinha e quantos foram gravados.

#### Scenario: Base vazia
- **WHEN** a base está vazia e a planilha tem 3.808 concursos válidos
- **THEN** os 3.808 concursos são gravados
- **AND** a importação informa 3.808 concursos no arquivo e 3.808 gravados

#### Scenario: Base com parte dos concursos
- **WHEN** a base já tem os concursos 1 a 3.800 e a planilha tem os concursos 1 a 3.808
- **THEN** somente os concursos 3.801 a 3.808 são gravados
- **AND** os concursos 1 a 3.800 continuam com os dados que já tinham

#### Scenario: Importar de novo
- **WHEN** a importação é feita com uma planilha cujos concursos já estão todos na base
- **THEN** nada é gravado
- **AND** a importação informa os concursos do arquivo e 0 gravados

### Requirement: Registro de falhas no log
Toda importação que falhar SHALL ser registrada no log de erros da aplicação, com a mensagem exibida e o detalhe
técnico da causa.

#### Scenario: Falha registrada
- **WHEN** a importação falha porque a Caixa respondeu com o código 503
- **THEN** o log de erros recebe um registro dessa falha
