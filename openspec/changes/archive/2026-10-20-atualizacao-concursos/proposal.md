## Why

A base já recebe o histórico completo pelo XLSX, mas fica parada no dia da importação: a Lotofácil tem sorteio de
segunda a sábado, e reimportar o arquivo inteiro a cada concurso é lento e desnecessário. A API JSON da Caixa
devolve um concurso por requisição e traz a **ordem do sorteio**, que o XLSX não tem. Esta fatia mantém a base em
dia com poucas requisições e é pré-requisito para a conferência de jogos de concursos futuros.

## What Changes

- Buscar na API JSON da Caixa, **somente quando o usuário pedir**, os concursos posteriores ao último gravado: consultar
  o número do último concurso divulgado e buscar, um a um e em ordem, do último gravado + 1 até ele.
- Respeitar uma pausa mínima configurável entre requisições à Caixa (padrão 500 ms).
- Converter cada resposta em concurso com as regras já existentes, agora **com a ordem do sorteio**. Concurso já
  sorteado mas ainda sem rateio divulgado não é gravado; a atualização para nele e orienta tentar mais tarde.
- Gravar cada concurso assim que ele é obtido. Se uma requisição falhar no meio, os concursos já gravados ficam
  (a sequência continua sem lacunas) e a mensagem informa a falha e até onde a base foi atualizada.
- Com a base vazia, não chamar a API: orientar o uso de "Importar histórico" antes.
- Na página **Resultados**, adicionar o botão "Atualizar resultados" ao lado de "Importar histórico", com
  andamento (concurso sendo buscado), mensagem de resultado ou de falha e recarga da lista. Enquanto uma das duas
  operações estiver em andamento, os dois botões ficam desabilitados.
- Concentrar o acesso HTTP à Caixa num único cliente (pausa, tempo limite e tradução de falhas), usado também pelo
  download do histórico, sem mudar as mensagens da importação.
- Registrar no log de erros as falhas da atualização.
- Testes de unidade (serviço e página) e de integração (leitura do JSON com amostra real, cliente HTTP falso,
  atualização de ponta a ponta com SQLite em memória).

## Capabilities

### New Capabilities
- `atualizacao-concursos`: atualização incremental da base pela API JSON da Caixa, do último concurso gravado até o
  último divulgado, com ordem do sorteio, pausa entre requisições e gravação concurso a concurso.

### Modified Capabilities
- `resultados`: a página ganha o botão "Atualizar resultados", com andamento e mensagens, e as operações de
  importação e atualização passam a bloquear uma à outra enquanto estão em andamento.

## Impact

- `Lotofacil.Dominio`: interface da fonte de concursos individuais; `IConcursoRepositorio` ganha a consulta do
  último número gravado.
- `Lotofacil.Aplicacao`: serviço de atualização e o resultado da atualização (com progresso).
- `Lotofacil.Infraestrutura`: cliente HTTP da Caixa compartilhado, fonte JSON de concursos e leitura do JSON com
  `Newtonsoft.Json` (pacote já autorizado, entra agora), nova opção de pausa em `FonteResultados`.
- `Lotofacil.Web`: botão, andamento e mensagens na página Resultados; `PausaEntreRequisicoesMs` no
  `appsettings.json`.
- Fora do escopo: preencher a ordem do sorteio dos concursos importados do XLSX, buscar lacunas no meio da
  sequência, atualização automática/agendada, resumo da base no Painel e na barra superior.
