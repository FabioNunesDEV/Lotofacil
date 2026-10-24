## 1. Domínio e infraestrutura

- [x] 1.1 `Concurso.DefinirOrdemSorteio` (mesmas 15 dezenas) e os métodos novos em `IConcursoRepositorio`
  (`ListarNumerosSemOrdemAsync`, `ContarSemOrdemAsync`, `GravarOrdemSorteioAsync`) e em `IFonteConcursos`
  (`ObterOrdemSorteioAsync`); verificar com testes de unidade: ordem válida gravada no concurso, ordem com dezena
  diferente recusada
- [x] 1.2 `LeitorConcursoJson.LerOrdemSorteio` e `ConcursosCaixaFonte.ObterOrdemSorteioAsync` (operação "buscar a
  ordem do concurso <n>"); verificar com testes: ordem do concurso 1 lida, lista vazia → `null`, número trocado,
  formato inválido, mensagem de falha de comunicação com a operação nova
- [x] 1.3 Implementar os métodos novos do `ConcursoRepositorio`; verificar com testes de integração em SQLite em
  memória: números sem ordem em ordem crescente, contagem, ordem gravada sem mudar dezenas, data, rateio e valores

## 2. Aplicação

- [x] 2.1 Criar `OrdemSorteioServico` (`ContarSemOrdemAsync`, `PreencherAsync`), `ResultadoPreenchimentoOrdem` e
  `FalhaOrdemSorteio` e registrar no `Program.cs`; verificar com testes de unidade com fonte e repositório
  substitutos: busca só os sem ordem em ordem crescente com andamento, pula comunicação/sem ordem/divergência e
  segue, 5 falhas seguidas encerram sem pedir o próximo, sucesso zera a contagem, cancelamento devolve interrompido
  sem relançar, erro inesperado é lançado mantendo o já gravado
- [x] 2.2 Limite de consultas: `FonteResultadosException.LimiteConsultas` marcado pelo `ClienteCaixa` no 429,
  esperas de 1, 2, 4 e 8 min no serviço (configuráveis no construtor), `AndamentoOrdemSorteio` com a espera e
  `DesfechoPreenchimento` no resultado; verificar com testes: 429 marcado no cliente, espera reportada e mesmo
  concurso repetido sem contar falha, limite persistente encerra com a falha, cancelamento durante a espera interrompe

## 3. Web

- [x] 3.1 Criar `AvisosOrdemSorteio` (andamento, resumo com e sem falhas, interrupção, encerramento por falhas,
  "Todos os concursos já têm a ordem do sorteio.", erro inesperado); verificar com testes de unidade com os textos
  dos cenários da spec (singular, plural e separador de milhar)
- [x] 3.2 Em Resultados: botão "Preencher ordem do sorteio", contagem dos sem ordem, andamento com "Interromper",
  cancelamento ao sair da página, log das falhas, recarga da lista e bloqueio mútuo com "Atualizar resultados" e
  "Importar histórico"; verificar com testes bUnit: botão habilitado só com concursos sem ordem, andamento e
  botões desabilitados durante o preenchimento, "Interromper" com a mensagem, resumo com e sem falhas, botão
  desabilitado depois de preencher tudo, testes atuais da página passando
- [x] 3.3 Andamento da espera ("A Caixa pediu uma pausa. Nova tentativa do concurso <n> às <HH:mm> …") e mensagem
  de encerramento por limite em `AvisosOrdemSorteio` e Resultados; verificar com testes de unidade dos textos e
  bUnit do andamento durante a espera

## 4. Verificação final

- [x] 4.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 4.2 Verificação manual com a API real numa cópia da base: preencher alguns concursos e interromper, conferir a
  ordem no detalhe (concurso 1: 18 20 25 23 10 11 24 14 06 02 13 09 05 16 03), retomar do ponto em que parou,
  simular a Caixa fora do ar (URL base inválida) para ver o encerramento após 5 falhas, simular o 429 com um
  servidor local de teste para ver a espera e a nova tentativa, layout em 390 px
- [x] 4.3 Atualizar `Docs/roteiro.md` (fatia 14 concluída, decisões registradas); verificar relendo o arquivo
