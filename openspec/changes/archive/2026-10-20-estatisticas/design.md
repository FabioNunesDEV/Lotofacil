## Context

Motivação em `proposal.md`; comportamento em `specs/estatisticas/spec.md`. Estado atual relevante:

- `Concurso` guarda as dezenas numa coluna JSON (`PrimitiveCollection`) e o rateio em tabela própria. Carregar
  ~3.800 concursos completos traz ~19.000 linhas de rateio que as estatísticas não usam.
- Os serviços da aplicação são singletons e consultam o repositório a cada chamada; a página Resultados formata
  valores com classes estáticas da Web (`FormatacaoConcurso`) e componentes pequenos (`Dezenas`, `DetalheConcurso`).
- O protótipo (`prototipo/`) define layout, textos, cores do mapa de calor e o formato de cada número.

## Goals / Non-Goals

**Goals:**
- Cálculo puro, no domínio, testável com históricos pequenos montados à mão.
- Uma consulta leve ao banco (número e dezenas) por cálculo.
- Página fiel à aba "Por dezena" do protótipo, com MudBlazor.

**Non-Goals:**
- Cache das estatísticas entre requisições (o cálculo sobre ~3.800 concursos leva milissegundos).
- Aba "Por perfil", intervalo personalizado, ordenação da tabela, destaques no Painel.
- Gráficos com biblioteca externa (o mapa de calor é uma grade de células coloridas).

## Decisions

### Domínio (`Lotofacil.Dominio/Estatisticas` e `Concursos`)
- `ResultadoSorteio(int Numero, IReadOnlyList<int> Dezenas)` em `Concursos`: o mínimo de um concurso para
  estatísticas.
- `IConcursoRepositorio.ListarResultadosAsync(CancellationToken)` → `IReadOnlyList<ResultadoSorteio>` em ordem
  crescente de número.
- `EstatisticasDezenas.Calcular(IReadOnlyList<ResultadoSorteio> historico, int? ultimosConcursos)` (fábrica
  estática; lança `ArgumentException` com histórico vazio ou janela ≤ 0). Expõe:
  - `PrimeiroConcurso`, `UltimoConcurso`, `ConcursosNaJanela`, `TotalConcursos`, `Esperado`;
  - `Dezenas`: 25 `EstatisticaDezena(int Dezena, int Sorteios, decimal Percentual, decimal Diferenca,
    double Desvio, int? Atraso, int? UltimaVez)`, em ordem de dezena;
  - `MaisSorteadas`, `MenosSorteadas`, `MaisAtrasadas`, `MenosAtrasadas`: `Ranking(IReadOnlyList<EstatisticaDezena>
    Itens, int Empatadas)`.
- Constantes novas em `RegrasLotofacil`: `QuantidadeDezenas = 25`; a proporção 0,6 vem de
  `DezenasSorteadas / QuantidadeDezenas`, sem número mágico.
- Tipos numéricos: `decimal` para percentual, esperado e diferença (exatos: `3808 × 0,6 = 2284,8`); `double` só para
  o desvio, que depende de raiz quadrada.
- Atraso a partir dos números dos concursos (`último gravado − última vez`), e não da posição na lista, para contar
  também concursos que faltem na base. "Nunca saiu" é `null` e, nos rankings de atraso, vale como maior que qualquer
  número.
- Ranking: ordena pelo critério e depois pela dezena; pega 5; `Empatadas` = quantas dezenas fora das 5 têm o mesmo
  valor da 5ª. Uma função única recebe o seletor do valor e a direção, para os quatro rankings.

Alternativa descartada: calcular no banco com SQL. As dezenas estão em JSON, a regra ficaria fora do domínio e o
`CLAUDE.md` proíbe regra de negócio no banco.

### Aplicação
`EstatisticasServico.CalcularAsync(int? ultimosConcursos, CancellationToken)` → `EstatisticasDezenas?` (`null`
com a base vazia). Valida a janela (`null` ou maior que zero). As janelas oferecidas (10, 20, 50, 100) ficam como
constante pública do serviço (`JanelasDisponiveis`), usada pela página.

### Infraestrutura
`ConcursoRepositorio.ListarResultadosAsync`: `AsNoTracking`, projeção `Select(c => new { c.Numero, c.Dezenas })`
ordenada por número, convertida em `ResultadoSorteio`. A projeção não toca a tabela de rateio. Sem migration.

### Web
- `Components/Pages/Estatisticas.razor(.cs)`: título, subtítulo com o intervalo, chips da janela (`MudChipSet`
  com seleção única), `MudAlert` informativo com o aviso, e os blocos abaixo. Ao trocar a janela, chama o serviço
  de novo.
- `Components/Estatisticas/MapaCalor.razor(.cs)`: grade CSS 5 × 5, `MudToggleGroup` para a métrica e a legenda.
  Cor de cada célula com `color-mix` sobre as cores do tema (`--mud-palette-primary`, `--mud-palette-success`,
  `--mud-palette-error`) e intensidade como no protótipo: `5 + 85 × (valor − mín) / (máx − mín)` para frequência e
  atraso; `85 × |desvio| / maior |desvio|` para o desvio; texto branco acima de 50%. No atraso, "nunca saiu" usa a
  intensidade máxima.
- `Components/Estatisticas/RankingDezenas.razor(.cs)`: título, lista com bola + valor e a linha de empate.
- Tabela com `MudSimpleTable` (25 linhas fixas, sem paginação), com classes `acima`/`abaixo` para a cor.
- `Components/Estatisticas/FormatacaoEstatistica.cs`: formatos da spec (sinal, casas, "conc.", "—"), na cultura
  pt-BR. As bolas das dezenas usam o mesmo estilo (`.bola`) e o formato de dois dígitos do componente `Dezenas`.
- Menu: `MudNavLink` "Estatísticas" (ícone `BarChart`) abaixo de "Resultados".
- Estilos novos em `wwwroot/app.css` (grade do mapa e listas de ranking), seguindo o protótipo.

### Testes
- **Unidade (domínio)**: históricos pequenos montados à mão — sorteios, percentual, esperado, diferença e desvio
  exatos; janela dos últimos N e base menor que N; atraso independente da janela; atraso com lacuna na base;
  dezena que nunca saiu; rankings com empate e desempate pela dezena; validações.
- **Unidade (aplicação)**: base vazia → `null`; janela repassada; janela inválida rejeitada.
- **Unidade (bUnit)**: menu, aviso, chips e subtítulo, troca de janela, mapa (troca de métrica, textos, classe de
  cor do desvio), rankings (valores e empate), tabela (uma linha completa e o esperado), base vazia.
- **Integração**: `ListarResultadosAsync` com SQLite em memória (ordem, dezenas, sem rateio necessário).
- **Manual**: comparar algumas frequências e atrasos da página com um cálculo independente feito sobre o XLSX real.

## Risks / Trade-offs

- [Usuário interpretar o desvio como tendência] → Aviso fixo na página e legendas descritivas; nada de linguagem
  como "quente"/"frio" ou "vai sair".
- [Arredondamento de exibição diferente do esperado (ex.: 0,645 → "0,6")] → Arredondamento padrão do .NET na
  formatação (`ToString("+0.0;-0.0;0.0")`), coberto por testes com os valores da spec.
- [Crescimento da base] → ~300 concursos por ano; a consulta leve e o cálculo em memória continuam baratos por
  muitos anos.
- [Cores do mapa sem contraste suficiente] → Texto branco nas células mais intensas, como no protótipo; conferido
  na verificação manual.

## Migration Plan

Sem migration de banco e sem configuração nova. Para desfazer, reverter o código.
