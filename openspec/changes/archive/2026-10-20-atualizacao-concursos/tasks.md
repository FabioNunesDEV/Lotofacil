## 1. Domínio e repositório

- [x] 1.1 Criar `IFonteConcursos` em `Lotofacil.Dominio/Concursos` (`ObterNumeroUltimoConcursoAsync`,
  `ObterConcursoAsync`); verificar com `dotnet build src/Lotofacil.slnx` sem avisos
- [x] 1.2 Adicionar `ObterUltimoNumeroAsync` a `IConcursoRepositorio` e implementá-lo com `MAX(Numero)`; verificar
  com testes de integração em `ConcursoRepositorioTestes` (base vazia → `null`; base com lacuna → maior número)

## 2. Serviço de atualização

- [x] 2.1 Criar `AndamentoAtualizacao`, `ResultadoAtualizacao` (com `BaseVazia`, `UltimoNumeroNaBase`,
  `PrimeiroGravado`) e `AtualizacaoConcursosServico`; verificar com testes de unidade (NSubstitute): base vazia sem
  chamar a fonte, já atualizada, três novos buscados em ordem e gravados um a um com o andamento 1 de 3 a 3 de 3,
  falha no meio preservando os gravados e devolvendo a falha, exceção do repositório propagada

## 3. Cliente HTTP da Caixa

- [x] 3.1 Adicionar `PausaEntreRequisicoesMs` a `OpcoesFonteResultados`, ao `appsettings.json` (500) e ao
  `Program.cs` (padrão 500), ajustando os usos existentes; verificar com o build sem avisos
- [x] 3.2 Alterar `RespostaFalsaHandler` para responder a partir da requisição e guardar os endereços pedidos,
  ajustando os testes atuais; verificar com `dotnet test --solution src/Lotofacil.slnx`
- [x] 3.3 Criar `ClienteCaixa` (URL, pausa serializada com `SemaphoreSlim` + `Stopwatch`, tradução de falhas com a
  operação) e passar `HistoricoCaixaFonte` a usá-lo; verificar com testes de integração da pausa (duas requisições
  com pausa de 200 ms começam com pelo menos 200 ms de diferença; pausa 0 sem espera) e das mensagens com a
  operação, e com os testes atuais de `HistoricoCaixaFonte` passando sem mudar os textos esperados

## 4. Leitura do JSON e fonte de concursos

- [x] 4.1 Adicionar `Newtonsoft.Json` (versão estável mais recente) ao `Directory.Packages.props` e à
  infraestrutura; extrair a remoção do sufixo `(Parameter '...')` para `MensagemValidacao.SemParametro`, usado por
  `LeitorPlanilhaHistorico`; verificar com o build sem avisos e os testes do leitor da planilha passando
- [x] 4.2 Baixar o JSON real do concurso 3808 para `Testes.Integracao/Arquivos/lotofacil-3808.json` (copiado para
  a saída) e criar `LeitorConcursoJson` (`LerNumero`, `LerConcurso`); verificar com testes de integração para cada
  cenário de "Leitura do concurso da API" e "Concurso sem rateio divulgado" da spec (valores exatos e ordem do
  sorteio da amostra, ordem ausente, descrição de faixa errada, número diferente, rateio vazio e todo zerado, JSON
  inválido, campo ausente, regra do domínio com mensagem sem sufixo)
- [x] 4.3 Criar `ConcursosCaixaFonte` e registrar em `AddInfraestrutura` um único `ClienteCaixa` usado pelas duas
  fontes; registrar `AtualizacaoConcursosServico` no `Program.cs`; verificar com testes de integração da fonte com
  handler falso (endereços `api/lotofacil/` e `api/lotofacil/{n}`, mensagens de 503 e tempo limite para "o último
  concurso" e "o concurso {n}") e com o teste de configuração de serviços resolvendo os dois serviços
- [x] 4.4 Teste de integração de ponta a ponta com handler falso e SQLite em memória: base até 3808 e API até 3811
  gravam 3809–3811 com ordem do sorteio; 503 no 3811 deixa 3809 e 3810 gravados e a execução seguinte começa no
  3811; base vazia não faz requisição; verificar com `dotnet test --solution src/Lotofacil.slnx`

## 5. Página Resultados

- [x] 5.1 Adicionar o botão "Atualizar resultados", o bloqueio mútuo com "Importar histórico", o texto de
  andamento, as mensagens (base vazia, já atualizada, 1 ou vários gravados, interrompida com e sem gravados, falha
  inesperada), o log da falha e a recarga da lista; verificar com testes bUnit para cada cenário de "Atualizar
  resultados pela página Resultados" da spec

## 6. Verificação final

- [x] 6.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 6.2 Verificação manual com uma base separada: importar o histórico, apagar os três concursos mais recentes
  dessa base, acionar "Atualizar resultados" e conferir o andamento, a mensagem, a lista e a ordem do sorteio no
  detalhe; acionar de novo e conferir "já está atualizada"
- [x] 6.3 Atualizar `docs/lotofacil-caixa-regras-e-dados.md` (seção 3.2) com o que foi conferido da API (faixa 1 a
  5, HTTP 500 para concurso inexistente, correspondência com as colunas do XLSX); verificar relendo a seção
