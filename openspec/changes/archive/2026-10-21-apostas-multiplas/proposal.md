## Why

Hoje o cadastro de jogos aceita só apostas de 15 dezenas, mas a Lotofácil permite apostar de 15 a 20 dezenas no
mesmo volante. Quem faz apostas múltiplas não consegue registrá-las nem conferir o prêmio, que nesse caso soma vários
prêmios em várias faixas. Esta fatia resolve a pendência "Apostas de 16 a 20 dezenas" do `CLAUDE.md` e deixa o
cadastro pronto para a geração de jogos, que já prevê o campo "Dezenas por jogo" no protótipo.

## What Changes

- Um jogo passa a ter de **15 a 20 dezenas** distintas (01 a 25). Uma aposta de n dezenas vale **C(n, 15) apostas
  de 15** (16 → 16, 17 → 136, 18 → 816, 19 → 3.876, 20 → 15.504).
- **Custo** da aposta = C(n, 15) × preço da aposta mínima da configuração (hoje R$ 3,50), usado no cadastro, no custo
  dos grupos aguardando sorteio e no indicador "Investido".
- **Cadastro manual**: novo campo "Dezenas por jogo" (15 a 20, padrão 15) ao lado de "Concurso". O volante limita a
  marcação a esse número, "Salvar" só habilita com a quantidade exata e a contagem mostra também o custo da aposta
  ("12 de 17 dezenas · R$ 476,00"). Não há confirmação extra para apostas caras.
- **Conferência pela regra oficial da aposta múltipla**: com a acertos entre as n dezenas, a aposta recebe
  C(a, k) × C(n − a, 15 − k) prêmios de k acertos, para k de 11 a 15, cada um pago pelo valor da faixa no **rateio
  gravado do próprio concurso**. O prêmio do jogo é a soma. Para 15 dezenas o resultado é o mesmo de hoje.
- Lista de jogos: jogos de 16 a 20 dezenas mostram a quantidade de dezenas; o prêmio exibido é o total.
- Janela de conferência: mostra a quantidade de dezenas e, para apostas múltiplas premiadas, os prêmios por faixa
  (quantidade × faixa × valor).
- A regra de jogo duplicado continua: mesmas dezenas no mesmo concurso. Um jogo de 15 dezenas contido num jogo de 16
  do mesmo concurso **não** é duplicado.
- Testes de unidade e de integração cobrindo as novas regras; `CLAUDE.md` atualizado (decisões e pendências).

## Capabilities

### New Capabilities
<!-- Nenhuma. -->

### Modified Capabilities
- `jogos`: regras de um jogo (15 a 20 dezenas), cadastro (campo "Dezenas por jogo" e custo), conferência (prêmios da
  aposta múltipla), lista e indicadores (custo por quantidade de dezenas), janela de conferência (prêmios por faixa).

## Impact

- `Lotofacil.Dominio`: regras de quantidade de dezenas, cálculo de combinações, apostas equivalentes do jogo e
  conferência com prêmios por faixa.
- `Lotofacil.Aplicacao`: custo e prêmio dos grupos considerando a quantidade de dezenas de cada jogo.
- `Lotofacil.Infraestrutura`: nenhuma mudança de esquema — as dezenas já são gravadas como coleção e o índice único
  continua valendo. Sem migration.
- `Lotofacil.Web`: diálogo de cadastro (campo "Dezenas por jogo" e custo), página Meus jogos (custo, investido,
  quantidade de dezenas na linha), janela de conferência (prêmios por faixa).
- Sem pacotes novos.
- Fora do escopo: geração de jogos, dezenas fixas e excluídas, Teimosinha, Bolão, probabilidades por quantidade de
  dezenas, tabela de preços em tela de configuração.
