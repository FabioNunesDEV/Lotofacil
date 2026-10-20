## ADDED Requirements

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
