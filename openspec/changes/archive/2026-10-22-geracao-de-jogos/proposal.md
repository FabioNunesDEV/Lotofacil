## Why

Hoje o usuário só cadastra jogos marcando dezena por dezena no volante. O sistema já calcula frequência e atraso
das dezenas, mas não as usa para montar jogos. Esta fatia (8 do `Docs/roteiro.md`) cria o gerador de jogos com as
estratégias descritas nos arquivos de `docs/`, dezenas fixas e excluídas, e grava os jogos pelo mesmo serviço do
cadastro manual. As decisões que estavam pendentes no `CLAUDE.md` (critério, janela, quantidade e limite das fixas)
foram tomadas pelo usuário no início da fatia.

## What Changes

- Nova página **"Gerar jogos"** (`/gerar-jogos`), no menu entre "Estatísticas" e "Meus jogos", com:
  - **concurso alvo** (sugerido: último gravado + 1), **quantidade de jogos** (1 a 100, padrão 10), **dezenas por
    jogo** (15 a 20), **estratégia** e **janela da análise** (Todos, padrão, e últimos 10, 20, 50 ou 100);
  - **volante de fixas e excluídas**: um clique fixa a dezena (em todos os jogos), outro a exclui (de todos os
    jogos), outro a libera; fixas podem chegar à quantidade de dezenas por jogo;
  - **custo total** da geração: quantidade × C(n, 15) × preço da aposta mínima;
  - **aviso fixo** de que nenhuma estratégia aumenta a chance de acerto.
- **Estratégias** (dos arquivos de `docs/`), sempre por **sorteio ponderado** das dezenas livres:
  - Aleatória (Surpresinha): todas as livres com a mesma chance;
  - Mais sorteadas: mais chance para as mais frequentes na janela;
  - Menos sorteadas: mais chance para as menos frequentes na janela;
  - Mais atrasadas: mais chance para as há mais tempo sem sair (atraso sempre sobre todo o histórico);
  - Equilibrada: metade das livres pelas mais sorteadas, metade pelas mais atrasadas.
- **Geração**: jogos sem repetição dentro da mesma geração; quando as fixas e excluídas deixam menos combinações que
  a quantidade pedida, gera todas as possíveis e avisa. Recusa com mensagem quando há mais fixas que dezenas por
  jogo ou quando não sobram dezenas livres suficientes.
- **Jogos gerados**: lista com as dezenas e o perfil de cada jogo (pares/ímpares, soma, repetidas do último
  concurso), remover um jogo, "Gerar novamente" e "Salvar jogos".
- **Salvar jogos**: grava pelo serviço de jogos com as mesmas regras do cadastro manual (base vazia, concurso já
  sorteado, duplicado); duplicados são recusados e os demais gravados, com um resumo.
- **Painel**: atalho "Gerar jogos" no cabeçalho, ao lado de "Novo jogo".
- `CLAUDE.md`: as pendências da geração saem de "Pendências de negócio" e as decisões vão para "Decisões de negócio
  já definidas".
- Testes de unidade (domínio, aplicação, formatos) e de componente com bUnit.

## Capabilities

### New Capabilities
- `geracao-jogos`: página Gerar jogos — parâmetros, estratégias, fixas e excluídas, regras da geração, lista dos
  jogos gerados com perfil e gravação dos jogos.

### Modified Capabilities
- `painel`: o cabeçalho passa a ter o atalho "Gerar jogos" (antes era proibido nesta versão).
- `jogos`: o item "Meus jogos" do menu passa a ficar abaixo de "Gerar jogos" (antes, logo abaixo de "Estatísticas").

## Impact

- `Lotofacil.Dominio`: gerador de jogos (pesos por estratégia, sorteio ponderado sem repetição, combinações
  possíveis), estratégias e perfil do jogo; sem acesso a banco.
- `Lotofacil.Aplicacao`: serviço de geração (estatísticas da janela, último concurso, gerador) e cadastro de vários
  jogos no `JogosServico`, reaproveitando as regras do cadastro manual.
- `Lotofacil.Web`: página Gerar jogos, volante de fixas e excluídas, item do menu, atalho no Painel, formatos;
  registro do serviço na injeção de dependência.
- `Lotofacil.Infraestrutura`: sem mudança; sem migration.
- Sem pacotes novos.
- Fora do escopo: filtros de perfil na geração (fatia 10), estatísticas por perfil (fatia 9), evitar padrões
  populares, Teimosinha e Bolão.
