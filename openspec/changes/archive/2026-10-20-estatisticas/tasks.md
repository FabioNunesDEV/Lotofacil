## 1. Domínio e repositório

- [x] 1.1 Criar `ResultadoSorteio` e `IConcursoRepositorio.ListarResultadosAsync`, implementado com projeção de
  número e dezenas em ordem crescente; verificar com teste de integração em `ConcursoRepositorioTestes` (ordem,
  dezenas e base vazia)
- [x] 1.2 Adicionar `RegrasLotofacil.QuantidadeDezenas` e criar `EstatisticaDezena`, `Ranking` e
  `EstatisticasDezenas.Calcular`; verificar com testes de unidade em `Lotofacil.Testes.Unidade/Dominio` para cada
  cenário de "Janela da análise", "Estatísticas por dezena" e "Rankings" da spec (valores exatos com históricos
  pequenos, últimos N e base menor que N, atraso independente da janela e com lacuna, dezena que nunca saiu,
  empates e desempate, validações)

## 2. Serviço

- [x] 2.1 Criar `EstatisticasServico` (`CalcularAsync`, `JanelasDisponiveis`) e registrá-lo no `Program.cs`;
  verificar com testes de unidade (base vazia → `null`, janela repassada ao cálculo, janela ≤ 0 rejeitada)

## 3. Página Estatísticas

- [x] 3.1 Criar `FormatacaoEstatistica` com os formatos da spec; verificar com testes de unidade (sinal e casas da
  diferença e do desvio, "0,0" sem sinal, percentual, esperado, "conc.", "último concurso", "nunca saiu", "—")
- [x] 3.2 Criar a página `/estatisticas` (título, aviso, chips da janela, intervalo, base vazia) e o item de menu
  "Estatísticas" abaixo de "Resultados"; verificar com testes bUnit dos cenários de "Acesso à página Estatísticas",
  "Janela da análise" e "Base vazia"
- [x] 3.3 Criar `RankingDezenas` e a tabela das dezenas na página; verificar com testes bUnit dos cenários de
  "Rankings" e "Tabela das dezenas"
- [x] 3.4 Criar `MapaCalor` (grade 5 × 5, métricas, cores, legendas) e os estilos em `app.css`; verificar com
  testes bUnit dos cenários de "Mapa de calor" (troca de métrica, textos das células, cor acima/abaixo no desvio)

## 4. Verificação final

- [x] 4.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 4.2 Verificação manual com uma base separada e o histórico real: abrir a página, conferir layout, aviso,
  janelas, mapa nas três métricas, rankings e tabela; comparar sorteios e atraso de algumas dezenas, em "Todos" e
  "Últimos 10", com um cálculo independente sobre o XLSX da Caixa
- [x] 4.3 Registrar no `CLAUDE.md` que a pendência "janela da análise" foi definida para as estatísticas (todo o
  histórico ou últimos 10/20/50/100; atraso sempre sobre todo o histórico); verificar relendo a seção
