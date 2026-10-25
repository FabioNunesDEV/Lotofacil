## MODIFIED Requirements

### Requirement: Registro de erros em arquivo
A aplicação SHALL gravar em arquivo, na pasta `log` (configurável, relativa à pasta da aplicação ou com caminho
absoluto), as mensagens de nível Erro ou mais grave, com data e hora, origem, mensagem e detalhes da exceção.
Mensagens de nível inferior a Erro MUST NOT ser gravadas. Deve existir um arquivo por dia, e a pasta SHALL ser criada
automaticamente se não existir.

#### Scenario: Erro gravado
- **WHEN** ocorre um erro registrado pela aplicação, com ou sem exceção
- **THEN** o arquivo do dia na pasta `log` recebe uma linha com data e hora, nível, origem e mensagem
- **AND** se houver exceção, o tipo, a mensagem e a pilha de chamadas são gravados logo abaixo

#### Scenario: Mensagem informativa ou de aviso
- **WHEN** a aplicação registra uma mensagem de nível Informação ou Aviso
- **THEN** nada é gravado no arquivo de log

#### Scenario: Pasta de log inexistente
- **WHEN** a aplicação registra um erro e a pasta `log` ainda não existe
- **THEN** a pasta é criada e o erro é gravado

#### Scenario: Pasta de log com caminho absoluto
- **WHEN** a configuração aponta a pasta de log para `D:\Dados\Lotofacil\log` e a aplicação registra um erro
- **THEN** o erro é gravado nessa pasta, e não na pasta da aplicação
