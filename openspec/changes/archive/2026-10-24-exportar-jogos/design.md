## Context

A motivação está em `proposal.md` e o comportamento, em `specs/jogos/spec.md`. Estado atual relevante:

- Meus jogos (`MeusJogos.razor(.cs)`) recebe de `JogosServico.ListarAsync` os `GrupoJogos`, cada um com os
  `JogoConferido` na ordem de cadastro (`Numero` = posição no concurso) e `QuantidadeApostas`. O custo exibido no
  grupo é `QuantidadeApostas × OpcoesApostas.PrecoApostaMinima` (`ValorGrupo`).
- `FormatacaoJogo` já tem `NumeroJogo`, `QuantidadeJogos`, `QuantidadeDezenas` e `Reais`.
- Os diálogos usam `IDialogService` do MudBlazor (`CadastroJogo`, `DialogoConferencia`).
- O projeto não tem JavaScript próprio nem chamadas por `IJSRuntime`.

## Goals / Non-Goals

**Goals:**
- Montar o texto numa classe estática testável, sem lógica no componente.
- Copiar e baixar sem pacote novo e sem arquivo `.js`.

**Non-Goals:**
- Outros formatos, exportação de vários concursos, conferência no texto.

## Decisions

- **`FormatacaoExportacao`** (estática, `Components/Jogos`): `Texto(grupo, precoApostaMinima)`,
  `NomeArquivo(numeroConcurso)` ("concurso-3813.txt") e `EnderecoDownload(texto)`. O texto é formatação de
  exibição de dados já calculados (dezenas, quantidade e custo do grupo), como as demais classes `Formatacao*` da Web.
  - Alternativa descartada: um serviço na Aplicação. Não há regra de negócio nova, só o arranjo do texto, e o custo
    já vem de `GrupoJogos.QuantidadeApostas`.
- As linhas do texto são separadas por `\r\n`, para o arquivo abrir certo no Bloco de Notas, e o texto não termina
  com quebra de linha.
- **Baixar:** um `MudButton` com `Href` = `data:text/plain;charset=utf-8,<texto codificado com
  Uri.EscapeDataString>` e o atributo `download="concurso-<n>.txt"`. O navegador baixa sem passar pelo servidor e
  sem script.
  - Alternativa descartada: endpoint de download. O projeto não usa controllers nem minimal API.
- **Copiar:** `IJSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", texto)`. O Blazor resolve o
  identificador a partir de `window`, e `localhost` é contexto seguro, então a API funciona. Uma `JSException` ou
  `InvalidOperationException` cai na mensagem de falha. O diálogo continua aberto nos dois casos.
  - Alternativa descartada: arquivo `.js` com uma função de cópia. Não é necessário para uma chamada só.
- **`DialogoExportacao`** (`Components/Jogos`): recebe `Texto` e `NomeArquivo`, mostra o texto num
  `MudTextField` multilinha somente leitura (fonte monoespaçada, sem quebrar as linhas e com rolagem horizontal em
  telas estreitas) e os três botões. Meus jogos monta os parâmetros com `FormatacaoExportacao` e abre o diálogo pelo
  botão "Exportar" do cabeçalho de cada grupo, com largura `MaxWidth.Small` e `FullWidth`, para caber uma linha de
  aposta de 16 dezenas.

## Risks / Trade-offs

- [Navegador recusa a cópia (permissão, página sem foco)] → mensagem de falha e o texto continua selecionável no
  diálogo.
- [Data URL longa] → no máximo algumas centenas de jogos por concurso, poucos KB, bem abaixo dos limites dos
  navegadores.
- [Testes bUnit não executam JavaScript] → o modo `Loose` do bUnit já aceita a chamada; o teste confere a chamada
  `navigator.clipboard.writeText` com o texto e simula a falha configurando o interop para lançar exceção.

## Migration Plan

Sem migration nem dado gravado.
