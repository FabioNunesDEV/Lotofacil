## MODIFIED Requirements

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
