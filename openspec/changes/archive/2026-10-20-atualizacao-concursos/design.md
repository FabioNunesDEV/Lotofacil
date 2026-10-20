## Context

Motivação em `proposal.md`; comportamento em `specs/`. Estado atual relevante:

- `HistoricoCaixaFonte` (infraestrutura) faz o GET do XLSX e traduz falhas em `FonteResultadosException`, com
  mensagens fixas para "baixar o histórico". O `HttpClient` é criado dentro do registro dessa fonte.
- `OpcoesFonteResultados(UrlBase, TempoLimiteSegundos)` vem da seção `FonteResultados` do `appsettings.json`.
- `ImportacaoHistoricoServico` (aplicação) não captura exceções; a página Resultados captura, mostra a mensagem e
  registra no log. `Lotofacil.Aplicacao` não tem abstrações de log.
- `LeitorPlanilhaHistorico` já remove o sufixo `(Parameter '...')` das mensagens de validação do domínio.
- `IConcursoRepositorio.AdicionarAsync` grava um lote numa transação.

API JSON conferida em 2026-10-20:

- `GET /api/lotofacil/` devolve o último concurso (3808); `GET /api/lotofacil/{n}` devolve o concurso `n`.
- Concurso inexistente (3809, ainda não sorteado; 99999) responde **HTTP 500** com JSON de erro, não 404.
- `listaDezenas` e `dezenasSorteadasOrdemSorteio`: listas de textos `"01"`…`"25"`.
- `listaRateioPremio[]`: `faixa` 1…5 com `descricaoFaixa` `"15 acertos"`…`"11 acertos"`, `numeroDeGanhadores`,
  `valorPremio` (número JSON, ex.: `1456047.72`).
- Valores do concurso 3808 iguais aos do XLSX: `valorAcumuladoProximoConcurso` corresponde ao "Acumulado 15
  acertos" da planilha.

## Goals / Non-Goals

**Goals:**
- Um único ponto de acesso HTTP à Caixa (pausa, tempo limite, tradução de falhas) para as duas fontes.
- Progresso preservado: o que foi obtido fica gravado, sem lacunas.
- Mensagens da importação do histórico inalteradas.

**Non-Goals:**
- Paralelizar requisições (a pausa existe justamente para não sobrecarregar a API).
- Atualizar dados de concursos já gravados (ex.: completar ordem do sorteio dos importados pelo XLSX).
- Cancelamento pelo usuário no meio da atualização.

## Decisions

### Domínio
- `IFonteConcursos` (em `Concursos/`):
  - `ObterNumeroUltimoConcursoAsync(CancellationToken)` → `int`;
  - `ObterConcursoAsync(int numero, CancellationToken)` → `Concurso`.
  Todas as falhas (rede, formato, rateio não divulgado, regra do domínio) chegam como `FonteResultadosException`,
  com a mensagem da spec. O serviço não precisa distinguir os casos: em todos, para e mantém o que gravou.
- `IConcursoRepositorio.ObterUltimoNumeroAsync(CancellationToken)` → `int?` (`null` com a base vazia), com
  `MAX(Numero)` no banco. Alternativa descartada: `ListarNumerosAsync().Max()`, que traria ~3.800 números para usar
  um.

### Aplicação
`AtualizacaoConcursosServico.AtualizarAsync(IProgress<AndamentoAtualizacao>? andamento, CancellationToken)`:
1. `ObterUltimoNumeroAsync`; se `null`, devolve resultado de base vazia sem chamar a fonte;
2. `ObterNumeroUltimoConcursoAsync`; se não for maior, devolve "já atualizada";
3. para cada número do último gravado + 1 até o último divulgado: informa o andamento, obtém o concurso e grava com
   `AdicionarAsync([concurso])`;
4. uma `FonteResultadosException` em 2 ou 3 encerra o laço e vai para o resultado (`Falha`); outras exceções sobem.

`ResultadoAtualizacao(int? UltimoNumeroAntes, int ConcursosGravados, FonteResultadosException? Falha)`, com
propriedades derivadas `BaseVazia`, `UltimoNumeroNaBase` e `PrimeiroGravado`.
`AndamentoAtualizacao(int Numero, int Posicao, int Total)`.

Por que a falha da fonte vira resultado e não exceção: interrupção com concursos gravados é um desfecho normal, e a
página precisa da quantidade gravada para a mensagem. Falhas inesperadas (banco) continuam como exceção, tratadas
pela página como na importação. A página registra no log a `Falha` (que carrega a causa técnica como
`InnerException`), porque a aplicação não tem log.

Gravar concurso a concurso (uma transação cada) em vez de um lote no fim: são poucos concursos por vez e a
interrupção no meio preserva o progresso, que é o comportamento pedido.

### Infraestrutura

**`ClienteCaixa`** (novo, interno, singleton): recebe o `HttpClient` e as opções e expõe
`ObterAsync(string caminho, string operacao, CancellationToken)` → `byte[]`.
- Monta `{UrlBase sem barra final}/{caminho}`.
- Pausa: um `SemaphoreSlim(1)` serializa as requisições; antes de cada uma, espera o que faltar para completar
  `PausaEntreRequisicoesMs` desde o início da anterior (medido com `Stopwatch`). Com pausa 0, não espera.
- Traduz falhas com `operacao` na mensagem: `HttpRequestException` → "Não foi possível conectar à Caixa para
  {operacao}."; status sem sucesso → "A Caixa respondeu com o código {código} ao {operacao}.";
  `TaskCanceledException` sem cancelamento do chamador → "A Caixa não respondeu em {s} segundos ao {operacao}.".
- `HistoricoCaixaFonte` passa a usar `ObterAsync("api/resultados/download?modalidade=Lotof%C3%A1cil", "baixar o
  histórico")`, o que mantém exatamente as mensagens atuais; seus testes continuam valendo sem mudança de texto.

Alternativa descartada: cada fonte com seu próprio `HttpClient` e sua própria pausa — duas fontes poderiam
disparar requisições sem respeitar o intervalo entre si.

**`ConcursosCaixaFonte`** (`IFonteConcursos`): usa `ClienteCaixa` com os caminhos `api/lotofacil/` (operação
"buscar o último concurso") e `api/lotofacil/{n}` ("buscar o concurso {n}") e entrega o conteúdo ao leitor.

**`LeitorConcursoJson`** (estático, interno):
- `Newtonsoft.Json` com classes internas de transferência (`[JsonProperty]` com os nomes da API) e
  `FloatParseHandling.Decimal`, para os valores não passarem por `double`.
- `LerNumero(conteudo)` para a consulta do último concurso; `LerConcurso(conteudo, numeroPedido)` para os demais.
- Campo obrigatório ausente, tipo errado ou `JsonException` → "A resposta da Caixa para {o concurso n | o último
  concurso} não está no formato esperado.".
- Ordem de verificação: formato → número igual ao pedido → rateio divulgado → faixas (descrição conferida) →
  regras do domínio (`Concurso <n>: <motivo>`).
- Rateio não divulgado: lista vazia ou todas as faixas com zero ganhadores. A faixa de 11 acertos sempre tem
  ganhadores num concurso apurado, então "todas zero" indica dado provisório.
- Data: `DateOnly.ParseExact(dataApuracao, "dd/MM/yyyy")`; dezenas: `int.Parse` dos textos.
- A remoção do sufixo `(Parameter '...')` sai de `LeitorPlanilhaHistorico` para um auxiliar interno comum
  (`MensagemValidacao.SemParametro`), usado pelos dois leitores.

**Configuração**: `OpcoesFonteResultados` ganha `PausaEntreRequisicoesMs`; `appsettings.json` recebe
`"PausaEntreRequisicoesMs": 500`, lido no `Program.cs` com padrão 500. `AddInfraestrutura` registra um único
`ClienteCaixa` e as duas fontes como singletons que o recebem.

**Pacote**: `Newtonsoft.Json` (autorizado no `CLAUDE.md`) entra em `Directory.Packages.props` com a versão estável
mais recente e é referenciado só pela infraestrutura.

### Página Resultados
- Botão "Atualizar resultados" (ícone de atualização, variante preenchida, por ser a ação do dia a dia) antes de
  "Importar histórico".
- Estado: `importando` e `atualizando`; os dois botões usam `Disabled="this.importando || this.atualizando"`.
- Andamento: texto inicial "Consultando o último concurso da Caixa…"; um `Progress<AndamentoAtualizacao>` atualiza
  o texto e chama `InvokeAsync(StateHasChanged)`. Mesmo bloco visual da importação (`MudProgressLinear` + texto).
- Mensagens pelo `ISnackbar`, montadas na página: base vazia (`Warning`), já atualizada (`Info`), concluída
  (`Success`), interrompida (`Error`, exige fechar). Singular/plural tratado na página.
- Quando `ConcursosGravados > 0`, recarrega a lista pelo mesmo caminho usado após a importação.
- `AtualizacaoConcursosServico` registrado como singleton no `Program.cs`.

### Testes
- **Unidade**: serviço com NSubstitute (base vazia sem chamar a fonte, já atualizada, três novos em ordem com
  gravação individual e andamento, falha no meio preservando os gravados, exceção do repositório propagada); página
  com bUnit para cada cenário da spec de `resultados`.
- **Integração**:
  - amostra real do JSON do concurso 3808 em `Testes.Integracao/Arquivos/lotofacil-3808.json`; variações
    construídas nos testes alterando o JSON da amostra (`JObject`);
  - `LeitorConcursoJson`: valores exatos e ordem do sorteio da amostra, ordem ausente, descrição de faixa errada,
    número diferente, rateio vazio e todo zerado, JSON inválido, campo ausente, regra do domínio;
  - `ClienteCaixa`: pausa respeitada entre duas requisições (tempo medido) e pausa zero; mensagens com a operação;
  - `RespostaFalsaHandler` passa a receber a requisição, para responder conforme a URL e guardar a lista de
    endereços pedidos;
  - ponta a ponta com SQLite em memória: base com 3808, API com 3811 → grava 3809–3811 com ordem do sorteio;
    503 no 3811 → ficam 3809 e 3810, e a execução seguinte começa no 3811.

## Risks / Trade-offs

- [A API muda nomes ou tipos de campos] → Mensagem "não está no formato esperado" com o número do concurso; nada
  inválido é gravado. Ajuste concentrado em `LeitorConcursoJson`.
- [Rateio provisório que não seja lista vazia nem tudo zero] → Não observado. Se aparecer, o concurso seria gravado
  com rateio errado e a conferência usaria esse valor; mitigação futura: regra adicional no leitor.
- [Concurso inexistente responde 500, não 404] → Só são pedidos números até o último divulgado; se mesmo assim
  vier 500, a mensagem de código de erro aparece e o progresso fica salvo.
- [Pausa torna atualizações longas lentas] → Depois de muito tempo sem atualizar (ex.: 60 concursos), leva cerca de
  30 s com a pausa padrão; o andamento mostra a posição. Para cargas grandes, existe a importação do histórico.
- [Duas abas atualizando ao mesmo tempo] → O `ClienteCaixa` serializa as requisições; a gravação de um número já
  gravado falha no repositório (erro inesperado na segunda aba), sem corromper a base.
- [Componente descartado durante a atualização] → A atualização termina normalmente no servidor; a chamada de
  `StateHasChanged` via `InvokeAsync` em componente descartado é ignorada pelo Blazor.

## Migration Plan

Sem migration de banco. Nova chave `PausaEntreRequisicoesMs` com padrão no código e no `appsettings.json`. Para
desfazer, reverter o código; os concursos gravados pela atualização continuam válidos.
