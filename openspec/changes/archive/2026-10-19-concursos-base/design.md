## Context

A solução tem os seis projetos, StyleCop com avisos tratados como erro e a página Painel vazia (change
`estrutura-solucao`). Ainda não há banco, EF Core nem serviços próprios registrados. O protótipo
(`prototipo/index.html`, seção Resultados) define colunas, detalhe e busca da página. Os formatos de origem dos
dados estão em `docs/lotofacil-caixa-regras-e-dados.md`: o XLSX não traz a ordem do sorteio, e concursos antigos
têm arrecadação e estimativa zeradas e prêmios fixos diferentes dos atuais. Por isso o concurso guarda o rateio de
**todas** as faixas.

Na máquina há o `dotnet-ef` 6.0.10 instalado globalmente, que não funciona com o EF Core 10.

## Goals / Non-Goals

**Goals:**
- Modelo de concurso que só existe em estado válido, sem depender do banco.
- Banco criado e migrado sem intervenção do usuário.
- Migrations verificadas por teste: alterar o modelo sem gerar a migration quebra os testes.

**Non-Goals:**
- Consultas agregadas no banco (frequência, atraso): as estatísticas serão calculadas em memória numa fatia
  futura. São cerca de 3.800 concursos, e todos cabem na memória.
- Paginação além de "pular N, pegar M": não há filtros por data nem por dezena nesta fatia.
- Edição ou exclusão de concursos pela interface.

## Decisions

### Domínio: entidade `Concurso` imutável e validada no construtor
`Concurso` recebe todos os dados no construtor, valida e não tem *setters* públicos. Se a regra falhar, lança
`ArgumentException` (ou `ArgumentOutOfRangeException`) com a mensagem em português exigida pela spec. As dezenas
ficam como `IReadOnlyList<int>`, ordenadas no construtor. `OrdemSorteio` é `IReadOnlyList<int>?`, e o rateio é
uma lista de `Rateio` (`Faixa`, `Ganhadores`, `ValorPremio`). Os valores monetários são `decimal` e a data é
`DateOnly`.

Há também um construtor privado sem parâmetros, usado só pelo EF Core para materializar a entidade.

*Alternativas:* um *value object* `Dezena` (inteiro de 1 a 25), descartado porque a regra cabe na validação do
concurso e o tipo extra se espalharia por todas as fatias sem ganho concreto. Validar fora da entidade, com um
validador separado, também foi descartado: permitiria existir um `Concurso` inválido.

Constantes da Lotofácil (`DezenaMinima = 1`, `DezenaMaxima = 25`, `DezenasSorteadas = 15`, faixas de 11 a 15)
ficam numa classe estática `RegrasLotofacil` no domínio. As próximas fatias (geração e conferência) também vão
usá-las.

### Domínio: interface `IConcursoRepositorio`
Somente o que esta fatia usa, já pensando na importação da próxima:
- `AdicionarAsync(IReadOnlyCollection<Concurso>, CancellationToken)`: grava em uma transação. Número repetido
  faz a operação inteira falhar.
- `ObterAsync(int numero, CancellationToken)` → `Concurso?`.
- `ListarAsync(int ignorar, int quantidade, CancellationToken)` → concursos em ordem decrescente de número.
- `ContarAsync(CancellationToken)`.

A busca do último número gravado entra na fatia de importação, quando for usada.

### Aplicação: `ConsultaConcursosServico`
Classe concreta com o repositório injetado:
- `ListarPaginaAsync(int pagina, int tamanhoPagina)` → `PaginaConcursos(IReadOnlyList<Concurso> Itens, int Total)`.
  Converte página e tamanho em "pular/pegar" e valida os argumentos (página ≥ 0, tamanho entre 1 e 100).
- `ObterAsync(int numero)` → `Concurso?`.

A página fala com este serviço, nunca com o repositório. Não há interface para o serviço: nos testes bUnit
registra-se o serviço real com um `IConcursoRepositorio` falso (NSubstitute), o que também exercita a conversão
de página.

*Alternativa:* injetar o repositório direto na página. Seria mais curto, mas a regra de paginação ficaria no
componente, o que o `CLAUDE.md` proíbe.

### Web pode usar tipos do domínio (decisão do usuário na implementação)
O serviço devolve as próprias entidades (`Concurso`, `Rateio`), e a página as exibe. Com isso, o assembly da Web
passa a referenciar `Lotofacil.Dominio` diretamente. A tabela de dependências do `CLAUDE.md` e o teste de
arquitetura são atualizados para permitir Web → Dominio. Dominio e Aplicacao continuam sem conhecer Web e
Infraestrutura.
*Alternativa descartada:* modelos de leitura na Aplicação (ex.: `ResumoConcurso`), que duplicariam a estrutura do
concurso e exigiriam mapeamento a cada fatia.

### Infraestrutura: mapeamento EF Core
- `LotofacilContexto` (`DbContext`) com `DbSet<Concurso> Concursos`. O mapeamento fica em uma classe
  `IEntityTypeConfiguration<Concurso>` separada, o que mantém o contexto livre de regras.
- Tabela `Concurso`: `Numero` (chave, sem autoincremento), `Data`, `Dezenas`, `OrdemSorteio` (nulo), `Arrecadacao`,
  `Acumulado15`, `EstimativaProximo`, `AcumuladoIndependencia`.
- **Dezenas e ordem do sorteio** em uma coluna cada, como coleção primitiva do EF Core (JSON no SQLite, ex.:
  `[1,2,4,5,7,...]`). São sempre lidas junto com o concurso, nunca filtradas no banco.
  *Alternativa:* tabela `ConcursoDezena` (uma linha por dezena), como sugere o modelo em `docs/`. Seriam 57 mil
  linhas e um *join* para cada leitura, só para permitir consultas SQL por dezena, que não serão usadas (ver
  Non-Goals).
- **Rateio** em tabela própria `ConcursoRateio` (`NumeroConcurso`, `Faixa`, `Ganhadores`, `ValorPremio`), com
  chave composta (`NumeroConcurso`, `Faixa`), mapeada como coleção *owned* (`OwnsMany`). Fica legível para quem
  consulta o banco diretamente e prepara a conferência, que lê o prêmio da faixa.
- `decimal` é gravado como TEXT pelo provedor SQLite, o que preserva os centavos exatamente. Por causa disso, o
  SQLite não ordena nem soma `decimal` no banco, o que é aceitável pelos Non-Goals.

### Infraestrutura: `IDbContextFactory` no repositório
No Blazor Server, um serviço *scoped* vive o circuito inteiro (a aba aberta). Um `DbContext` *scoped* seria
compartilhado entre operações concorrentes e acumularia entidades rastreadas. O repositório recebe
`IDbContextFactory<LotofacilContexto>` e cria um contexto por operação. As leituras usam `AsNoTracking()`. Essa é
a recomendação da Microsoft para o Blazor Server.

### Infraestrutura: registro e inicialização
- Uma classe estática `ConfiguracaoInfraestrutura` com:
  - `AddInfraestrutura(this IServiceCollection, string caminhoBanco)`: registra a *factory* do contexto (SQLite,
    `Data Source=<caminho>`) e `IConcursoRepositorio`.
  - `InicializarBancoDadosAsync(this IServiceProvider)`: cria um contexto pela *factory* e executa
    `Database.MigrateAsync()`. Isso cria o arquivo se ele não existir e aplica as migrations pendentes.
- O `Program.cs` lê `BancoDados:Arquivo` (padrão `lotofacil.db`), resolve o caminho a partir do
  `ContentRootPath` (mesmo padrão do log), registra o serviço de consulta e chama a inicialização antes de
  `app.Run()`.
- `*.db`, `*.db-shm` e `*.db-wal` entram no `.gitignore`.

### Migrations
- Pacote `Microsoft.EntityFrameworkCore.Design` na Infraestrutura com `PrivateAssets="all"`, e
  `LotofacilContextoFabricaProjeto` (`IDesignTimeDbContextFactory`) também na Infraestrutura. Assim o comando roda
  sem depender da Web: `dotnet ef migrations add <Nome> --project src/Lotofacil.Infraestrutura`.
- `dotnet-ef` como **ferramenta local** (`dotnet-tools.json` na raiz, local padrão do SDK 10), fixada na mesma versão do EF Core.
  A ferramenta global instalada (6.0.10) não serve, e a local garante a mesma versão para qualquer pessoa que
  clonar o repositório.
- Migrations geradas ficam em `Lotofacil.Infraestrutura/Migracoes`. Os arquivos gerados não seguem o StyleCop
  (cabeçalho, documentação). Por isso a pasta recebe, no `.editorconfig`, `generated_code = true`, o que faz os
  analisadores ignorarem esses arquivos. As migrations são código gerado e não devem ser editadas à mão.
- Primeira migration: `CriacaoConcursos`.

### Web: página Resultados
- `Components/Pages/Resultados.razor` + `.razor.cs`, rota `/resultados`. Novo `MudNavLink` "Resultados" no
  `MainLayout`, logo abaixo de "Painel".
- `MudTable` com `ServerData`: a cada troca de página, chama `ListarPaginaAsync`. `MudTablePager` com opções
  10/25/50 e textos em português (`RowsPerPageString`, `InfoFormat` = `{first_item}–{last_item} de {all_items}`).
- O detalhe aparece em `ChildRowContent`, controlado por um conjunto de números expandidos que alterna a cada
  clique. Fica num componente próprio, `DetalheConcurso.razor`, com a tabela de rateio, a arrecadação e a ordem do
  sorteio.
- As dezenas aparecem num componente `Dezenas.razor` (bolinhas com dois dígitos, no estilo do protótipo), que as
  próximas fatias vão reutilizar.
- Busca: `MudNumericField`. Com um número, a tabela passa a usar o resultado de `ObterAsync` como única linha, ou
  mostra a mensagem de não encontrado. Vazio, volta ao `ServerData` normal (`ReloadServerData`).
- Base vazia: o conteúdo `NoRecordsContent` da `MudTable` mostra a mensagem da spec.
- As regras de apresentação ("—" sem ganhador, "Não informada" para arrecadação zero, situação "Saiu"/"Acumulou")
  são condicionais de exibição e ficam no componente. Não são regra de negócio.

### Cultura pt-BR
`app.UseRequestLocalization` com pt-BR como cultura padrão e única. Os componentes formatam com `ToString("C")` e
`ToString("dd/MM/yyyy")` sem montar cultura à mão. Nos testes bUnit, a cultura pt-BR é definida no início de cada
teste da página (`CultureInfo.CurrentCulture`).

### Testes
- **Unidade, domínio:** um teste por regra de validação da spec (número, quantidade, repetida, intervalo, ordem,
  rateio, negativos, zeros aceitos) e a ordenação das dezenas.
- **Unidade, aplicação:** `ConsultaConcursosServico` com repositório NSubstitute: conversão de página em
  pular/pegar, total, argumentos inválidos.
- **Unidade, bUnit:** página Resultados com o serviço real sobre repositório falso: colunas e formatação, "—" e
  "Acumulou", paginação (1–10 de 23), detalhe abre e fecha, ordem do sorteio, "Não informada", busca encontrada e
  não encontrada, limpar busca, base vazia, item de menu.
- **Integração, SQLite em memória** (`Data Source=:memory:`, com a conexão aberta durante o teste e
  `MigrateAsync()`): gravar e recuperar com centavos, inexistente devolve nulo, lote com número repetido não grava
  nada, listagem decrescente com pular/pegar e contagem. Mais um teste que confirma
  `Database.HasPendingModelChanges() == false` (modelo e migrations em dia), e outro que roda
  `InicializarBancoDadosAsync` sobre um arquivo numa pasta temporária, confere que o arquivo foi criado e que, numa
  segunda execução, os dados continuam lá.

## Risks / Trade-offs

- [Coleções primitivas em JSON não permitem filtrar por dezena no SQL] → Aceito pelos Non-Goals. Se uma fatia
  futura precisar, uma migration pode extrair as dezenas para uma tabela.
- [`decimal` como TEXT impede `ORDER BY`/`SUM` em SQL] → As consultas desta e das próximas fatias usam número e
  data. Somas de prêmios, se surgirem, serão feitas em memória.
- [`MigrateAsync` na inicialização altera o banco sem pedir confirmação] → É uma aplicação local de usuário único,
  e as migrations são aditivas. Antes de uma migration destrutiva no futuro, avaliar uma cópia de segurança do
  arquivo.
- [Arquivo do banco aberto em outro programa (ex.: DB Browser) com escrita pendente] → O SQLite usa bloqueio de
  arquivo. A aplicação registra o erro no log, e o usuário fecha o outro programa.
- [Supressão do StyleCop na pasta `Migracoes` poderia esconder código escrito à mão ali] → Só código gerado fica
  nessa pasta. A *factory* de design e o mapeamento ficam fora dela.
- [Formatação de moeda depende da cultura do processo nos testes] → Os testes da página definem pt-BR
  explicitamente.

## Migration Plan

Não há dados nem versão anterior: a primeira execução cria o banco. Para voltar atrás, basta apagar o arquivo
`lotofacil.db`.
