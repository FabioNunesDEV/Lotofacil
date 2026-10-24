## Context

A motivação está em `proposal.md` e o comportamento, em `specs/geracao-jogos/spec.md`. Estado atual relevante:

- `GeradorJogos.Gerar(parametros, estatisticas)` recusa quando há fixas demais ou faltam livres. Quando
  C(livres, n − fixas) ≤ quantidade, enumera todas as combinações; senão, sorteia com pesos até ter a quantidade, com
  até `quantidade × 1.000` tentativas, descartando jogos repetidos. O resultado é
  `ResultadoGeracao(Jogos, CombinacoesPossiveis, Recusa)`.
- `ParametrosGeracao` valida quantidade, dezenas por jogo, estratégia, fixas e excluídas.
- `GeracaoJogosServico.GerarAsync` já obtém o `ContextoGeracao`, com o último resultado gravado, e calcula o
  `PerfilJogo` de cada jogo contra ele.
- Fatia 9: `ValorPerfil.Calcular(tipo, dezenas, anterior)` conta qualquer quantidade de dezenas, e
  `DistribuicaoTeorica.Percentual(tipo, minimo, maximo)`, `MenorValor` e `MaiorValor` dão o teórico para 15 dezenas.
- `FormatacaoGeracao` (Web) monta os textos da página; `GerarJogos.razor(.cs)` tem o formulário.

## Goals / Non-Goals

**Goals:**
- Filtros no domínio, aplicados pelo próprio gerador, testáveis com um `Random` fixo e sem tela.
- Reaproveitar o cálculo dos perfis e o teórico da fatia 9, sem duplicar conjuntos de dezenas.
- Sem filtros ligados, o gerador se comporta exatamente como hoje (mesmas mensagens, mesma sequência de sorteios
  para a mesma semente).

**Non-Goals:**
- Gravar ou lembrar os filtros entre visitas.
- Contar com exatidão quantos jogos atendem aos filtros fora do caso de poucas combinações.
- Dica de chance teórica para apostas de 16 a 20 dezenas e para a maior sequência.

## Decisions

### Domínio (`Lotofacil.Dominio/Geracao`)
- **`LimitePerfil`** (record): `Tipo` (`TipoPerfil`), `Minimo` (`int?`), `Maximo` (`int?`). Método
  `Contem(valor)`, em que um lado nulo não limita.
- **`FiltrosGeracao`** (classe imutável): `Limites` (lista de `LimitePerfil`, no máximo um por perfil, só os que têm
  algum lado preenchido) e `MaiorSequenciaPermitida` (`int?`, nulo com "Evitar padrões populares" desligado). Inclui
  `Nenhum` (instância vazia), `MaiorSequenciaPadrao` (9), `Ligados` (quantidade de filtros ligados, contando a
  sequência), `Atende(jogo, ultimoResultado)`, `MaiorSequencia(dezenas)` estático, que dá o tamanho da maior corrida
  de dezenas consecutivas, e as cópias alteradas `ComLimite`, `ComMaiorSequencia` e `AjustarPara(n)`, usadas pela
  página. `Atende` usa `ValorPerfil.Calcular`, passando o último resultado como "anterior" para Repetidas.
  - Alternativa descartada: criar `TipoPerfil.Sequencia`. O enum descreve os cartões das Estatísticas, que não têm
    sequência, e o teórico dela exigiria outro cálculo.
- **`ParametrosGeracao`** ganha o parâmetro opcional `filtros` (padrão `FiltrosGeracao.Nenhum`). Limites fora de
  0..n (soma: n(n+1)/2..n(51−n)/2) ou sequência fora de 1..n lançam `ArgumentOutOfRangeException`, como as demais
  faixas, porque a página não deixa digitar esses valores. Mínimo > máximo **não** lança: o gerador recusa com
  mensagem, para a página só exibi-la.
- **`GeradorJogos.Gerar(parametros, estatisticas, ultimoResultado)`**:
  1. recusas atuais (fixas demais, livres insuficientes);
  2. recusa por mínimo > máximo, no primeiro limite em ordem de `TipoPerfil` ("No filtro de pares, ...");
  3. enumeração (C(livres, a) ≤ quantidade): filtra os jogos enumerados. `CombinacoesPossiveis` passa a ser a
     quantidade dos que atendem (preenchida quando é menor que a pedida, como hoje). Se nenhum atende, recusa com
     "Nenhum jogo atendeu aos filtros. ...";
  4. sorteio: o jogo sorteado só entra se `Atende`. Sem filtros, o limite de tentativas continua
     `quantidade × 1.000`. Com filtros, passa a ser `max(quantidade × 1.000, 100.000)`. Se nenhum jogo for achado,
     recusa; se faltar algum, o novo campo `ResultadoGeracao.JogosPedidos` diz quantos foram pedidos, e a página
     monta o aviso.
  - Sortear com pesos e descartar o que não passa equivale a sortear pelos pesos **entre os jogos que atendem**, o
    que mantém o sentido da estratégia.
  - Alternativa descartada: enumerar todas as combinações que atendem e sortear entre elas. Com 15 dezenas, chega a
    3.268.760 combinações por geração e exigiria um peso por jogo diferente do sorteio por dezena da fatia 8.
- **`ResultadoGeracao`** ganha `ComFiltros` (bool), que escolhe o texto "com essas fixas e excluídas" ou "com essas
  fixas, excluídas e filtros", e `JogosPedidos` (`int?`, preenchido só quando o sorteio achou menos que o pedido).
- As mensagens de recusa ficam no domínio, como as atuais. O nome do filtro na mensagem ("de pares", "da moldura")
  sai de um `switch` em `FiltrosGeracao`.

### Aplicação
- `GeracaoJogosServico.GerarAsync` passa `contexto.UltimoResultado` ao gerador e repassa `ComFiltros` e
  `JogosPedidos` em `JogosGerados`. Nenhum serviço novo.

### Web
- A seção "Filtros de perfil" é um `MudExpansionPanels` com um painel fechado ao abrir. O cabeçalho mostra o resumo
  de `FormatacaoGeracao.FiltrosLigados(k)`.
- Um componente `FiltrosPerfil` (`Components/Geracao`) recebe `DezenasPorJogo` e `UltimoConcurso` e expõe
  `@bind-Filtros` (`FiltrosGeracao`). Por perfil, tem dois `MudNumericField<int?>` com `Min`/`Max` dados por
  `FiltrosGeracao.MenorLimite(tipo, n)`/`MaiorLimite(tipo, n)` no domínio. Tem também o `MudSwitch` "Evitar padrões
  populares", que mostra o campo da sequência (9 ao ligar), e o botão "Limpar filtros". A página continua só
  montando `ParametrosGeracao`.
- Dica: `FormatacaoGeracao.ChanceTeorica(limite)` usa `DistribuicaoTeorica.Percentual(tipo, min ?? MenorValor,
  max ?? MaiorValor)` e formata "Chance teórica: 57,17%". Ela só é exibida com n = 15. Para Repetidas, o teórico da
  fatia 9 (hipergeométrica com as 15 do concurso anterior) é a mesma conta.
- Avisos: `FormatacaoGeracao.AvisoPoucasCombinacoes(possiveis, comFiltros)` e `AvisoFiltros(gerados, pedidos)`,
  com singular e plural, exibidos pelo `ISnackbar`, como o aviso atual.
- Ao trocar "Dezenas por jogo", limites que ficarem fora da nova faixa são ajustados para o limite mais próximo,
  como o `MudNumericField` já faz ao editar. Assim, a página nunca monta parâmetros inválidos.

## Risks / Trade-offs

- [Filtros muito restritos levam o sorteio a esgotar as tentativas sem achar jogos que existem] → o aviso fala em
  "poucos jogos atendem" (não afirma que não existem), a recusa sugere afrouxar os limites e o mínimo de 100.000
  tentativas cobre filtros que deixam passar cerca de 0,01% das combinações para 10 jogos.
- [Geração mais lenta com filtros apertados: até ~100.000 sorteios] → custo de milissegundos a poucas centenas de
  milissegundos. O botão já fica desabilitado durante a geração.
- [Usuário entender o filtro como aumento de chance] → texto fixo na seção, além do aviso geral da página, e a dica
  mostra só a parte das combinações, nunca "chance de ganhar".
- [Repetidas dependem do último concurso gravado; após atualizar os resultados, o mesmo filtro dá outros jogos] →
  o rótulo mostra o número do concurso ("Repetidas do 3810").

## Migration Plan

Sem migration nem dado gravado. A geração sem filtros continua igual, e os testes atuais do gerador devem passar sem
mudança, exceto a assinatura, que ganha o último resultado.
