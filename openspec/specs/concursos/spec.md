# concursos Specification

## Purpose

Define o que é um concurso válido da Lotofácil para o sistema e como ele é guardado na base local, que serve de
fonte para a consulta de resultados, as estatísticas e a conferência de jogos.

## Requirements

### Requirement: Dados do concurso
Um concurso SHALL conter: número, data do sorteio, as 15 dezenas sorteadas, a ordem do sorteio (opcional), o
rateio das faixas de 11 a 15 acertos (quantidade de ganhadores e valor do prêmio de cada faixa), a arrecadação
total, o valor acumulado para a faixa de 15 acertos, a estimativa de prêmio do próximo concurso e o valor
acumulado para a Lotofácil da Independência. As dezenas MUST ficar disponíveis em ordem crescente, qualquer que
seja a ordem em que foram informadas.

#### Scenario: Concurso completo
- **WHEN** um concurso é criado com número, data, 15 dezenas, ordem do sorteio, rateio das cinco faixas e valores
- **THEN** todos os dados informados ficam disponíveis no concurso

#### Scenario: Dezenas informadas fora de ordem
- **WHEN** um concurso é criado com as dezenas 25, 03, 14, ... (fora de ordem)
- **THEN** as dezenas do concurso aparecem em ordem crescente

#### Scenario: Concurso sem ordem do sorteio
- **WHEN** um concurso é criado sem a ordem do sorteio
- **THEN** o concurso é válido e indica que a ordem do sorteio não é conhecida

### Requirement: Validação do concurso
O sistema MUST rejeitar, com mensagem que identifique o problema, um concurso que não atenda a todas as regras:
- número maior que zero;
- exatamente 15 dezenas, distintas, cada uma entre 1 e 25;
- ordem do sorteio, quando informada, com as mesmas 15 dezenas do concurso;
- rateio com exatamente uma entrada para cada faixa de 11, 12, 13, 14 e 15 acertos;
- quantidade de ganhadores e valores monetários (prêmios, arrecadação, acumulados e estimativa) maiores ou iguais
  a zero.

Valores monetários iguais a zero SHALL ser aceitos, pois concursos antigos não informam arrecadação nem
estimativa.

#### Scenario: Número inválido
- **WHEN** um concurso é criado com número 0
- **THEN** a criação é rejeitada informando que o número deve ser maior que zero

#### Scenario: Quantidade de dezenas diferente de 15
- **WHEN** um concurso é criado com 14 ou 16 dezenas
- **THEN** a criação é rejeitada informando que são necessárias 15 dezenas

#### Scenario: Dezena repetida
- **WHEN** um concurso é criado com uma dezena repetida
- **THEN** a criação é rejeitada informando a dezena repetida

#### Scenario: Dezena fora do intervalo
- **WHEN** um concurso é criado com a dezena 0 ou 26
- **THEN** a criação é rejeitada informando que as dezenas vão de 1 a 25

#### Scenario: Ordem do sorteio com dezenas diferentes
- **WHEN** um concurso é criado com uma ordem do sorteio que contém uma dezena que não está entre as sorteadas
- **THEN** a criação é rejeitada informando que a ordem do sorteio não corresponde às dezenas

#### Scenario: Rateio incompleto ou com faixa repetida
- **WHEN** um concurso é criado sem a faixa de 13 acertos, ou com a faixa de 14 acertos duas vezes, ou com uma
  faixa fora de 11 a 15
- **THEN** a criação é rejeitada informando o problema no rateio

#### Scenario: Valor negativo
- **WHEN** um concurso é criado com quantidade de ganhadores ou algum valor monetário negativo
- **THEN** a criação é rejeitada informando qual valor é inválido

#### Scenario: Valores zerados de concurso antigo
- **WHEN** um concurso é criado com arrecadação, estimativa e acumulados iguais a zero
- **THEN** o concurso é válido

### Requirement: Gravação na base local
O sistema SHALL gravar concursos em lote na base local e recuperá-los com exatamente os mesmos dados. Cada número
de concurso MUST existir uma única vez na base. Se algum concurso do lote já estiver gravado, a gravação MUST
falhar e nenhum concurso do lote SHALL ser gravado.

#### Scenario: Gravar e recuperar
- **WHEN** um lote de concursos é gravado e depois um deles é buscado pelo número
- **THEN** o concurso recuperado tem o mesmo número, data, dezenas, ordem do sorteio, rateio e valores do gravado,
  com centavos preservados

#### Scenario: Concurso inexistente
- **WHEN** é buscado um número de concurso que não está na base
- **THEN** a busca informa que o concurso não foi encontrado, sem erro

#### Scenario: Concurso já gravado
- **WHEN** é gravado um lote que contém um concurso cujo número já está na base
- **THEN** a gravação falha
- **AND** nenhum concurso desse lote fica gravado

#### Scenario: Listagem do mais recente ao mais antigo
- **WHEN** são pedidos os concursos a partir de uma posição e com uma quantidade
- **THEN** a base devolve essa fatia em ordem decrescente de número, junto com o total de concursos gravados

### Requirement: Base local criada automaticamente
A base local SHALL ser um arquivo SQLite cujo caminho é definido em configuração: relativo à pasta da aplicação
(padrão `lotofacil.db`) ou absoluto. Ao iniciar, a aplicação MUST criar a pasta e o arquivo se eles não existirem e
atualizar a estrutura da base para a versão atual, preservando os concursos já gravados.

#### Scenario: Primeira execução
- **WHEN** a aplicação é iniciada e o arquivo da base não existe
- **THEN** o arquivo é criado com a estrutura atual e a base fica vazia

#### Scenario: Execução seguinte
- **WHEN** a aplicação é iniciada com uma base que já tem concursos gravados
- **THEN** os concursos continuam disponíveis

#### Scenario: Caminho absoluto em pasta inexistente
- **WHEN** a configuração aponta a base para `D:\Dados\Lotofacil\lotofacil.db` e a pasta `D:\Dados\Lotofacil` não
  existe
- **THEN** a aplicação cria a pasta e a base vazia nesse caminho, e não na pasta da aplicação
