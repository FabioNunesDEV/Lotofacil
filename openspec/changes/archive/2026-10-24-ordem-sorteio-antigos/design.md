## Context

A motivação está em `proposal.md` e o comportamento, em `specs/ordem-sorteio/spec.md`. Estado atual relevante:

- `Concurso.OrdemSorteio` é opcional e só é definido no construtor, que já recusa uma ordem com dezenas diferentes
  das sorteadas. A coluna `OrdemSorteio` (JSON) já existe; os concursos do XLSX ficam com `null`.
- `IFonteConcursos.ObterConcursoAsync` (via `ConcursosCaixaFonte` e `LeitorConcursoJson.LerConcurso`) exige o
  concurso completo, inclusive rateio com as descrições de faixa atuais. Para concursos antigos isso é
  desnecessário e arriscado: basta a ordem.
- `ClienteCaixa` é único, aplica a pausa entre todas as requisições e converte falhas de rede, código HTTP e tempo
  limite em `FonteResultadosException` ("... para <operação>.", "... ao <operação>."). Um cancelamento pedido pelo
  chamador sai como `OperationCanceledException`.
- `AtualizacaoConcursosServico` mostra o padrão de andamento (`IProgress<AndamentoAtualizacao>`) e de resultado
  (`ResultadoAtualizacao`). A página Resultados mostra o andamento num texto (`andamento`) e desabilita os botões
  enquanto ele não é `null`; os textos ficam em `AvisosAtualizacao`.
- Na base real são 3.808 concursos sem ordem. Com a pausa de 500 ms, o preenchimento leva de 35 a 45 minutos.

## Goals / Non-Goals

**Goals:**
- Buscar só a ordem, sem depender do formato do rateio dos concursos antigos.
- Gravar concurso a concurso, retomando naturalmente pelos que continuam sem ordem.
- Interromper a qualquer momento, e não ficar minutos esperando tempos limite com a Caixa fora do ar.

**Non-Goals:**
- Corrigir ou completar outros dados dos concursos antigos; preencher a ordem durante a importação do XLSX;
  execução em segundo plano fora da página.

## Decisions

- **Domínio:**
  - `Concurso.DefinirOrdemSorteio(IEnumerable<int> ordem)` aplica a mesma regra do construtor (mesmas 15 dezenas;
    senão `ArgumentException`).
  - `IFonteConcursos.ObterOrdemSorteioAsync(numero)` devolve a ordem lida da API, ou `null` quando a resposta não
    traz a lista.
  - `IConcursoRepositorio` ganha:
    - `ListarNumerosSemOrdemAsync()`: números em ordem crescente;
    - `ContarSemOrdemAsync()`;
    - `GravarOrdemSorteioAsync(numero, ordem)`: carrega o concurso, chama `DefinirOrdemSorteio` e salva.
  - Alternativa descartada: reaproveitar `ObterConcursoAsync` e copiar a ordem. Ele falharia em qualquer concurso
    antigo com rateio fora do padrão atual, sem necessidade.
- **Infraestrutura:**
  - `LeitorConcursoJson.LerOrdemSorteio(conteudo, numeroPedido)` desserializa o mesmo `ConcursoCaixaJson`. Exige
    `numero` igual ao pedido (mesma mensagem de número trocado). Lê `dezenasSorteadasOrdemSorteio` com a validação
    de dezenas existente (`LerDezenas`). Lista vazia ou ausente devolve `null`. Formato inválido gera a mensagem
    "A resposta da Caixa para o concurso <n> não está no formato esperado.".
  - `ConcursosCaixaFonte` chama o cliente com a operação "buscar a ordem do concurso <n>". A pausa vale entre
    todas as requisições, porque o `ClienteCaixa` é o mesmo.
  - No repositório, `OrdemSorteio == null` funciona na consulta EF (coluna nula). A gravação usa o contexto
    rastreado: `Find` → `DefinirOrdemSorteio` → `SaveChanges`.
- **Aplicação — `OrdemSorteioServico`** (`Lotofacil.Aplicacao.Concursos`):
  - `ContarSemOrdemAsync()` repassa ao repositório.
  - `PreencherAsync(IProgress<AndamentoAtualizacao>?, CancellationToken)` lista os números sem ordem e percorre em
    ordem crescente. Antes de cada busca, reporta o andamento (reaproveita `AndamentoAtualizacao`). Em cada concurso:
    - ordem `null` → falha "A Caixa não informou a ordem do sorteio do concurso <n>.";
    - `ArgumentException` de `DefinirOrdemSorteio` → falha "A ordem do sorteio informada pela Caixa para o concurso
      <n> não corresponde às dezenas gravadas.";
    - `FonteResultadosException` → falha com a própria mensagem;
    - as falhas voltam numa lista no resultado, com a exceção de cada uma, para a página registrar no log (como a
      atualização faz com `Falha`);
    - 5 falhas seguidas (`FalhasSeguidasParaEncerrar = 5`) encerram a busca; um sucesso zera a contagem;
    - `OperationCanceledException` com o cancelamento pedido → para e devolve o resultado como interrompido, sem
      relançar;
    - qualquer outra exceção é lançada (a página mostra o erro inesperado).
  - `ResultadoPreenchimentoOrdem(Total, Preenchidos, Falhas, Interrompido, EncerradoPorFalhas)`, com `SemOrdem` =
    `Falhas.Count` e `Restantes` = `Total − Preenchidos`. `FalhaOrdemSorteio(Numero, Mensagem, Exception?)` descreve
    cada falha.
  - Alternativa descartada: parar na primeira falha, como a atualização. É decisão do usuário pular; o limite de 5
    seguidas evita esperar até 3.808 tempos limite de 60 s com a Caixa fora do ar.
- **Web:**
  - `AvisosOrdemSorteio` (estática, `Components/Concursos`) monta o andamento, o resumo, a interrupção e o
    encerramento por falhas, com "1 concurso"/"<n> concursos" e separador de milhar (`N0`). O aviso de
    encerramento é erro e persiste; o resumo com falhas é aviso; os demais são sucesso ou informação.
  - Em Resultados:
    - botão "Preencher ordem do sorteio" (`botao-ordem`, contornado, ícone `FormatListNumbered`);
    - `semOrdem` é carregado em `OnInitializedAsync` e depois de cada importação, atualização ou preenchimento;
      o botão fica desabilitado com `semOrdem == 0` ou com a página ocupada;
    - o andamento ganha o botão "Interromper" (`botao-interromper`) só durante o preenchimento, que cancela o
      `CancellationTokenSource` da operação;
    - a página passa a implementar `IDisposable` e cancela o preenchimento ao sair;
    - falhas do resultado são registradas no log uma a uma; com algum preenchido, a lista é recarregada
      (`BuscarAsync(null)`).
- **Testes:** domínio (`DefinirOrdemSorteio` aceita e recusa); leitor JSON (ordem lida, lista vazia → `null`,
  número trocado, formato inválido); repositório em SQLite em memória (lista/conta sem ordem, grava a ordem sem mudar
  o resto); serviço com fonte e repositório substitutos (ordem crescente, pula falhas e divergência, 5 seguidas
  encerram, sucesso zera a contagem, cancelamento interrompe sem relançar, andamento); `AvisosOrdemSorteio`; bUnit
  de Resultados (botão habilitado/desabilitado, andamento com "Interromper", interromper, resumo, recarga e bloqueio
  mútuo com os outros botões).

- **Limite de consultas (429), achado na verificação manual:**
  - Com a pausa de 500 ms, a Caixa respondeu 429 depois de cerca de 40 concursos e liberou minutos depois, sem o
    cabeçalho `Retry-After`. Contando o 429 como falha comum, cada rodada pararia em umas 40 requisições.
  - `FonteResultadosException` ganha `LimiteConsultas`, que o `ClienteCaixa` marca quando o código é 429. A
    mensagem não muda.
  - O serviço recebe as esperas no construtor (`IReadOnlyList<TimeSpan>? esperasLimite = null`, padrão 1, 2, 4 e
    8 min; os testes passam esperas zeradas). Num 429, reporta a espera e aguarda com `Task.Delay(espera,
    cancelamento)`, então "Interromper" corta a espera na hora. Depois repete o mesmo concurso. Esgotadas as
    esperas, encerra com a falha na lista.
  - O andamento passa a ser `AndamentoOrdemSorteio(Numero, Posicao, Total, Espera)`, com `Espera` nula fora das
    pausas. A página calcula o horário da nova tentativa (`DateTime.Now + Espera`) e formata com
    `AvisosOrdemSorteio.Andamento(andamento, agora)`.
  - `ResultadoPreenchimentoOrdem` troca os dois indicadores por `Desfecho` (`DesfechoPreenchimento`: `Concluido`,
    `Interrompido`, `EncerradoPorFalhas`, `EncerradoPorLimite`).
  - Alternativas descartadas pelo usuário: pausa maior só no preenchimento (não garante) e manter o 429 como falha
    comum (exigiria dezenas de cliques).

## Risks / Trade-offs

- [Limite da Caixa desconhecido] → as esperas crescem até 8 min. Se o bloqueio durar mais, a busca encerra com aviso,
  e o próximo clique continua de onde parou.

- [Preenchimento longo (35 a 45 min) preso à página aberta] → o usuário pode interromper e retomar; sair da página
  também interrompe sem perder o que já foi gravado.
- [Caixa limita requisições em sequência] → a pausa de 500 ms já vale para a atualização. Se a Caixa recusar, as
  falhas seguidas encerram a busca e o usuário tenta depois.
- [Ordem da Caixa errada para algum concurso antigo] → só é gravada se tiver exatamente as dezenas gravadas.

## Migration Plan

Sem migration: a coluna `OrdemSorteio` já existe. Nada muda nos dados até o usuário acionar o botão.
