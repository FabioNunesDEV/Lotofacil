## 1. Aplicação

- [x] 1.1 Criar `ConferenciaConcursos` e `JogosServico.ConferirConcursosAsync(primeiro, ultimo)`, extraindo o
  agrupamento e a conferência de `ListarAsync` para um método comum; verificar com testes de unidade: só entram os
  concursos do intervalo, em ordem crescente; números dos jogos iguais aos de Meus jogos; contas de conferidos,
  premiados e total; grupos premiados só com os jogos premiados; sem jogos no intervalo, tudo zerado; `ListarAsync`
  com os testes atuais passando

## 2. Web

- [x] 2.1 Criar `FormatacaoPremios` (resumo com singulares, concurso, prazo a receber ou encerrado, linha do jogo com
  e sem "<n> dezenas", total, mensagem de nenhum prêmio com um e com vários jogos); verificar com testes de unidade
  dos textos da spec
- [x] 2.2 Criar o componente `DialogoJogosPremiados` e o `AvisoJogosPremiados.MostrarAsync`; verificar com testes
  bUnit: diálogo com os dois concursos do cenário da spec, concurso sem prêmio fora, aposta múltipla, prazo
  encerrado, "Ver em Meus jogos" navega para `/meus-jogos`, "Fechar" fecha; snackbar de nenhum prêmio; nada sem
  jogos
- [x] 2.3 Chamar o aviso depois de "Atualizar resultados" nas páginas Resultados e Painel; verificar com testes bUnit
  das duas páginas: atualização com jogo premiado abre o diálogo depois do desfecho, jogos sem prêmio exibem o
  aviso, jogos de concursos antigos não entram, atualização interrompida depois de gravar avisa, nada gravado não
  confere; testes atuais das páginas passando

## 3. Verificação final

- [x] 3.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 3.2 Verificação manual numa cópia da base sem os últimos concursos e com jogos cadastrados para eles
  (premiados e não premiados, 15 e 16 dezenas), atualizando pela API real nas páginas Resultados e Painel: diálogo,
  valores conferidos com Meus jogos, "Ver em Meus jogos", aviso de nenhum prêmio, layout em 390px
- [x] 3.3 Atualizar `Docs/roteiro.md` (fatia 11 concluída; plano das fatias 12 a 15: exportar jogos, Configurações,
  ordem do sorteio dos concursos antigos, Teimosinha e Bolão); verificar relendo o arquivo
