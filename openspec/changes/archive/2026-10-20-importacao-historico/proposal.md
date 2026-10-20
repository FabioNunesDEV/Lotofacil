## Why

A base local e a página Resultados já existem, mas a base continua vazia: hoje os concursos só entram pelos
testes. As estatísticas, a geração e a conferência de jogos dependem do histórico completo. A Caixa oferece todo o
histórico num único arquivo XLSX, a forma mais rápida de fazer a carga inicial (uma requisição em vez de cerca de
3.800 chamadas à API JSON, que fica para a atualização incremental da próxima fatia).

## What Changes

- Baixar da Caixa, **somente quando o usuário pedir**, o arquivo XLSX com o histórico completo da Lotofácil. A URL
  base fica na configuração (`servicebus3.caixa.gov.br/portaldeloterias`, o host já mudou uma vez) e o download
  tem tempo limite configurável.
- Ler a planilha da Caixa: colunas por posição (as células de dados não trazem referência de coluna), cabeçalho
  conferido para detectar mudança de layout, valores em reais como texto (`R$1.388,95`) e datas `dd/MM/aaaa`.
  Ordem do sorteio, cidade/UF dos ganhadores e observação não são importadas.
- Validar cada linha com as regras do concurso. Se qualquer linha for inválida, ou o arquivo não puder ser lido,
  **nada é gravado** e a mensagem informa a linha, o concurso e o problema.
- Gravar apenas os concursos que ainda não estão na base, num único lote. Importar de novo é seguro: os concursos
  já gravados são mantidos sem alteração.
- Na página **Resultados**, adicionar o botão "Importar histórico", com indicação de andamento, mensagem de
  resultado (concursos lidos e importados) ou de falha, e recarga da lista ao final. A mensagem de base vazia passa
  a orientar o uso do botão.
- Registrar no log de erros as falhas da importação.
- Testes de unidade (leitura da planilha, serviço de importação, página com bUnit) e de integração (download com
  cliente HTTP falso e gravação em SQLite em memória).

## Capabilities

### New Capabilities
- `importacao-historico`: carga do histórico completo de concursos a partir do XLSX oficial da Caixa, com
  validação de todo o arquivo antes de gravar e gravação somente dos concursos novos.

### Modified Capabilities
- `resultados`: a página ganha o botão "Importar histórico" e a mensagem de base vazia passa a indicar como
  carregar os concursos.

## Impact

- `Lotofacil.Dominio`: interface da fonte do histórico e exceção para falhas da fonte de resultados.
- `Lotofacil.Aplicacao`: serviço de importação do histórico e o resultado da importação.
- `Lotofacil.Infraestrutura`: download do XLSX da Caixa, leitor da planilha (`System.IO.Compression` e
  `XmlReader`, do próprio .NET), consulta dos números de concursos já gravados e registro na injeção de
  dependência.
- `Lotofacil.Web`: botão e mensagens na página Resultados, nova seção `FonteResultados` no `appsettings.json`.
- Nenhum pacote novo: o download usa o `HttpClient` do .NET.
- A aplicação passa a fazer requisição para a internet (somente ao clicar no botão).
- Fora do escopo: atualização incremental pela API JSON e ordem do sorteio (fatia `atualizacao-concursos`), envio
  de arquivo XLSX pelo usuário, página Configurações, resumo da base no Painel e na barra superior.
