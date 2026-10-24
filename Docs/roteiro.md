# Roteiro das próximas fatias

- **Atualizado em:** 24/10/2026
- **Concluídas:** 0 `estrutura-solucao` · 1 `concursos-base` · 2 `importacao-historico` · 3 `atualizacao-concursos` ·
  4 `estatisticas` · 5 `jogos-e-conferencia` · 6 `apostas-multiplas` · 7 `painel` · 8 `geracao-de-jogos` ·
  9 `estatisticas-por-perfil` · 10 `filtros-de-perfil-na-geracao` · 11 `aviso-jogos-premiados` ·
  12 `exportar-jogos` · 13 `pagina-configuracoes` · 14 `ordem-sorteio-antigos` · 15 `teimosinha`

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

## Fatia 12 — `exportar-jogos` (concluída)

Em Meus jogos, cada concurso ganhou o botão "Exportar", que abre o diálogo "Exportar · concurso <n>" com o texto dos
jogos e os botões "Copiar" (área de transferência), "Baixar .txt" (`concurso-<n>.txt`, UTF-8) e "Fechar".

- Texto: "Lotofácil · concurso <n>", "<quantidade> · <custo>", linha em branco e um jogo por linha
  ("01) 01 02 04 …", com " (16 dezenas)" nas apostas múltiplas).

**Decidido:** sem conferência no texto, nem em concursos sorteados; só em Meus jogos; linhas com quebra do Windows;
cópia e download sem pacote nem arquivo `.js` (interop do navegador e link com data URL).

## Fatia 13 — `pagina-configuracoes` (concluída)

Página "Configurações" (`/configuracoes`), último item do menu, com o campo "Aposta mínima (15 dezenas)", o botão
"Salvar" ("Configurações salvas.") e a tabela de 15 a 20 dezenas (apostas equivalentes e valor), que acompanha o
preço digitado.

- O preço salvo fica na tabela `ConfiguracaoApostas` (linha única) e vale na hora em cadastro, Gerar jogos, Meus jogos
  e exportação. Sem linha gravada, vale `Lotofacil:PrecoApostaMinima` do `appsettings.json` (R$ 3,50).
- Preço aceito de R$ 0,01 a R$ 100,00, com até dois decimais; fora disso o campo avisa e nada é gravado.

**Decidido:** só o preço é editável (prêmios fixos de 11 a 13 não são usados em cálculo; URL e pausa da Caixa
continuam no `appsettings.json`); preço único, sem vigência (jogos antigos também passam a usar o preço novo); tabela
só exibida.

## Fatia 14 — `ordem-sorteio-antigos` (concluída)

Botão "Preencher ordem do sorteio" na página Resultados, habilitado só com concursos sem ordem (os 3.808 importados
pelo XLSX). Um clique busca na API da Caixa, do menor para o maior e com a pausa de sempre, só a ordem de cada
concurso sem ela, e grava cada uma assim que chega.

- Andamento "Buscando a ordem do sorteio do concurso <n> (<posição> de <total>)…" com "Interromper"; sair da página
  também interrompe. O próximo clique continua pelos que faltam.
- Concurso que falhar (comunicação, resposta sem ordem, ordem com outras dezenas) é pulado. Depois de 5 falhas
  seguidas, a busca é encerrada.
- A Caixa limita as consultas: com a pausa de 500 ms, respondeu 429 depois de cerca de 40 concursos. No 429 a busca
  espera 1, 2, 4 e 8 minutos e tenta de novo o mesmo concurso, mostrando a hora da nova tentativa.

**Decidido:** botão em Resultados; tudo num clique com "Interromper"; falhas puladas com resumo no fim; espera com
nova tentativa no limite de consultas (decidido depois de achado na verificação manual).

## Fatia 15 — `teimosinha` (concluída)

Teimosinha: grupo com nome de um ou mais jogos que se repete em todo concurso até o usuário remover. É marcada com o
switch "Teimosinha (repetir em todo concurso)" ao salvar no cadastro manual ou em Gerar jogos, com o nome sugerido
entre as ativas.

- Os jogos dela viram jogos comuns em Meus jogos, com a marca "Teimosinha · <nome>": criados ao salvar e, a cada
  atualização/importação e na inicialização, até o próximo concurso (concursos pulados também), antes do aviso de
  jogos premiados. Aviso "Teimosinhas: 2 jogos criados para o concurso 3814.".
- Página "Teimosinhas" (`/teimosinhas`) com nome, "Desde o concurso", jogos, custo por concurso e "Remover"
  (desativa e mantém os jogos já criados).

**Decidido:** registro permanente até remover (não o número fixo de concursos da Caixa); grupo com nome; criar jogos
a cada concurso em vez de conferir à parte; página própria. Bolão descartado pelo usuário.

## Próximas fatias

O roteiro combinado terminou. Novas fatias dependem de pedido do usuário.
