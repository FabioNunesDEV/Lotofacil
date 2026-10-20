## Context

Motivação em `proposal.md`. Estado atual:

- `Concurso` (domínio) já valida todas as regras e aceita ordem do sorteio nula; `IConcursoRepositorio.AdicionarAsync`
  grava um lote numa transação e falha se algum número já existir.
- O repositório é singleton e cria um `DbContext` por operação (`IDbContextFactory`).
- O log do Serilog grava apenas erros, em arquivo, configurado no `Program.cs` da Web.
- `Lotofacil.Aplicacao` não referencia nenhum pacote; `Lotofacil.Infraestrutura` é uma biblioteca comum (sem o
  framework do ASP.NET Core), com EF Core Sqlite.

Arquivo real da Caixa, baixado e conferido em 2026-10-20 (385 KB, concursos 1 a 3.808, todos válidos pelas regras
do domínio):

- Um único `xl/worksheets/sheet1.xml`, com prefixo de namespace `x:` (`<x:row>`, `<x:c>`, `<x:v>`).
- Cabeçalho (linha 1) com células `t="s"` apontando para `xl/sharedStrings.xml` e com atributo `r`.
- Linhas de dados **sem** atributo `r` nas linhas e nas células, sempre com as 33 células, valores inline:
  `t="n"` para números (dezenas com zero à esquerda, `<x:v>05</x:v>`) e `t="str"` para data e valores em reais.
- `<dimension ref="A1:AG1">` está errado (não reflete as linhas), por isso não é usado.
- Valores em reais sempre no formato `R$1.234.567,89`, sem espaço.

## Goals / Non-Goals

**Goals:**
- Leitor de XLSX próprio e pequeno, preso ao layout da Caixa, sem pacote novo.
- Toda a validação acontece antes de gravar; a gravação é uma única transação.
- Mensagens de falha claras para o usuário, com o detalhe técnico no log.

**Non-Goals:**
- Leitor de XLSX genérico (estilos, fórmulas, várias planilhas, datas seriais do Excel).
- Cancelamento da importação pelo usuário ou barra de progresso por linha (o processo leva poucos segundos).
- Bloqueio de importações simultâneas em abas diferentes (ver Riscos).
- Atualizar concursos já gravados com dados do arquivo.

## Decisions

### Separação entre download e leitura
- **Domínio** (`Concursos/`): `IFonteHistoricoConcursos` com `ObterHistoricoAsync(CancellationToken)`, que devolve
  `IReadOnlyList<Concurso>`; e `FonteResultadosException`, cuja mensagem é a que o usuário vê. A mesma exceção
  servirá à API JSON na próxima fatia.
- **Infraestrutura** (`Caixa/`):
  - `HistoricoCaixaFonte` implementa a interface: faz o GET, converte as falhas de rede em
    `FonteResultadosException` (com a exceção original como `InnerException`) e entrega o conteúdo ao leitor.
  - `LeitorPlanilhaHistorico` transforma um `Stream` de XLSX em `IReadOnlyList<Concurso>`; não conhece HTTP.
  - `OpcoesFonteResultados` (registro simples com `UrlBase` e `TempoLimiteSegundos`).
- Alternativa descartada: uma única classe que baixa e lê. Separar permite testar o leitor com arquivos locais e o
  download com um `HttpMessageHandler` falso, cada um com poucos casos.

### Leitura do XLSX com `ZipArchive` + `XmlReader`
Decisão já tomada no roteiro: sem ExcelDataReader. O leitor:
1. Abre o pacote com `ZipArchive` (`InvalidDataException` → "não é uma planilha XLSX válida"; também quando falta
   `xl/worksheets/sheet1.xml`).
2. Carrega `xl/sharedStrings.xml`, se existir, numa lista (só o cabeçalho usa).
3. Percorre `sheet1.xml` com `XmlReader`, comparando pelo nome local (`row`, `c`, `v`, `t`) para não depender do
   prefixo `x:`. Cada linha vira uma lista de textos por **posição**: o leitor ignora o atributo `r`, que o
   cabeçalho tem e os dados não. `t="s"` busca o texto em `sharedStrings`; `t="inlineStr"` lê `<is><t>`; os demais
   usam o texto de `<v>`.
4. Confere o cabeçalho: ao menos 33 colunas e os títulos esperados nas posições usadas (comparação exata, após
   remover espaços nas pontas). As posições ignoradas (Cidade / UF e Observação) não são conferidas.
5. Converte cada linha de dados, ignorando linhas sem nenhum texto preenchido.

As posições das colunas ficam em constantes nomeadas no leitor, junto com os títulos esperados.

Alternativa considerada: localizar as colunas pelos títulos. Rejeitada: o arquivo sempre tem a ordem fixa, e a
conferência dos títulos já detecta mudança de layout com uma mensagem clara.

### Conversão de valores
- Inteiros: `int.Parse` com `NumberStyles.None` e cultura invariável (aceita `05`).
- Data: `DateOnly.ParseExact(texto, "dd/MM/yyyy", CultureInfo.InvariantCulture)`.
- Reais: remove o prefixo `R$` e espaços e usa `decimal.Parse` com `NumberStyles.AllowThousands |
  NumberStyles.AllowDecimalPoint` e a cultura `pt-BR` (separador `.` de milhar e `,` decimal). Preserva os
  centavos exatos, sem passar por `double`.
- Falha de conversão: `FonteResultadosException` com
  `Linha <n> (concurso <número>): a coluna "<título>" tem o valor "<texto>", que não é <tipo esperado>.`
- Regras do concurso: o leitor cria o `Concurso` e captura `ArgumentException`, anexando a mensagem do domínio ao
  prefixo `Linha <n> (concurso <número>):`. Quando `ParamName` está preenchido, o .NET acrescenta
  ` (Parameter '<nome>')` ao fim de `Message`; o leitor remove esse sufixo para a mensagem ficar limpa.
- Números repetidos na planilha: o leitor guarda os números já lidos num `HashSet<int>` e falha na primeira
  repetição.

### Download
- `HttpClient` único, criado em `AddInfraestrutura` com `SocketsHttpHandler { PooledConnectionLifetime = 5 min }`
  e `Timeout` vindo da configuração, entregue a `HistoricoCaixaFonte`. Evita o pacote `Microsoft.Extensions.Http`
  (não autorizado) e é suficiente para uma aplicação local com pouquíssimas requisições. A fatia da API JSON
  reutilizará o mesmo cliente.
- URL: `UrlBase` sem barra final + `/api/resultados/download?modalidade=Lotof%C3%A1cil`.
- O conteúdo é copiado para um `MemoryStream` (cerca de 400 KB) antes da leitura: `ZipArchive` precisa de stream
  com busca.
- Tradução de falhas:
  - `HttpRequestException` → "Não foi possível conectar à Caixa para baixar o histórico.";
  - status sem sucesso → "A Caixa respondeu com o código <código> ao baixar o histórico.";
  - `TaskCanceledException` sem cancelamento pedido pelo chamador → mensagem de tempo limite com os segundos
    configurados. Cancelamento pedido pelo chamador continua como `OperationCanceledException`.
- Sem `User-Agent` próprio: o arquivo respondeu 200 desta máquina sem ele (verificado em 2026-10-18 e 2026-10-20).

### Configuração
Nova seção no `appsettings.json`, lida no `Program.cs` e passada a `AddInfraestrutura`, no mesmo estilo de
`BancoDados:Arquivo`:

```json
"FonteResultados": {
  "UrlBase": "https://servicebus3.caixa.gov.br/portaldeloterias",
  "TempoLimiteSegundos": 60
}
```

### Serviço de importação (Aplicação)
`ImportacaoHistoricoServico.ImportarAsync(CancellationToken)`:
1. `fonte.ObterHistoricoAsync` (já validado; falhas sobem como `FonteResultadosException`);
2. `repositorio.ListarNumerosAsync` → conjunto de números já gravados;
3. filtra os novos e, se houver, chama `repositorio.AdicionarAsync` com todos de uma vez;
4. devolve `ResultadoImportacao(int ConcursosNoArquivo, int ConcursosGravados)`.

Nova operação no repositório: `ListarNumerosAsync` devolvendo `IReadOnlySet<int>` (cerca de 3.800 inteiros; uma
consulta só da coluna `Numero`). Alternativa descartada: "último número gravado", que falharia com lacunas na base.

O serviço não captura exceções nem registra log: `Lotofacil.Aplicacao` não tem as abstrações de log e a falha
precisa chegar à tela de qualquer forma.

### Página Resultados
- Botão "Importar histórico" (`MudButton`, ícone de download) no cabeçalho, ao lado da busca.
- Durante a importação: botão desabilitado e `MudProgressLinear` indeterminado com o texto "Importando o histórico
  da Caixa…".
- Mensagens com `ISnackbar` do MudBlazor: sucesso (`Severity.Success`), nada novo (`Severity.Info`), falha
  (`Severity.Error`). Os textos são montados na página (apresentação), com quantidades formatadas `N0` na cultura
  pt-BR.
- A página captura `FonteResultadosException` (mostra a mensagem) e `Exception` (mostra o texto de erro
  inesperado) e registra ambas com `ILogger<Resultados>` em nível `Error`, que vai para o arquivo do Serilog.
  `OperationCanceledException` não é tratada como falha.
- Após gravar concursos novos: limpa a busca e os detalhes abertos e volta a tabela para a primeira página (mesmo
  caminho da limpeza da busca).
- `ImportacaoHistoricoServico` registrado como singleton, como `ConsultaConcursosServico`.

### Testes
- **Unidade**: `ImportacaoHistoricoServico` com fonte e repositório substitutos (NSubstitute): base vazia, base
  parcial, nada novo, falha da fonte sem gravação. Página Resultados com bUnit: botão, mensagens, botão
  desabilitado durante a importação (fonte que só termina quando o teste libera), recarga da lista e nova
  mensagem de base vazia.
- **Integração**: `LeitorPlanilhaHistorico` com arquivos XLSX montados nos testes por um auxiliar que gera o
  pacote mínimo no mesmo formato da Caixa (cabeçalho em `sharedStrings`, dados inline sem `r`, prefixo `x:`), o
  que permite variar cabeçalho, valores e linhas; e com um arquivo de amostra recortado do XLSX real
  (cabeçalho + concursos 1, 2 e 3.808), guardado em `Testes.Integracao/Arquivos/`. `HistoricoCaixaFonte` com
  `HttpMessageHandler` falso (URL montada, 503, erro de rede, tempo limite, conteúdo HTML). Importação de ponta a
  ponta com SQLite em memória: grava, reimporta sem duplicar e não grava nada com linha inválida.
  `ListarNumerosAsync` no teste do repositório.

## Risks / Trade-offs

- [A Caixa muda o layout ou o formato dos valores] → A conferência do cabeçalho e as mensagens por linha param a
  importação sem gravar nada e dizem o que mudou; o ajuste fica concentrado no leitor.
- [A Caixa muda o host novamente] → URL base na configuração.
- [O formato do sufixo `(Parameter '<nome>')` mudar numa versão do .NET] → Se o sufixo não for encontrado, a
  mensagem segue inteira (só fica menos limpa). Coberto por teste da mensagem de dezena repetida.
- [Duas abas importam ao mesmo tempo] → A segunda gravação falha na verificação de duplicados do repositório ou na
  chave primária, numa transação; nada fica pela metade e a falha aparece como erro inesperado. Aceitável para uso
  local de um usuário.
- [Importação de 3.808 concursos numa transação] → Poucos segundos no SQLite; conferido na verificação manual.
- [Arquivo grande ou malicioso] → A fonte é fixa (Caixa, HTTPS) e o arquivo tem ~400 KB; não há limite de tamanho
  além do tempo limite.

## Migration Plan

Sem migration de banco: a estrutura das tabelas não muda. A seção `FonteResultados` tem valores padrão no
`appsettings.json`. Para desfazer, basta reverter o código; os concursos importados continuam válidos na base.
