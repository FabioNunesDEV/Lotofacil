# Roteiro das próximas fatias

- **Atualizado em:** 24/10/2026
- **Concluídas:** 0 `estrutura-solucao` · 1 `concursos-base` · 2 `importacao-historico` · 3 `atualizacao-concursos` ·
  4 `estatisticas` · 5 `jogos-e-conferencia` · 6 `apostas-multiplas` · 7 `painel` · 8 `geracao-de-jogos` ·
  9 `estatisticas-por-perfil` · 10 `filtros-de-perfil-na-geracao` · 11 `aviso-jogos-premiados`

Cada fatia vira uma change do OpenSpec (`/opsx:propose` → `/opsx:apply` → `/opsx:archive`), numa branch própria
criada a partir da `main` atualizada. As decisões marcadas como **a confirmar** são perguntadas ao usuário no início
da fatia; a proposta indicada é o ponto de partida.

As estratégias de geração vêm dos arquivos desta pasta (`lotofacil-analise-adital.md`,
`lotofacil-analise-lotodicas-estatisticas.md`, `lotofacil-caixa-regras-e-dados.md`). Não há outro material.

> Nenhuma estratégia aumenta a probabilidade de acerto: cada combinação de 15 dezenas tem 1 chance em 3.268.760.
> As estratégias organizam os jogos segundo critérios escolhidos pelo usuário. O único efeito real é evitar
> combinações populares, que reduz a divisão do prêmio, não a chance de ganhar.

## Fatia 7 — `painel` (concluída)

Preencheu a página Painel (`/`), que só tinha o título, com o resumo do protótipo. Não depende de definição de negócio.

- Botão "Atualizar resultados" (mesmo serviço da página Resultados).
- Indicadores: concursos na base (e data do último sorteio gravado), próximo concurso com a estimativa de prêmio
  gravada no último concurso, jogos aguardando sorteio, prêmios a receber (dentro do prazo).
- Último resultado: dezenas, ordem do sorteio (quando houver) e rateio por faixa.
- Seus jogos no último concurso, conferidos (ou aviso de que não há jogos nele).
- Top 5 dezenas mais sorteadas e mais atrasadas (todo o histórico).
- Base vazia: mensagem pedindo a importação do histórico.

**Decidido:** atalho só para "Novo jogo" (abre Meus jogos com o cadastro aberto); "Prêmios a receber" soma os
prêmios ainda no prazo de 90 dias. O atalho "Gerar jogos" entra na fatia 8.

## Fatia 8 — `geracao-de-jogos` (concluída)

Nova página "Gerar jogos" (`/gerar-jogos`, no menu entre Estatísticas e Meus jogos), seguindo o protótipo.

- Concurso alvo (o próximo, como no cadastro), quantidade de jogos, dezenas por jogo (15 a 20), estratégia, janela
  da análise e custo total (quantidade × C(n, 15) × preço).
- Volante de dezenas **fixas** (em todos os jogos) e **excluídas** (em nenhum): um clique fixa, outro exclui, outro
  limpa.
- Lista dos jogos gerados com o perfil de cada um (pares/ímpares, soma, repetidas do último concurso), remover um
  jogo, "Gerar novamente" e "Salvar jogos" (pelo `JogosServico`, com as mesmas regras do cadastro manual).
- Aviso fixo na página de que a estratégia não aumenta a chance de acerto.
- Atalho "Gerar jogos" no cabeçalho do Painel, ao lado de "Novo jogo".

Estratégias (dos arquivos de `docs/`):

| Estratégia | Critério para escolher as dezenas livres |
|---|---|
| Aleatória (Surpresinha) | Todas as livres com o mesmo peso |
| Mais sorteadas ("quentes") | Peso maior para as mais frequentes na janela |
| Menos sorteadas ("frias") | Peso maior para as menos frequentes na janela |
| Mais atrasadas | Peso maior para as há mais tempo sem sair |
| Equilibrada | Metade das livres pelas quentes, metade pelas atrasadas |

**Decidido:** sorteio ponderado (peso = 1 + quantidade de dezenas com valor pior no critério); janela igual às
Estatísticas, padrão "Todos"; 1 a 100 jogos, padrão 10; fixas podem chegar às dezenas por jogo; repetidos evitados,
com aviso quando faltam combinações; duplicados recusados ao salvar e os demais gravados, com o resumo.

## Fatia 9 — `estatisticas-por-perfil` (concluída)

A página Estatísticas ganhou as abas "Por dezena" (conteúdo anterior) e "Por perfil do concurso"; a janela e o aviso
ficam fora das abas e valem para as duas.

- Um cartão por perfil: pares/ímpares, primos, múltiplos de 3, Fibonacci, soma, repetidas do concurso anterior e
  moldura/miolo do volante.
- Cada linha mostra observado (na janela) × teórico (distribuição hipergeométrica; soma pela contagem das 3.268.760
  combinações), com barra e marca do teórico, e o atraso da faixa sobre todo o histórico.
- Conferido com o histórico real: observado igual a um cálculo independente sobre o XLSX e próximo do Lotodicas.

**Decidido:** os 7 perfis acima; soma em faixas de 15 (120–134 … 255–270); uma linha por quantidade, com as pontas de
chance teórica abaixo de 1% somadas à vizinha mais próxima com pelo menos 1% ("≤ 4 pares", "≥ 10 pares",
"≤ 164", "≥ 225"); atraso por faixa ("último" quando saiu no último concurso, "—" se nunca saiu); repetidas
comparam com o concurso gravado imediatamente antes, sem contar o primeiro da base.

## Fatia 10 — `filtros-de-perfil-na-geracao` (concluída)

A página Gerar jogos ganhou a seção recolhível "Filtros de perfil", desligada ao abrir.

- Mínimo e máximo (cada um opcional) para os 7 perfis da fatia 9: pares, primos, múltiplos de 3, Fibonacci, soma,
  repetidas do último concurso gravado e moldura, com a dica de chance teórica quando o jogo tem 15 dezenas.
- "Evitar padrões populares": campo "Maior sequência permitida" (9 ao ligar). É o único critério com efeito real,
  sobre a divisão do prêmio.
- Jogos fora dos filtros são descartados no sorteio e na enumeração de poucas combinações; avisos quando faltam
  jogos e recusa quando nenhum atende ou o mínimo passa do máximo.

**Decidido:** os 7 perfis; em apostas de 16 a 20 dezenas os filtros valem sobre o jogo inteiro (dica só para 15);
maior sequência padrão 9, que corta só as sequências de 10 ou mais (~1% das combinações de 15). Ficaram de fora o
critério "dezenas até 31 (datas)", que não se aplica (a Lotofácil vai até 25), e linhas/colunas completas do volante,
que aparecem em 48,8% das combinações (não são padrão raro). Sequência de 5 ou mais aparece em 57% das combinações.

## Fatia 11 — `aviso-jogos-premiados` (concluída)

Depois de "Atualizar resultados" (páginas Resultados e Painel), os jogos dos concursos gravados nessa atualização são
conferidos e o resultado é avisado logo após o desfecho.

- Algum premiado: diálogo "Jogos premiados" com o resumo ("3 de 6 jogos conferidos foram premiados."), cada concurso
  com data e prazo ("Receber até …" ou "Prazo encerrado em …"), os jogos premiados (número no concurso, dezenas a
  mais, acertos e prêmio), o total e os botões "Ver em Meus jogos" e "Fechar".
- Jogos sem prêmio: aviso "Seus <n> jogos dos concursos novos foram conferidos; nenhum foi premiado.".
- Sem jogos nos concursos novos: só o desfecho da atualização.

**Decidido:** só os jogos dos concursos gravados pela atualização (também quando ela é interrompida depois de gravar);
nada novo no banco; conferência pelas regras e pelo rateio já usados em Meus jogos.

## Próximas fatias (ordem de esforço, do menor para o maior)

Ordem combinada com o usuário em 24/10/2026. As decisões **a confirmar** são perguntadas no início de cada fatia.

### Fatia 12 — exportar jogos de um concurso

Texto com os jogos de um concurso para conferir ou levar à lotérica, a partir de Meus jogos.

**A confirmar:** formato (copiar texto ou baixar arquivo), o que entra (dezenas, custo, conferência) e de onde se
aciona.

### Fatia 13 — página Configurações

Preço da aposta mínima (hoje no `appsettings.json`) editável pela interface e gravado na base.

**A confirmar:** quais valores entram além do preço (valores fixos das faixas 11 a 13 para estimativas, pausa entre
requisições, URL da Caixa) e se a tabela de preços de 16 a 20 dezenas é só exibida.

### Fatia 14 — ordem do sorteio dos concursos antigos

Preencher a ordem do sorteio dos concursos importados pelo XLSX buscando cada um na API da Caixa, de forma
incremental, com pausa entre as requisições e retomada depois de interrupção.

**A confirmar:** onde fica o botão, se busca tudo de uma vez ou em lotes e o que fazer com concursos que a API não
devolver.

### Fatia 15 — Teimosinha e Bolão

Continuam pendentes de definição de negócio (ver `CLAUDE.md`).

**A confirmar:** regras de cada modalidade, como cadastrar, conferir e mostrar.
