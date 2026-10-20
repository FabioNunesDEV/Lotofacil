# resultados Specification

## Purpose

Permite ao usuário consultar os concursos gravados na base local: o resultado de cada sorteio, o rateio por faixa
de acertos e os valores do concurso.

## Requirements

### Requirement: Acesso à página Resultados
A aplicação SHALL oferecer a página "Resultados" na rota `/resultados`, acessível por um item "Resultados" no menu
lateral, logo abaixo de "Painel".

#### Scenario: Abrir pelo menu
- **WHEN** o usuário aciona o item "Resultados" do menu lateral
- **THEN** a página "Resultados" é exibida com o título "Resultados"
- **AND** o item "Resultados" do menu aparece como selecionado

### Requirement: Lista de concursos
A página SHALL listar os concursos gravados, do mais recente ao mais antigo, em páginas de 10 concursos (com
opção de 25 ou 50 por página), mostrando o total de concursos da base. Cada linha MUST exibir:
- número do concurso;
- data do sorteio no formato `dd/MM/aaaa`;
- as 15 dezenas em ordem crescente, com dois dígitos (`01` a `25`);
- quantidade de ganhadores de 15 acertos;
- prêmio de 15 acertos em reais (`R$ 1.234,56`), ou "—" quando não houve ganhador;
- situação: "Saiu" quando houve ganhador de 15 acertos, "Acumulou" quando não houve.

#### Scenario: Primeira página
- **WHEN** o usuário abre a página e a base tem 23 concursos, do 1 ao 23
- **THEN** a lista mostra os concursos 23 a 14, nessa ordem
- **AND** a paginação indica 1–10 de 23

#### Scenario: Próxima página
- **WHEN** o usuário avança para a página seguinte
- **THEN** a lista mostra os concursos 13 a 4

#### Scenario: Concurso sem ganhador de 15 acertos
- **WHEN** a lista exibe um concurso sem ganhador de 15 acertos
- **THEN** a coluna de prêmio mostra "—" e a situação mostra "Acumulou"

#### Scenario: Concurso com ganhador de 15 acertos
- **WHEN** a lista exibe um concurso com 2 ganhadores de 15 acertos e prêmio de R$ 801.455,92
- **THEN** a linha mostra 2 ganhadores, "R$ 801.455,92" e a situação "Saiu"

### Requirement: Detalhe do concurso
Ao clicar na linha de um concurso, a página SHALL exibir logo abaixo dela o detalhe do concurso; clicar de novo
MUST ocultá-lo. O detalhe contém:
- o rateio das faixas de 15 a 11 acertos, nessa ordem, com a quantidade de ganhadores e o prêmio de cada faixa,
  ou "—" no prêmio quando a faixa não teve ganhador;
- a arrecadação total, ou "Não informada" quando o valor gravado for zero;
- a ordem do sorteio, quando conhecida.

#### Scenario: Abrir e fechar o detalhe
- **WHEN** o usuário clica na linha de um concurso
- **THEN** o rateio das cinco faixas é exibido abaixo da linha
- **AND** um novo clique na mesma linha oculta o detalhe

#### Scenario: Concurso com ordem do sorteio
- **WHEN** o detalhe é exibido para um concurso com ordem do sorteio conhecida
- **THEN** as dezenas aparecem também na ordem em que foram sorteadas

#### Scenario: Concurso antigo sem arrecadação
- **WHEN** o detalhe é exibido para um concurso com arrecadação zero e sem ordem do sorteio
- **THEN** a arrecadação aparece como "Não informada"
- **AND** a ordem do sorteio não é exibida

### Requirement: Busca por número
A página SHALL ter um campo de busca por número de concurso. Ao buscar um número, a lista MUST mostrar apenas esse
concurso; se ele não estiver na base, a página MUST informar "Concurso <número> não encontrado na base local".
Limpar o campo SHALL voltar à lista completa.

#### Scenario: Concurso encontrado
- **WHEN** o usuário busca o número 3805 e esse concurso está na base
- **THEN** a lista mostra somente o concurso 3805

#### Scenario: Concurso não encontrado
- **WHEN** o usuário busca o número 9999 e esse concurso não está na base
- **THEN** a página mostra "Concurso 9999 não encontrado na base local"

#### Scenario: Limpar a busca
- **WHEN** o usuário limpa o campo de busca
- **THEN** a lista volta a mostrar todos os concursos a partir do mais recente

### Requirement: Base vazia
Quando a base local não tiver nenhum concurso, a página SHALL exibir a mensagem "Nenhum concurso gravado na base
local. Use “Importar histórico” para carregar os concursos da Caixa." no lugar da lista.

#### Scenario: Abrir a página sem concursos
- **WHEN** o usuário abre a página e a base está vazia
- **THEN** a mensagem "Nenhum concurso gravado na base local. Use “Importar histórico” para carregar os
  concursos da Caixa." é exibida

### Requirement: Importar histórico pela página Resultados
A página Resultados SHALL ter, no cabeçalho, o botão "Importar histórico", que inicia a importação do histórico
completo de concursos da Caixa. Enquanto a importação estiver em andamento, o botão MUST ficar desabilitado e a
página MUST indicar que o histórico está sendo importado. Ao final:
- com concursos novos gravados, a página MUST exibir "Importação concluída: <lidos> concursos no arquivo,
  <gravados> gravados na base local." e recarregar a lista a partir do concurso mais recente;
- sem nenhum concurso novo, a página MUST exibir "A base local já tem todos os <lidos> concursos do arquivo; nada
  foi gravado.";
- em caso de falha, a página MUST exibir "A importação falhou e nada foi gravado. <motivo>", em que `<motivo>` é a
  mensagem da falha; para uma falha inesperada, o motivo é "Erro inesperado; os detalhes foram registrados no
  log.".

As quantidades MUST ser exibidas com separador de milhar (`3.808`).

#### Scenario: Importação com concursos novos
- **WHEN** o usuário aciona "Importar histórico" com a base vazia e o arquivo da Caixa tem 3.808 concursos válidos
- **THEN** a página exibe "Importação concluída: 3.808 concursos no arquivo, 3.808 gravados na base local."
- **AND** a lista passa a mostrar os concursos, a partir do 3.808

#### Scenario: Nada novo para gravar
- **WHEN** o usuário aciona "Importar histórico" e todos os 3.808 concursos do arquivo já estão na base
- **THEN** a página exibe "A base local já tem todos os 3.808 concursos do arquivo; nada foi gravado."

#### Scenario: Importação em andamento
- **WHEN** o usuário aciona "Importar histórico" e o download ainda não terminou
- **THEN** o botão "Importar histórico" fica desabilitado e a página indica que o histórico está sendo importado

#### Scenario: Falha na importação
- **WHEN** o usuário aciona "Importar histórico" e a Caixa responde com o código 503
- **THEN** a página exibe "A importação falhou e nada foi gravado. A Caixa respondeu com o código 503 ao baixar o
  histórico."
- **AND** o botão volta a ficar habilitado

#### Scenario: Falha inesperada
- **WHEN** o usuário aciona "Importar histórico" e ocorre um erro não previsto (por exemplo, na gravação da base)
- **THEN** a página exibe "A importação falhou e nada foi gravado. Erro inesperado; os detalhes foram registrados
  no log."

### Requirement: Atualizar resultados pela página Resultados
A página Resultados SHALL ter, no cabeçalho, ao lado de "Importar histórico", o botão "Atualizar resultados", que
inicia a atualização da base pela API da Caixa. Enquanto a atualização **ou** a importação estiver em andamento,
os botões "Atualizar resultados" e "Importar histórico" MUST ficar desabilitados. Durante a atualização, a página
MUST mostrar "Consultando o último concurso da Caixa…" e, depois, "Buscando o concurso <número> (<posição> de
<total>)…". Ao final, a página MUST exibir:
- com a base vazia: "A base local está vazia. Use “Importar histórico” antes de atualizar os resultados.";
- sem concursos novos: "A base local já está atualizada: último concurso <número>.";
- com 1 concurso gravado: "Atualização concluída: 1 concurso novo gravado (<número>).";
- com mais de 1 concurso gravado: "Atualização concluída: <quantidade> concursos novos gravados (<primeiro> a
  <último>).";
- em caso de interrupção: "A atualização foi interrompida. <motivo>", seguida, quando algum concurso tiver sido
  gravado antes da falha, de " Concursos gravados antes da falha: <quantidade> (até o <último>)."; para uma falha
  inesperada, o motivo é "Erro inesperado; os detalhes foram registrados no log.".

Quando algum concurso for gravado, a lista MUST ser recarregada a partir do concurso mais recente. Números de
concurso MUST ser exibidos sem separador de milhar, como na lista (`3809`).

#### Scenario: Atualização com concursos novos
- **WHEN** o último concurso gravado é o 3808 e o usuário aciona "Atualizar resultados" com o 3811 já divulgado
- **THEN** a página exibe "Atualização concluída: 3 concursos novos gravados (3809 a 3811)."
- **AND** a lista passa a começar pelo concurso 3811

#### Scenario: Um concurso novo
- **WHEN** o usuário aciona "Atualizar resultados" e só o concurso 3809 é novo
- **THEN** a página exibe "Atualização concluída: 1 concurso novo gravado (3809)."

#### Scenario: Base já atualizada
- **WHEN** o usuário aciona "Atualizar resultados" e o 3808 é o último gravado e o último divulgado
- **THEN** a página exibe "A base local já está atualizada: último concurso 3808."

#### Scenario: Base vazia
- **WHEN** o usuário aciona "Atualizar resultados" com a base vazia
- **THEN** a página exibe "A base local está vazia. Use “Importar histórico” antes de atualizar os resultados."

#### Scenario: Andamento
- **WHEN** a atualização está buscando o concurso 3810, segundo de três
- **THEN** a página mostra "Buscando o concurso 3810 (2 de 3)…"
- **AND** os botões "Atualizar resultados" e "Importar histórico" estão desabilitados

#### Scenario: Importação em andamento bloqueia a atualização
- **WHEN** a importação do histórico está em andamento
- **THEN** o botão "Atualizar resultados" também fica desabilitado

#### Scenario: Interrupção depois de gravar
- **WHEN** a atualização grava os concursos 3809 e 3810 e a busca do 3811 recebe o código 503
- **THEN** a página exibe "A atualização foi interrompida. A Caixa respondeu com o código 503 ao buscar o concurso
  3811. Concursos gravados antes da falha: 2 (até o 3810)."
- **AND** a lista passa a começar pelo concurso 3810

#### Scenario: Falha inesperada
- **WHEN** ocorre um erro não previsto durante a atualização (por exemplo, na gravação da base)
- **THEN** a página exibe "A atualização foi interrompida. Erro inesperado; os detalhes foram registrados no log."
