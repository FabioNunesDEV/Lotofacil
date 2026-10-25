## Why

Em Gerar jogos, os "Filtros de perfil" mostram só o nome de cada perfil e os campos "Mín." e "Máx.". Quem não lembra
quais dezenas são primas, o que é a moldura ou que valores são usuais fica sem base para preencher os limites. As
explicações existem só na aba "Por perfil" das Estatísticas, em outra página.

## What Changes

- Frase no topo da seção: "Mín. e Máx.: quantas dezenas do jogo entram em cada grupo. Deixe vazio para não limitar.".
- Embaixo do nome de cada filtro, sempre visível (sem depender do mouse, para funcionar no celular), o que ele conta:
  as dezenas do grupo, a soma com a faixa possível para as dezenas por jogo escolhidas, as repetidas do último
  concurso ou a moldura.
- Com 15 dezenas por jogo, a mesma linha traz o valor mais comum do perfil e a sua chance, por exemplo "mais comum: 7
  ou 8 (57%)" em Pares e "mais comum: 180 a 209 (59%)" em Soma, calculado sobre as 3.268.760 combinações, como a
  "Chance teórica". Com 16 a 20 dezenas, só a explicação aparece.
- Nada muda na geração nem nos limites aceitos.

## Capabilities

### New Capabilities
<!-- Nenhuma. -->

### Modified Capabilities
- `geracao-jogos`: requisito novo "Explicação dos filtros", com a frase do topo, o texto de cada filtro e o valor mais
  comum só com 15 dezenas.

## Impact

- Domínio: `DistribuicaoTeorica` ganha o cálculo da faixa mais comum de um perfil.
- Web: `FiltrosPerfil.razor` mostra a frase e as explicações; os textos ficam em `FormatacaoGeracao`.
- Testes de unidade e bUnit. Sem pacote novo.
