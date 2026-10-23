## 1. Aplicação

- [x] 1.1 Adicionar `GrupoJogos.DentroDoPrazo(DateOnly hoje)`; verificar com testes de unidade (sorteio em
  19/10/2026: dentro do prazo em 17/01/2027, fora em 18/01/2027, grupo aguardando sempre fora)
- [x] 1.2 Criar `ResumoPainel` e `PainelServico.ObterResumoAsync(hoje)` sobre `IConcursoRepositorio`,
  `EstatisticasServico` e `JogosServico`; verificar com testes de unidade: base vazia devolve `null`; total de
  concursos e último concurso; grupo do último concurso (com e sem jogos); jogos aguardando em um e em vários
  concursos, com os números em ordem; prêmios a receber somando só os grupos dentro do prazo; rankings iguais aos
  de `EstatisticasServico.CalcularAsync(null)`
- [x] 1.3 Registrar `PainelServico` em `Program.cs`; verificar com o build e a página abrindo nos testes bUnit

## 2. Web: textos compartilhados

- [x] 2.1 Extrair os textos do andamento e do desfecho da atualização da página Resultados para `AvisosAtualizacao`
  e fazer a página usá-los; verificar com testes de unidade de `AvisosAtualizacao` (cada desfecho e o andamento) e
  com os testes bUnit atuais da página Resultados passando sem mudança de expectativa
- [x] 2.2 Criar `FormatacaoPainel` e `FormatacaoConcurso.DataComDiaSemana`; verificar com testes de unidade
  ("Último sorteio em 17/10/2026", "Estimativa R$ 1.800.000,00", "Estimativa não informada", "Concurso 3809",
  "2 concursos", "Nenhum jogo aguardando sorteio", "1 ganhador", "2 ganhadores", "Acumulou",
  "sábado, 17/10/2026")

## 3. Web: página Painel

- [x] 3.1 Montar o Painel com cabeçalho, indicadores, último resultado, destaques e jogos do último concurso, e a
  mensagem de base vazia; verificar com testes bUnit dos cenários de "Indicadores do Painel", "Último resultado no
  Painel", "Destaques das dezenas no Painel", "Jogos do último concurso no Painel" e "Painel com a base vazia"
- [x] 3.2 Ligar "Atualizar resultados" ao `AtualizacaoConcursosServico` com andamento, mensagens e recarga do
  Painel; verificar com testes bUnit de "Atualizar resultados pelo Painel" (concursos novos com recarga, base já
  atualizada, andamento com botão desabilitado, falha inesperada registrada no log)
- [x] 3.3 Ligar "Novo jogo" à navegação para `meus-jogos?novo=true` e fazer `MeusJogos` abrir o cadastro pelo
  parâmetro, trocando o endereço para `/meus-jogos`; verificar com testes bUnit (Painel navega para o endereço;
  Meus jogos com `?novo=true` abre o cadastro com o concurso sugerido e troca o endereço; sem o parâmetro não abre;
  salvar pelo atalho mostra o jogo na lista)

## 4. Verificação final

- [x] 4.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 4.2 Verificação manual numa base separada com o histórico real: Painel com a base vazia; depois da importação,
  conferir indicadores, último resultado e destaques com as páginas Resultados e Estatísticas; cadastrar jogos pelo
  atalho "Novo jogo"; apagar os últimos concursos dessa base, cadastrar jogos para eles, acionar "Atualizar
  resultados" no Painel e conferir a recarga, os jogos do último concurso e os prêmios a receber
- [x] 4.3 Atualizar `Docs/roteiro.md` (fatia 7 concluída); verificar relendo o arquivo
