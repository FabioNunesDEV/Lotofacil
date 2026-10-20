## 1. Domínio e repositório

- [x] 1.1 Criar em `Lotofacil.Dominio/Concursos` a interface `IFonteHistoricoConcursos` (`ObterHistoricoAsync`) e a
  `FonteResultadosException` (mensagem para o usuário + exceção interna); verificar com
  `dotnet build src/Lotofacil.slnx` sem avisos
- [x] 1.2 Adicionar `ListarNumerosAsync` a `IConcursoRepositorio` e implementá-lo em `ConcursoRepositorio`
  (consulta só da coluna `Numero`); verificar com teste de integração novo em `ConcursoRepositorioTestes` (base
  vazia e base com concursos)

## 2. Serviço de importação

- [x] 2.1 Criar em `Lotofacil.Aplicacao/Concursos` o `ResultadoImportacao` e o `ImportacaoHistoricoServico`
  (obtém o histórico, filtra os números já gravados e grava os novos num único lote); verificar com testes de
  unidade usando NSubstitute: base vazia, base parcial (grava só os novos), nada novo (não chama a gravação) e
  falha da fonte (não chama a gravação e a exceção sobe)

## 3. Leitura da planilha da Caixa

- [x] 3.1 Criar em `Lotofacil.Testes.Integracao` o auxiliar que monta XLSX no formato da Caixa (cabeçalho em
  `sharedStrings` com `r`, dados inline `t="n"`/`t="str"` sem `r`, prefixo `x:`) a partir de linhas de texto, e
  gravar em `Testes.Integracao/Arquivos/` a amostra recortada do XLSX real (cabeçalho + concursos 1, 2 e 3.808),
  copiada para a saída do build; verificar abrindo a amostra pelo auxiliar de teste
- [x] 3.2 Criar `Lotofacil.Infraestrutura/Caixa/LeitorPlanilhaHistorico` (`ZipArchive` + `XmlReader`, colunas por
  posição, cabeçalho conferido, conversão de inteiros, data e reais, regras do concurso, número repetido, linhas
  vazias ignoradas); verificar com testes de integração para cada cenário de "Leitura da planilha da Caixa" e
  "Arquivo inválido não grava nada" da spec, incluindo a amostra real (valores exatos do concurso 2) e a
  mensagem de dezena repetida sem o sufixo `(Parameter '...')`

## 4. Download da Caixa

- [x] 4.1 Criar `OpcoesFonteResultados` e `HistoricoCaixaFonte` (GET em `{UrlBase}/api/resultados/download?...`,
  conteúdo em `MemoryStream`, tradução de erro de rede, status sem sucesso e tempo limite para
  `FonteResultadosException`); verificar com testes de integração usando `HttpMessageHandler` falso: URL montada a
  partir da configuração (com e sem barra final), 503, erro de rede, tempo limite, cancelamento pelo chamador e
  conteúdo HTML
- [x] 4.2 Registrar em `AddInfraestrutura` o `HttpClient` único (`SocketsHttpHandler` com
  `PooledConnectionLifetime`, `Timeout` da configuração) e `IFonteHistoricoConcursos`; adicionar a seção
  `FonteResultados` ao `appsettings.json` e lê-la no `Program.cs`; registrar `ImportacaoHistoricoServico` como
  singleton; verificar com teste de integração que resolve o serviço pelo provedor montado como no `Program.cs`
- [x] 4.3 Teste de integração de ponta a ponta com `HttpMessageHandler` falso devolvendo XLSX gerado e SQLite em
  memória: base vazia grava tudo, segunda importação grava 0 sem alterar os concursos, arquivo com linha inválida
  não grava nada; verificar com `dotnet test --solution src/Lotofacil.slnx`

## 5. Página Resultados

- [x] 5.1 Adicionar à página o botão "Importar histórico", o indicador de andamento, as mensagens por
  `ISnackbar` (sucesso, nada novo, falha conhecida, falha inesperada), o log de falhas com `ILogger<Resultados>` e
  a recarga da lista a partir da primeira página; verificar com testes bUnit para cada cenário de "Importar
  histórico pela página Resultados" da spec (incluindo botão desabilitado enquanto a fonte não termina e
  quantidades com separador de milhar)
- [x] 5.2 Trocar a mensagem de base vazia para "Nenhum concurso gravado na base local. Use “Importar histórico”
  para carregar os concursos da Caixa."; verificar atualizando o teste bUnit existente da base vazia

## 6. Verificação final

- [x] 6.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 6.2 Verificação manual: rodar a aplicação com a base vazia, importar o histórico real da Caixa, conferir a
  mensagem, a lista a partir do concurso mais recente e o rateio de um concurso antigo; importar de novo e
  conferir a mensagem de "nada foi gravado"
- [x] 6.3 Atualizar `docs/lotofacil-caixa-regras-e-dados.md` (seção 3.1) com o formato conferido do XLSX
  (cabeçalho em `sharedStrings`, prefixo `x:`, títulos exatos das colunas); verificar relendo a seção
