## ADDED Requirements

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

## MODIFIED Requirements

### Requirement: Base vazia
Quando a base local não tiver nenhum concurso, a página SHALL exibir a mensagem "Nenhum concurso gravado na base
local. Use “Importar histórico” para carregar os concursos da Caixa." no lugar da lista.

#### Scenario: Abrir a página sem concursos
- **WHEN** o usuário abre a página e a base está vazia
- **THEN** a mensagem "Nenhum concurso gravado na base local. Use “Importar histórico” para carregar os
  concursos da Caixa." é exibida
