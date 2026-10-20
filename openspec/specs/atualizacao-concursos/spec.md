# atualizacao-concursos Specification

## Purpose

Mantém a base local em dia buscando na API JSON da Caixa, um a um, os concursos divulgados depois do último
concurso gravado, já com a ordem do sorteio.

## Requirements

### Requirement: Atualização a partir do último concurso gravado
O sistema SHALL atualizar a base somente quando o usuário pedir, assim:
1. com a base vazia, a atualização MUST terminar sem fazer nenhuma requisição, informando que a base está vazia;
2. caso contrário, consultar o último concurso divulgado em `{URL base}/api/lotofacil/`;
3. se o número dele não for maior que o do último concurso gravado, terminar informando que a base já está
   atualizada;
4. senão, buscar em `{URL base}/api/lotofacil/{número}`, em ordem crescente, cada concurso do último gravado + 1
   até o último divulgado.

Números que faltam antes do último concurso gravado (lacunas) MUST NOT ser buscados. A URL base é a mesma da
importação do histórico, vinda da configuração.

#### Scenario: Base vazia
- **WHEN** o usuário pede a atualização e a base não tem nenhum concurso
- **THEN** nenhuma requisição é feita à Caixa
- **AND** a atualização informa que a base está vazia

#### Scenario: Base já atualizada
- **WHEN** o último concurso gravado é o 3808 e a Caixa informa o 3808 como último divulgado
- **THEN** nenhum concurso é buscado nem gravado
- **AND** a atualização informa que a base já está atualizada até o 3808

#### Scenario: Concursos novos
- **WHEN** o último concurso gravado é o 3808 e a Caixa informa o 3811 como último divulgado
- **THEN** os concursos 3809, 3810 e 3811 são buscados, nessa ordem, e gravados

#### Scenario: Lacuna na base
- **WHEN** a base tem os concursos 1 a 100 e 102 a 3808, e a Caixa informa o 3808 como último divulgado
- **THEN** o concurso 101 não é buscado e a atualização informa que a base já está atualizada

### Requirement: Pausa entre requisições à Caixa
Entre o início de uma requisição à Caixa e o início da seguinte MUST haver um intervalo mínimo, configurável em
milissegundos (padrão 500). A regra vale para todas as requisições à Caixa feitas pela aplicação, inclusive o
download do histórico.

#### Scenario: Requisições seguidas
- **WHEN** a pausa configurada é 500 ms e a atualização busca três concursos
- **THEN** cada requisição começa pelo menos 500 ms depois do início da anterior

#### Scenario: Pausa zero
- **WHEN** a pausa configurada é 0
- **THEN** as requisições são feitas sem espera

### Requirement: Leitura do concurso da API
Cada resposta da API SHALL ser convertida em concurso com as regras de validade já existentes:
- número: `numero`; data: `dataApuracao` (`dd/MM/aaaa`);
- dezenas: `listaDezenas`; ordem do sorteio: `dezenasSorteadasOrdemSorteio` (concurso sem ordem quando a lista
  vier vazia ou ausente);
- rateio: `listaRateioPremio`, em que a faixa 1 corresponde a 15 acertos, a 2 a 14, a 3 a 13, a 4 a 12 e a 5 a
  11, conferindo a descrição da faixa (`"15 acertos"` ... `"11 acertos"`); ganhadores em `numeroDeGanhadores` e
  prêmio em `valorPremio`;
- arrecadação: `valorArrecadado`; acumulado da faixa 15: `valorAcumuladoProximoConcurso`; estimativa do próximo:
  `valorEstimadoProximoConcurso`; acumulado da Independência: `valorAcumuladoConcursoEspecial`.

Valores monetários MUST ser preservados com os centavos exatos. A conversão MUST falhar, interrompendo a
atualização, quando:
- a resposta não for um JSON com os campos acima no formato esperado: "A resposta da Caixa para o concurso
  <número> não está no formato esperado.";
- a resposta trouxer um número diferente do pedido: "A Caixa devolveu o concurso <devolvido> ao buscar o concurso
  <pedido>.";
- o concurso não atender às regras de validade: "Concurso <número>: <motivo>".

Para a consulta do último concurso, a mensagem de formato inválido MUST ser "A resposta da Caixa para o último
concurso não está no formato esperado.".

#### Scenario: Concurso completo com ordem do sorteio
- **WHEN** a API devolve o concurso 3808, de 19/10/2026, com as dezenas em ordem crescente, a ordem do sorteio
  13, 10, 22, ..., 17, 1 ganhador de 15 acertos com prêmio 1456047.72 e arrecadação 17703532.0
- **THEN** o concurso 3808 é gravado com essa data, essas dezenas, essa ordem do sorteio, prêmio de
  R$ 1.456.047,72 na faixa 15 e arrecadação de R$ 17.703.532,00

#### Scenario: Descrição de faixa inesperada
- **WHEN** a faixa 1 do rateio vem com a descrição "14 acertos"
- **THEN** a atualização é interrompida com a mensagem de formato inesperado para aquele concurso

#### Scenario: Número diferente do pedido
- **WHEN** ao buscar o concurso 3809 a Caixa devolve o concurso 3808
- **THEN** a atualização é interrompida com a mensagem "A Caixa devolveu o concurso 3808 ao buscar o concurso
  3809."

### Requirement: Concurso sem rateio divulgado
Um concurso cujo rateio ainda não foi divulgado (lista de rateio vazia, ou todas as faixas sem nenhum ganhador)
MUST NOT ser gravado. A atualização MUST parar nele com a mensagem "O concurso <número> já foi sorteado, mas a
Caixa ainda não divulgou o rateio. Tente atualizar mais tarde.".

#### Scenario: Concurso recém-sorteado
- **WHEN** a atualização busca os concursos 3809 e 3810 e o 3810 vem com a lista de rateio vazia
- **THEN** o 3809 é gravado e o 3810 não
- **AND** a atualização informa que o rateio do concurso 3810 ainda não foi divulgado

### Requirement: Falhas de comunicação com a Caixa
Falhas de comunicação MUST interromper a atualização com uma mensagem que identifique a causa e a operação, em
que `<operação>` é "buscar o último concurso" ou "buscar o concurso <número>":
- sem conexão ou erro de rede: "Não foi possível conectar à Caixa para <operação>.";
- resposta com código diferente de sucesso: "A Caixa respondeu com o código <código> ao <operação>.";
- tempo limite excedido: "A Caixa não respondeu em <segundos> segundos ao <operação>.".

As mensagens da importação do histórico MUST continuar as mesmas.

#### Scenario: Caixa fora do ar ao consultar o último concurso
- **WHEN** a consulta do último concurso recebe o código 503
- **THEN** a atualização é interrompida com a mensagem "A Caixa respondeu com o código 503 ao buscar o último
  concurso."
- **AND** nenhum concurso é gravado

#### Scenario: Tempo limite ao buscar um concurso
- **WHEN** o tempo limite configurado é 60 segundos e a busca do concurso 3810 não recebe resposta nesse prazo
- **THEN** a atualização é interrompida com a mensagem "A Caixa não respondeu em 60 segundos ao buscar o concurso
  3810."

### Requirement: Gravação concurso a concurso
Cada concurso obtido SHALL ser gravado assim que for convertido, antes de buscar o seguinte. Se a atualização for
interrompida, os concursos já gravados MUST permanecer na base. Ao final, com ou sem interrupção, a atualização
MUST informar quantos concursos foram gravados e o número do último concurso da base.

#### Scenario: Interrupção no meio
- **WHEN** a atualização deve buscar os concursos 3809 a 3812 e a busca do 3811 falha
- **THEN** os concursos 3809 e 3810 ficam gravados e o 3811 e o 3812 não
- **AND** a atualização informa a falha, 2 concursos gravados e o 3810 como último concurso da base

#### Scenario: Atualização seguinte continua de onde parou
- **WHEN** depois da interrupção acima o usuário pede a atualização de novo
- **THEN** a busca começa no concurso 3811

### Requirement: Andamento da atualização
Durante a atualização, o sistema SHALL informar qual concurso está sendo buscado, sua posição e o total a buscar.

#### Scenario: Andamento de três concursos
- **WHEN** a atualização busca os concursos 3809 a 3811
- **THEN** o andamento informa, em sequência, 3809 (1 de 3), 3810 (2 de 3) e 3811 (3 de 3)

### Requirement: Registro de falhas no log
Toda atualização interrompida por falha SHALL ser registrada no log de erros da aplicação, com a mensagem exibida e
o detalhe técnico da causa.

#### Scenario: Falha registrada
- **WHEN** a atualização é interrompida porque a Caixa respondeu com o código 503
- **THEN** o log de erros recebe um registro dessa falha
