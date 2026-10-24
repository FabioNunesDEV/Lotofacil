## Context

A motivação está em `proposal.md` e o comportamento, em `specs/configuracoes/spec.md` e `specs/jogos/spec.md`.
Estado atual relevante:

- O preço vem de `Lotofacil:PrecoApostaMinima` no `appsettings.json`. O `Program.cs` monta com ele o singleton
  `OpcoesApostas` (Web), injetado em `CadastroJogo`, `GerarJogos` e `MeusJogos`, que leem
  `Opcoes.PrecoApostaMinima` de forma síncrona: em propriedades calculadas (`Custo`, `CustoGeracao`, `CustoLista`,
  `ValorGrupo`), no markup do "Investido" e na exportação.
- Os serviços da Aplicação e os repositórios são singletons. Os repositórios abrem um `LotofacilContexto` por
  operação pela `IDbContextFactory`. A base é criada e migrada por `InicializarBancoDadosAsync` no `Program.cs`.
- Decimais são gravados como TEXT no SQLite (como os valores de `Concurso`), o que preserva os centavos.
- Não há nenhuma tabela de configuração. Os mapeamentos do EF seguem o nome `<Entidade>Configuracao`.

## Goals / Non-Goals

**Goals:**
- Preço editável, gravado na base, valendo na hora em todas as telas, sem reiniciar.
- Mudança mínima nos componentes que já usam o preço.
- A regra de validação do preço no domínio, testável.

**Non-Goals:**
- Vigência ou histórico de preços, prêmios fixos, URL e pausa da Caixa, arquivo do banco.

## Decisions

- **Domínio — `ConfiguracaoApostas`** (`Lotofacil.Dominio.Configuracoes`): entidade com `Id` e
  `PrecoApostaMinima`, sempre uma única linha (`IdUnico = 1`). Constantes `PrecoMinimo = 0,01` e
  `PrecoMaximo = 100,00`. A estática `PrecoValido(decimal)` diz se o preço está na faixa e tem no máximo dois
  decimais. O construtor lança `ArgumentOutOfRangeException` para preço inválido, como `Jogo` e
  `ParametrosGeracao`.
  - Nome: "Configuracao" sozinho geraria o mapeamento `ConfiguracaoConfiguracao`. "Apostas" diz o que a linha guarda e
    deixa espaço para outros valores das apostas no futuro.
  - Uma linha com colunas tipadas, e não uma tabela chave/valor: só há um valor, e a coluna decimal dispensa
    conversões de texto.
- **`IConfiguracaoApostasRepositorio`** (domínio): `ObterAsync()` devolve a linha ou `null` (nada gravado) e
  `SalvarAsync(configuracao)` insere ou atualiza a linha única.
- **Infraestrutura:** `ConfiguracaoApostasConfiguracao` mapeia para a tabela `ConfiguracaoApostas`, com a chave sem
  geração automática (`ValueGeneratedNever`). `ConfiguracaoApostasRepositorio` é registrado em `AddInfraestrutura`.
  A migration `CriacaoConfiguracaoApostas` só cria a tabela, sem linha inicial: a ausência de linha significa "use o
  padrão da configuração".
- **Aplicação — `ConfiguracoesServico`** (`Lotofacil.Aplicacao.Configuracoes`), singleton, recebe o repositório e o
  preço padrão (do `appsettings.json`):
  - `PrecoApostaMinima` (síncrona): preço gravado ou o padrão. O serviço guarda a `ConfiguracaoApostas` gravada num
    campo, cuja troca de referência é atômica entre circuitos.
  - `CarregarAsync()`: lê a linha gravada. O `Program.cs` chama depois de `InicializarBancoDadosAsync`.
  - `SalvarPrecoApostaMinimaAsync(preco)`: devolve `false` sem gravar quando o preço é inválido. Quando é válido,
    grava, atualiza o campo e devolve `true`.
  - Alternativa descartada: cada página lê o preço da base em `OnInitializedAsync`. `CadastroJogo` inicializa de forma
    síncrona e mostra o custo na primeira renderização. `MeusJogos` pode abrir o cadastro em `OnAfterRenderAsync` antes
    de a carga terminar. Com a leitura assíncrona, o custo apareceria zerado por um instante, e o preço teria de ser
    passado entre componentes. Com o valor guardado no serviço, as telas leem como hoje, e só o salvar muda o valor.
- **Web:**
  - `OpcoesApostas` é removido. `CadastroJogo`, `GerarJogos` e `MeusJogos` injetam `ConfiguracoesServico` e leem
    `PrecoApostaMinima` nos mesmos pontos de hoje. O `Program.cs` registra o serviço com
    `Lotofacil:PrecoApostaMinima` (padrão 3,50) e chama `CarregarAsync`.
  - Página `Configuracoes.razor(.cs)` em `/configuracoes`. Cabeçalho com título, subtítulo e "Salvar", como no
    protótipo. Cartão "Preço da aposta" com `MudNumericField<decimal?>` ("Aposta mínima (15 dezenas)", prefixo
    "R$", dois decimais, cultura pt-BR) e `Validation` por `ConfiguracaoApostas.PrecoValido`, com a mensagem da spec.
    A tabela é uma `MudSimpleTable` de 15 a 20 dezenas: apostas por `Jogo.CalcularQuantidadeApostas`, número com
    separador de milhar, valor por `FormatacaoJogo.Reais`, ou "—" com o campo inválido.
  - "Salvar" valida o campo (`ValidateAsync`). Se for inválido, mostra o erro no campo e não chama o serviço. Se for
    válido, chama `SalvarPrecoApostaMinimaAsync` e exibe "Configurações salvas.".
  - O aviso vem só da `Validation` do campo. Passar `Error`/`ErrorText` pela página não funciona no navegador: a
    validação interna do campo, que roda a cada mudança de valor, sobrescreve o erro, e ele some.
  - As células numéricas da tabela não quebram linha (`white-space: nowrap`), para "R$ 62.016,00" caber inteiro em
    390 px.
  - Menu: `MudNavLink` "Configurações" (`Icons.Material.Filled.Settings`) depois de "Meus jogos".
- **Testes:** domínio (`PrecoValido` e construtor nos limites 0,01, 100,00, 0, 100,01 e 3,555), Aplicação com
  repositório substituto (padrão sem linha, carregar, salvar válido e inválido), integração do repositório em SQLite
  em memória (obter vazio, inserir, atualizar sem duplicar), bUnit da página (valor ao abrir, tabela de R$ 3,50,
  tabela acompanhando a digitação, "—", recusa, salvar) e testes existentes de `MeusJogos`/`GerarJogos` registrando
  `ConfiguracoesServico` no lugar de `OpcoesApostas`, mais um cenário de "Investido" com R$ 4,00.

## Risks / Trade-offs

- [Preço guardado no serviço diverge da base se ela for alterada por fora] → a base só é escrita pela aplicação.
  Uma edição manual vale depois de reiniciar.
- [Mudar o preço altera o custo exibido de jogos antigos] → decisão do usuário (preço único). A spec e a página
  deixam isso claro.
- [Ferramenta `dotnet-ef` global antiga (6.0.10)] → gerar a migration com uma versão compatível com o EF Core do
  projeto. Se for preciso atualizar a ferramenta, avisar antes.

## Migration Plan

Migration `CriacaoConfiguracaoApostas` aplicada na inicialização, como as demais. Bases existentes ficam sem linha e
seguem com o preço do `appsettings.json` até o primeiro "Salvar". Rollback: remover a migration. Nenhum dado
existente é alterado.
