## Context

A motivação está em `proposal.md` e o comportamento, em `specs/geracao-jogos/spec.md`. Estado atual relevante:

- `FiltrosPerfil.razor` (Web, `Components/Geracao`) monta uma linha por `TipoPerfil` com o rótulo
  (`FormatacaoGeracao.RotuloFiltro`), os campos "Mín."/"Máx." e, com 15 dezenas e algum limite, a
  `FormatacaoGeracao.ChanceTeorica`. No topo há o aviso `AvisoFiltros`.
- `DistribuicaoTeorica` (domínio, `Estatisticas/Perfis`) já sabe, para cada perfil, quantas das 3.268.760
  combinações de 15 dezenas têm cada valor (`Combinacoes`), o percentual de uma faixa (`Percentual`) e os extremos
  (`MenorValor`/`MaiorValor`).
- `EstatisticasPerfis.FaixasBase` agrupa a Soma em faixas de 15 a partir de 120 (`LarguraFaixaSoma`, privada).
- `FiltrosGeracao.MenorLimite`/`MaiorLimite(tipo, dezenasPorJogo)` dão a menor e a maior soma para n dezenas.
- As descrições da aba "Por perfil" (`FormatacaoPerfil.Descricao`) foram escritas para o cartão das Estatísticas;
  algumas não servem aqui (a Soma fala em "faixas de 15", a Repetidas em "concurso anterior").

## Goals / Non-Goals

**Goals:**
- Explicar cada filtro na própria linha, também no celular.
- Calcular o "mais comum" no domínio, testável e coerente com a "Chance teórica".

**Non-Goals:**
- O "mais comum" para 16 a 20 dezenas, dicas ao passar o mouse, mudar a geração ou os limites.

## Decisions

- **`DistribuicaoTeorica.FaixaMaisComum(TipoPerfil tipo)`** devolve `(int Minimo, int Maximo)`: o par de valores
  vizinhos com mais combinações. Na Soma, o par de faixas vizinhas de 15, alinhadas como na aba "Por perfil" (120–134,
  135–149, …). Em caso de empate, fica o par de menor valor. O percentual vem do `Percentual` que já existe, o mesmo da
  "Chance teórica".
  - `LarguraFaixaSoma` passa a ser uma constante pública de `DistribuicaoTeorica`, usada também por
    `EstatisticasPerfis`, para as duas telas não divergirem.
  - Alternativa descartada: fixar os sete textos com os números. Ficariam sem ligação com o cálculo, e uma mudança na
    regra passaria despercebida.
- **Textos em `FormatacaoGeracao`:**
  - `InstrucaoFiltros` (a frase do topo);
  - `ExplicacaoFiltro(tipo, ultimoConcurso, dezenasPorJogo)`: o texto do grupo; na Soma, usa
    `FiltrosGeracao.MenorLimite`/`MaiorLimite`;
  - `MaisComum(tipo)`: " · mais comum: 7 ou 8 (57%)" ou " · mais comum: 180 a 209 (59%)", com o percentual
    arredondado para inteiro.
  - Ficam separados de `FormatacaoPerfil.Descricao`, porque o contexto é outro (jogo inteiro e último concurso, não
    o cartão da estatística).
- **`FiltrosPerfil.razor`:** a frase vai abaixo do aviso atual. Na linha, a explicação (`explicacao-filtro`) ocupa a
  largura toda, embaixo do nome e dos campos "Mín."/"Máx.", e a "Chance teórica" vem depois dela. O "mais comum" só
  entra com `DezenasPorJogo == 15`.
  - Alternativa descartada: explicação embaixo do nome, ao lado dos campos. A seção fica numa coluna estreita também
    no desktop, e o texto se espremia em quatro ou cinco linhas.

## Risks / Trade-offs

- [Mais altura no painel] → ele é recolhível e fica fechado ao abrir a página; a explicação é uma linha pequena.
- [Arredondamento diferente da "Chance teórica"] → o "mais comum" usa inteiro (57%) e a "Chance teórica", duas casas
  (57,17%), para ficar curto. O cálculo é o mesmo.

## Migration Plan

Nada a migrar.
