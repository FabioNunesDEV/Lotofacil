## 1. Domínio

- [x] 1.1 Criar `RegrasLotofacil` (dezenas 1 a 25, 15 sorteadas, faixas 11 a 15) e o tipo `Rateio` (`Faixa`, `Ganhadores`, `ValorPremio`); verificar com `dotnet build src/Lotofacil.slnx` sem avisos
- [x] 1.2 Criar a entidade `Concurso` imutável, com validação no construtor (número, 15 dezenas distintas de 1 a 25, ordem do sorteio opcional com as mesmas dezenas, rateio com as cinco faixas, valores ≥ 0), dezenas ordenadas e construtor privado para o EF Core; verificar com os testes da tarefa 1.3
- [x] 1.3 Escrever testes de unidade de `Concurso`, um para cada cenário dos requisitos "Dados do concurso" e "Validação do concurso" da spec `concursos`; verificar com `dotnet test --solution src/Lotofacil.slnx`
- [x] 1.4 Criar a interface `IConcursoRepositorio` (`AdicionarAsync`, `ObterAsync`, `ListarAsync`, `ContarAsync`); verificar com `dotnet build` sem avisos

## 2. Aplicação

- [x] 2.1 Criar `PaginaConcursos` e `ConsultaConcursosServico` (`ListarPaginaAsync` com validação de página ≥ 0 e tamanho de 1 a 100, `ObterAsync`); verificar com `dotnet build` sem avisos
- [x] 2.2 Escrever testes de unidade do serviço com repositório NSubstitute (conversão de página em pular/pegar, total, argumentos inválidos, busca por número); verificar com `dotnet test`

## 3. Infraestrutura e banco

- [x] 3.1 Adicionar `Microsoft.EntityFrameworkCore.Sqlite` e `Microsoft.EntityFrameworkCore.Design` (`PrivateAssets="all"`) na versão estável 10.0.x mais recente ao `Directory.Packages.props` e à Infraestrutura; criar `dotnet-tools.json` na raiz com `dotnet-ef` na mesma versão; verificar com `dotnet restore` sem avisos e `dotnet tool run dotnet-ef --version`
- [x] 3.2 Criar `LotofacilContexto` e o mapeamento de `Concurso` em `IEntityTypeConfiguration` (tabela `Concurso`, dezenas e ordem como coleção primitiva, rateio `OwnsMany` na tabela `ConcursoRateio` com chave composta); verificar com `dotnet build` sem avisos
- [x] 3.3 Criar `LotofacilContextoFabricaProjeto` (`IDesignTimeDbContextFactory`), marcar a pasta `Migracoes` como código gerado no `.editorconfig` e gerar a migration `CriacaoConcursos`; verificar que o build continua sem avisos e revisar o SQL com `dotnet ef migrations script`
- [x] 3.4 Criar `ConcursoRepositorio` com `IDbContextFactory` (contexto por operação, leituras `AsNoTracking`, gravação do lote numa transação); verificar com os testes da tarefa 3.6
- [x] 3.5 Criar `ConfiguracaoInfraestrutura` com `AddInfraestrutura(caminhoBanco)` e `InicializarBancoDadosAsync()`; verificar com `dotnet build` sem avisos
- [x] 3.6 Escrever testes de integração com SQLite em memória: gravar e recuperar com centavos, concurso inexistente, lote com número repetido não grava nada, listagem decrescente com pular/pegar e contagem, e `HasPendingModelChanges()` falso; verificar com `dotnet test`
- [x] 3.7 Escrever teste de integração de `InicializarBancoDadosAsync` em pasta temporária: o arquivo é criado vazio na primeira execução e os concursos continuam lá na segunda; verificar com `dotnet test`

## 4. Web

- [x] 4.0 Permitir Web → Dominio: atualizar a tabela de dependências do `CLAUDE.md`, adicionar a referência ao projeto `Lotofacil.Web` e incluir `Lotofacil.Dominio` entre as permitidas da Web no teste de arquitetura; verificar com `dotnet test`
- [x] 4.1 Ligar no `Program.cs`: `BancoDados:Arquivo` em `appsettings.json` (padrão `lotofacil.db`, relativo ao `ContentRootPath`), `AddInfraestrutura`, registro do `ConsultaConcursosServico`, `UseRequestLocalization` com pt-BR e `InicializarBancoDadosAsync` antes do `app.Run()`; incluir `*.db`, `*.db-shm` e `*.db-wal` no `.gitignore`; verificar que `dotnet run` cria `lotofacil.db` e que `git status` não o lista
- [x] 4.2 Criar o componente `Dezenas` (bolinhas com dois dígitos) e o componente `DetalheConcurso` (rateio de 15 a 11, "—" sem ganhador, arrecadação ou "Não informada", ordem do sorteio quando houver); verificar com os testes da tarefa 4.5
- [x] 4.3 Criar a página `Resultados` (`/resultados`) com `MudTable` e `ServerData`, paginador 10/25/50 em português, colunas da spec, detalhe que abre e fecha no clique, busca por número com mensagem de não encontrado e mensagem de base vazia; verificar com os testes da tarefa 4.5
- [x] 4.4 Adicionar o item "Resultados" no menu lateral, abaixo de "Painel"; verificar com os testes da tarefa 4.5
- [x] 4.5 Escrever testes bUnit (cultura pt-BR, serviço real sobre repositório NSubstitute) para cada cenário da spec `resultados`; verificar com `dotnet test`

## 5. Verificação final

- [x] 5.1 Rodar `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` com todos os testes passando
- [x] 5.2 Abrir a aplicação no navegador: a página Resultados mostra a mensagem de base vazia; inserir manualmente dois concursos no `lotofacil.db` (SQL de teste, depois apagado) e conferir a lista, o detalhe e a busca
