## Why

A estrutura da solução está pronta, mas ainda não existe onde guardar os resultados. Todas as fatias seguintes
dependem de concursos gravados na base local: a importação do histórico (XLSX), a atualização incremental (API
JSON), as estatísticas e a conferência de jogos. Esta fatia cria o modelo do concurso, o banco SQLite e a primeira
tela que lê dele. Assim, a importação da próxima fatia já terá onde gravar e onde mostrar os dados.

## What Changes

- Criar no domínio a entidade **Concurso** com as regras de validade: número, data, 15 dezenas distintas entre 01
  e 25, ordem do sorteio opcional (o XLSX não a traz), rateio das faixas 11 a 15 (ganhadores e valor do prêmio) e
  os valores do concurso (arrecadação total, acumulado da faixa 15, estimativa do próximo concurso e acumulado da
  Lotofácil da Independência).
- Definir no domínio a interface do repositório de concursos e implementá-la na infraestrutura com EF Core e
  SQLite. O banco fica num arquivo local configurável e é criado ou atualizado por migrations ao iniciar a
  aplicação.
- Criar na aplicação um serviço de consulta de concursos (lista paginada e busca por número), usado pela tela.
- Criar a página **Resultados** (`/resultados`) e o item correspondente no menu, como no protótipo: lista dos
  concursos gravados, do mais recente ao mais antigo, com paginação, busca por número, detalhe do rateio ao clicar
  na linha e mensagem própria quando a base estiver vazia.
- Usar a cultura pt-BR na interface (datas `dd/MM/aaaa`, valores em R$).
- Testes de unidade para as regras do concurso, o serviço e a página (bUnit) e testes de integração do
  repositório com SQLite em memória, incluindo a verificação de que as migrations estão em dia com o modelo.

## Capabilities

### New Capabilities
- `concursos`: dados de um concurso da Lotofácil, regras de validade e gravação na base local SQLite, criada e
  atualizada automaticamente na inicialização.
- `resultados`: tela de consulta dos concursos gravados, com lista paginada, busca por número e detalhe do
  rateio.

### Modified Capabilities
<!-- Nenhuma: o novo item de menu segue a regra existente de listar apenas páginas disponíveis. -->

## Impact

- Novos códigos em `Lotofacil.Dominio` (entidade e interface), `Lotofacil.Aplicacao` (serviço de consulta),
  `Lotofacil.Infraestrutura` (`DbContext`, mapeamento, migrations, repositório, registro na injeção de
  dependência) e `Lotofacil.Web` (página, componentes, `Program.cs`, `appsettings.json`).
- Pacotes autorizados que entram agora: `Microsoft.EntityFrameworkCore.Sqlite` e as ferramentas de migrations do
  EF Core (`Microsoft.EntityFrameworkCore.Design` e a ferramenta local `dotnet-ef`).
- Novo arquivo de banco (`lotofacil.db`) ao lado da aplicação em execução; `.gitignore` passa a ignorar
  arquivos do SQLite.
- A base continua vazia até a próxima fatia (`importacao-historico`); nesta fatia os dados só entram pelos
  testes.
- Fora do escopo: importação do XLSX, chamada à API da Caixa, ganhadores por cidade/UF, campo Observação,
  estatísticas, jogos e o resumo da base no Painel e na barra superior.
