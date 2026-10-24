## Context

Motivação em `proposal.md`; comportamento em `specs/estatisticas/spec.md`. Estado atual relevante:

- `EstatisticasServico.CalcularAsync(janela)` lê `ListarResultadosAsync` (número e dezenas de todos os concursos) e
  chama `EstatisticasDezenas.Calcular(historico, janela)` no domínio; a página guarda a janela e recalcula ao trocar.
- A página `Estatisticas` não tem abas: janela e intervalo no cabeçalho, aviso fixo, e depois mapa, rankings e
  tabela.
- `Combinatoria.Combinacoes` já existe; C(25, 15) = 3.268.760 cabe em `int`.
- `FormatacaoEstatistica.Percentual` já formata "31,18%" e `Indefinido` é "—".

## Goals / Non-Goals

**Goals:**
- Cálculo dos perfis, faixas, teórico e atraso no domínio, testável sem banco e sem tela.
- Teórico calculado uma vez por execução, sem depender da base.
- Reaproveitar a consulta leve dos resultados e o padrão de janela já usado pela aba por dezena.

**Non-Goals:**
- Guardar perfis no banco (tudo calculado na leitura, como as estatísticas por dezena).
- Filtros de perfil na geração (fatia 10), que vão reaproveitar o valor do perfil por concurso.

## Decisions

### Domínio (`Lotofacil.Dominio/Estatisticas/Perfis`)
- `TipoPerfil` (enum): `Pares`, `Primos`, `MultiplosDe3`, `Fibonacci`, `Soma`, `Repetidas`, `Moldura`.
- `ValorPerfil` (estática): `Calcular(tipo, dezenas, anterior)` devolve o valor do concurso no perfil (quantidade de
  dezenas do conjunto, soma, ou repetidas do anterior). Os conjuntos (pares, primos, múltiplos de 3, Fibonacci,
  moldura) ficam como constantes do domínio.
- `DistribuicaoTeorica` (estática, resultados calculados uma vez em campos `static readonly`):
  - perfis de contagem: hipergeométrica com `Combinatoria.Combinacoes` e K do conjunto (15 para repetidas);
  - soma: contagem por programação dinâmica (quantos subconjuntos de 15 dezenas de 1 a 25 têm cada soma), de 120 a
    270, somando 3.268.760.
  - Alternativa: enumerar as 3.268.760 combinações. Descartada: a programação dinâmica dá o mesmo resultado com
    25 × 15 × 270 passos.
- `FaixaPerfil(Minimo, Maximo, AbertaAbaixo, AbertaAcima, Observado, Teorico, Atraso)`: limites da linha (iguais
  numa linha simples de contagem), se agrupou a ponta de baixo ou de cima, percentuais e atraso (`null` se nunca
  saiu).
- `EstatisticasPerfis.Calcular(historico, ultimosConcursos)` devolve `IReadOnlyList<DistribuicaoPerfil>` (tipo e
  faixas), na ordem dos tipos:
  1. faixas base: uma por valor possível (teórico > 0) ou as 10 faixas de soma da spec;
  2. agrupa as pontas: da ponta de baixo, enquanto o teórico da faixa for menor que 1%, junta com a seguinte (e o
     mesmo pela ponta de cima), marcando `AbertaAbaixo`/`AbertaAcima`;
  3. observado: percorre os concursos da janela (repetidas: pulando o primeiro da base, comparando com o concurso
     anterior na lista ordenada) e conta em qual faixa cada valor cai;
  4. atraso: percorre todo o histórico guardando o último número em que cada faixa saiu; atraso = último gravado −
     esse número.
  - A janela segue a mesma regra de `EstatisticasDezenas` (os N concursos de maior número); a validação da janela
    continua no serviço.

### Aplicação
- `EstatisticasServico.CalcularPerfisAsync(ultimosConcursos)` devolve `IReadOnlyList<DistribuicaoPerfil>?` (`null` com
  a base vazia), com a mesma validação de janela de `CalcularAsync`.
  - Alternativa: um único método que devolva dezenas e perfis. Descartada: mudaria o contrato usado pela geração de
    jogos; dois métodos sobre a mesma consulta leve são simples e a base é local.

### Web
- `Estatisticas.razor`: `MudTabs` com os painéis "Por dezena" (conteúdo atual, sem mudança) e "Por perfil do
  concurso"; janela, intervalo e aviso continuam acima das abas. Ao trocar a janela, a página recalcula dezenas e
  perfis; o índice da aba ativa fica no estado da página, então a aba não muda.
- Componente `CartaoPerfil` (`Components/Estatisticas`): título, descrição e tabela com faixa, barra (largura do
  observado e marca do teórico, escala = maior valor do cartão × 1,1, como no protótipo), observado, teórico e atraso.
- `FormatacaoPerfil`: títulos, descrições, rótulo da linha (simples, "≤ ..." ou "≥ ...") e atraso ("último", número
  ou "—"); percentuais por `FormatacaoEstatistica.Percentual`.

## Risks / Trade-offs

- [Teórico com arredondamento diferente do Lotodicas] → os testes conferem os valores da spec, calculados à parte
  (31,18%, 29,07%, 29,67%, 1,27%, 2,87%, 4,55%, 5,13%).
- [Concursos faltando na base mudam o "anterior" das repetidas] → a regra usa o concurso gravado imediatamente antes;
  com a base completa (importação + atualização) é o número anterior.
- [Dois cálculos ao trocar a janela] → consulta local e leve, os dois juntos levam milissegundos com ~3.800 concursos.
