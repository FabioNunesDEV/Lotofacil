## Why

A base local já tem o histórico completo e se mantém atualizada, mas o sistema ainda não mostra nada além dos
resultados. As estatísticas por dezena (mais e menos sorteadas, há mais e menos tempo sem sair) são o primeiro
objetivo do projeto e a base para a geração de jogos, que vai usar esses números como critério de escolha.

## What Changes

- Nova página **Estatísticas** (`/estatisticas`), com item no menu logo abaixo de "Resultados", seguindo a aba
  "Por dezena" do protótipo.
- **Janela da análise**: todo o histórico (padrão) ou os últimos 10, 20, 50 ou 100 concursos. A janela afeta a
  frequência; o **atraso é sempre calculado sobre todo o histórico**.
- Para cada dezena: quantidade de sorteios na janela, percentual dos concursos, diferença do esperado (60% dos
  concursos), desvio em desvios-padrão, atraso (concursos desde a última vez que saiu) e o último concurso em que
  saiu.
- **Rankings** com as 5 primeiras dezenas de cada critério: mais sorteadas, menos sorteadas, há mais tempo sem sair
  e há menos tempo sem sair, indicando quantas outras empatam com a 5ª.
- **Mapa de calor** das 25 dezenas, no formato do volante, alternando entre frequência, atraso e desvio.
- **Tabela** com todas as dezenas e o valor esperado por dezena.
- Aviso fixo de que as estatísticas descrevem o passado e não aumentam a chance de acerto.
- Mensagem própria quando a base está vazia.
- Testes de unidade dos cálculos (domínio), do serviço e da página (bUnit) e de integração da nova consulta ao
  banco.

## Capabilities

### New Capabilities
- `estatisticas`: estatísticas por dezena calculadas a partir da base local (frequência na janela escolhida,
  desvio do esperado e atraso sobre todo o histórico), exibidas em rankings, mapa de calor e tabela.

### Modified Capabilities
<!-- Nenhuma: o novo item de menu segue a regra existente de listar apenas páginas disponíveis. -->

## Impact

- `Lotofacil.Dominio`: cálculo das estatísticas por dezena e dos rankings (regra de negócio, sem dependências);
  `IConcursoRepositorio` ganha a leitura leve de número e dezenas de todos os concursos.
- `Lotofacil.Aplicacao`: serviço de estatísticas.
- `Lotofacil.Infraestrutura`: implementação da nova consulta (só número e dezenas, sem rateio).
- `Lotofacil.Web`: página Estatísticas, componentes de mapa de calor e de ranking, item de menu.
- Sem pacotes novos e sem migration.
- Resolve a pendência de negócio "janela da análise" do `CLAUDE.md` para as estatísticas.
- Fora do escopo: aba "Por perfil do concurso", intervalo de concursos personalizado, ordenação da tabela por
  coluna, destaques no Painel e geração de jogos.
