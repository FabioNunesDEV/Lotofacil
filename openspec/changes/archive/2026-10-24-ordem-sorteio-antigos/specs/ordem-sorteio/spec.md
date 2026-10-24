## Purpose

Preenche, pela API da Caixa, a ordem do sorteio dos concursos gravados sem ela (os importados pelo histórico),
concurso a concurso, com andamento, interrupção pelo usuário, falhas puladas e resumo no fim.

## ADDED Requirements

### Requirement: Botão na página Resultados
A página Resultados SHALL ter, no cabeçalho, ao lado de "Atualizar resultados" e "Importar histórico", o botão
"Preencher ordem do sorteio". O botão MUST ficar habilitado só quando houver algum concurso gravado sem a ordem do
sorteio e nenhuma importação, atualização ou preenchimento estiver em andamento. Enquanto o preenchimento estiver em
andamento, "Atualizar resultados" e "Importar histórico" MUST ficar desabilitados.

#### Scenario: Concursos sem ordem
- **WHEN** a base tem os concursos 1 a 3812 e só os 3809 a 3812 têm a ordem do sorteio
- **THEN** o botão "Preencher ordem do sorteio" está habilitado

#### Scenario: Todos com ordem
- **WHEN** todos os concursos gravados têm a ordem do sorteio, ou a base está vazia
- **THEN** o botão "Preencher ordem do sorteio" está desabilitado

#### Scenario: Outra operação em andamento
- **WHEN** a atualização dos resultados ou a importação do histórico está em andamento
- **THEN** o botão "Preencher ordem do sorteio" está desabilitado

### Requirement: Busca da ordem concurso a concurso
Ao acionar o botão, o sistema SHALL buscar na API da Caixa (`<URL base>/api/lotofacil/<número>`), do menor para o
maior número, cada concurso gravado sem a ordem do sorteio, respeitando a mesma pausa entre requisições da
atualização. A ordem MUST ser lida de `dezenasSorteadasOrdemSorteio` e gravada assim que obtida, antes de buscar o
concurso seguinte, desde que tenha as mesmas 15 dezenas gravadas para o concurso. Só a ordem do sorteio MUST ser
gravada: dezenas, data, rateio e valores do concurso não mudam, e o rateio da resposta não é exigido. Se, ao
acionar, nenhum concurso estiver sem ordem, a página MUST exibir "Todos os concursos já têm a ordem do sorteio.".

#### Scenario: Ordem gravada
- **WHEN** o concurso 1 está gravado com as dezenas 02 03 05 06 09 10 11 13 14 16 18 20 23 24 25 e sem ordem, e a API
  devolve para ele a ordem 18 20 25 23 10 11 24 14 06 02 13 09 05 16 03
- **THEN** o concurso 1 passa a ter essa ordem do sorteio, e o detalhe do concurso na página Resultados a mostra

#### Scenario: Só os que faltam
- **WHEN** os concursos 1 a 3812 estão gravados e só os 3809 a 3812 têm a ordem
- **THEN** a busca pede à API os concursos 1 a 3808, nessa ordem, e nenhum dos 3809 a 3812

#### Scenario: Demais dados preservados
- **WHEN** a ordem do concurso 1500 é gravada
- **THEN** as dezenas, a data, o rateio e os valores do concurso 1500 continuam os mesmos

### Requirement: Andamento e interrupção
Durante o preenchimento, a página SHALL mostrar "Buscando a ordem do sorteio do concurso <número> (<posição> de
<total>)…", com posição e total com separador de milhar e o número sem separador, e o botão "Interromper". Ao
acionar "Interromper", ou ao sair da página, a busca MUST parar sem pedir o próximo concurso; as ordens já gravadas
MUST permanecer, e um novo acionamento MUST buscar só os concursos que continuam sem ordem. Ao interromper, a
página MUST exibir "Preenchimento interrompido: ordem preenchida em <preenchidos>; <restantes> continuam sem
ordem.", com as quantidades no formato "1 concurso" ou "<n> concursos" e o verbo concordando com a quantidade
("1 concurso continua sem ordem").

#### Scenario: Andamento
- **WHEN** o preenchimento está buscando o concurso 1234, 1.234º de 3.808
- **THEN** a página mostra "Buscando a ordem do sorteio do concurso 1234 (1.234 de 3.808)…" e o botão "Interromper"
- **AND** os botões "Preencher ordem do sorteio", "Atualizar resultados" e "Importar histórico" estão desabilitados

#### Scenario: Interromper
- **WHEN** o usuário aciona "Interromper" depois de gravada a ordem dos concursos 1 a 500, de 3.808 a buscar
- **THEN** a página exibe "Preenchimento interrompido: ordem preenchida em 500 concursos; 3.308 concursos continuam
  sem ordem."
- **AND** os concursos 1 a 500 mantêm a ordem gravada

#### Scenario: Continuar de onde parou
- **WHEN** depois da interrupção acima o usuário aciona "Preencher ordem do sorteio" de novo
- **THEN** a busca começa no concurso 501, com total 3.308

### Requirement: Falhas puladas
Um concurso cuja ordem não puder ser obtida SHALL ser pulado, ficando sem ordem, e a busca MUST seguir com o
próximo. O limite de consultas da Caixa (código 429) não é falha: tem tratamento próprio. São falhas:
- comunicação com a Caixa, com as mensagens da atualização e a operação "buscar a ordem do concurso <número>"
  (por exemplo, "A Caixa respondeu com o código 503 ao buscar a ordem do concurso 1500.");
- resposta fora do formato esperado: "A resposta da Caixa para o concurso <número> não está no formato esperado.";
- resposta com outro número: "A Caixa devolveu o concurso <devolvido> ao buscar o concurso <pedido>.";
- resposta sem a ordem: "A Caixa não informou a ordem do sorteio do concurso <número>.";
- ordem com dezenas diferentes das gravadas: "A ordem do sorteio informada pela Caixa para o concurso <número> não
  corresponde às dezenas gravadas.".

Cada falha MUST ser registrada no log de erros com a mensagem e o detalhe técnico. Depois de 5 falhas seguidas, a
busca MUST ser encerrada, sem pedir o próximo concurso, e a página MUST exibir, até o usuário fechar, "Preenchimento
encerrado após 5 falhas seguidas. <mensagem da última falha> Ordem preenchida em <preenchidos>; <restantes>
continuam sem ordem.".

#### Scenario: Falha no meio
- **WHEN** a busca dos concursos 1499, 1500 e 1501 recebe, para o 1500, o código 503
- **THEN** as ordens do 1499 e do 1501 são gravadas, o 1500 continua sem ordem e a busca segue
- **AND** o log de erros recebe "A Caixa respondeu com o código 503 ao buscar a ordem do concurso 1500."

#### Scenario: Ordem divergente
- **WHEN** a API devolve para o concurso 10 uma ordem com uma dezena que não está entre as gravadas
- **THEN** o concurso 10 continua sem ordem e a busca segue

#### Scenario: Caixa fora do ar
- **WHEN** as buscas dos concursos 1 a 5 falham seguidas porque não é possível conectar à Caixa
- **THEN** a busca é encerrada sem pedir o concurso 6
- **AND** a página exibe "Preenchimento encerrado após 5 falhas seguidas. Não foi possível conectar à Caixa para
  buscar a ordem do concurso 5. Ordem preenchida em 0 concursos; 3.808 concursos continuam sem ordem."

#### Scenario: Sucesso zera a contagem
- **WHEN** 4 concursos seguidos falham, o seguinte é gravado e mais 4 seguidos falham
- **THEN** a busca não é encerrada por falhas seguidas

### Requirement: Limite de consultas da Caixa
Quando a Caixa responder com o código 429 (excesso de consultas) ao buscar a ordem de um concurso, o sistema SHALL
esperar e buscar o mesmo concurso de novo, sem contar como falha, com esperas de 1, 2, 4 e 8 minutos, nessa ordem.
Uma resposta sem 429 encerra as esperas, e o concurso segue as regras normais. Durante a espera, a página MUST
mostrar "A Caixa pediu uma pausa. Nova tentativa do concurso <número> às <HH:mm> (<posição> de <total>)…", com o
horário local da nova tentativa, e "Interromper" MUST interromper na hora, sem esperar o fim da pausa. Se a resposta
ainda for 429 depois da espera de 8 minutos, a busca MUST ser encerrada, a falha MUST ser registrada no log e a
página MUST exibir, até o usuário fechar, "Preenchimento encerrado: a Caixa continua limitando as consultas.
<mensagem da falha> Ordem preenchida em <preenchidos>; <restantes> continuam sem ordem.".

#### Scenario: Espera e nova tentativa
- **WHEN** a busca do concurso 38, 38º de 3.808, recebe o código 429 às 18:16 e a nova tentativa, um minuto depois,
  devolve a ordem
- **THEN** a página mostra "A Caixa pediu uma pausa. Nova tentativa do concurso 38 às 18:17 (38 de 3.808)…"
  durante a espera
- **AND** a ordem do concurso 38 é gravada, a busca segue no 39 e nenhuma falha é contada

#### Scenario: Interromper durante a espera
- **WHEN** o usuário aciona "Interromper" durante a espera
- **THEN** a busca para na hora e a página exibe a mensagem de interrupção

#### Scenario: Limite persistente
- **WHEN** a busca do concurso 38 recebe o código 429 na primeira tentativa e depois das esperas de 1, 2, 4 e 8
  minutos
- **THEN** a busca é encerrada sem pedir o concurso 39
- **AND** a página exibe "Preenchimento encerrado: a Caixa continua limitando as consultas. A Caixa respondeu com o
  código 429 ao buscar a ordem do concurso 38. Ordem preenchida em 37 concursos; 3.771 concursos continuam sem ordem."

### Requirement: Resumo do preenchimento
Ao terminar a busca de todos os concursos que faltavam, a página SHALL exibir, com as quantidades no formato
"1 concurso" ou "<n> concursos" e os verbos concordando com a quantidade:
- sem falhas: "Ordem do sorteio preenchida em <preenchidos>.";
- com falhas: "Ordem do sorteio preenchida em <preenchidos>; <sem ordem> ficaram sem ordem e podem ser buscados de
  novo." (no singular, "1 concurso ficou sem ordem e pode ser buscado de novo.").

Para um erro não previsto (por exemplo, na gravação da base), a busca MUST parar, as ordens já gravadas MUST
permanecer, o erro MUST ser registrado no log e a página MUST exibir "O preenchimento foi interrompido. Erro
inesperado; os detalhes foram registrados no log.". Quando alguma ordem tiver sido gravada, a lista de concursos MUST
ser recarregada, e o botão MUST refletir se ainda há concursos sem ordem.

#### Scenario: Tudo preenchido
- **WHEN** a busca grava a ordem dos 3.808 concursos que faltavam
- **THEN** a página exibe "Ordem do sorteio preenchida em 3.808 concursos."
- **AND** o botão "Preencher ordem do sorteio" fica desabilitado

#### Scenario: Com falhas
- **WHEN** a busca grava a ordem de 3.806 concursos e 2 falham
- **THEN** a página exibe "Ordem do sorteio preenchida em 3.806 concursos; 2 concursos ficaram sem ordem e podem
  ser buscados de novo."
- **AND** o botão "Preencher ordem do sorteio" continua habilitado

#### Scenario: Um concurso
- **WHEN** só o concurso 3808 estava sem ordem e ela é gravada
- **THEN** a página exibe "Ordem do sorteio preenchida em 1 concurso."

#### Scenario: Erro inesperado
- **WHEN** a gravação da ordem do concurso 20 falha por um erro não previsto
- **THEN** a página exibe "O preenchimento foi interrompido. Erro inesperado; os detalhes foram registrados no log."
- **AND** as ordens dos concursos 1 a 19 continuam gravadas
