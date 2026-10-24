## Why

O usuário repete os mesmos jogos em todo concurso e hoje precisa cadastrá-los de novo a cada sorteio. É a fatia 15
do `Docs/roteiro.md`. O Bolão, que estava junto nas pendências de negócio, foi descartado pelo usuário.

## What Changes

- **Teimosinha:** um grupo com nome, de um ou mais jogos de 15 a 20 dezenas, que vale para todo concurso a partir
  do informado até o usuário remover. Isso é diferente da Teimosinha da Caixa, que vale por um número fixo de
  concursos; aqui ela serve de controle do usuário.
- **Marcar Teimosinha** ao salvar um jogo no cadastro manual (Novo jogo) ou ao salvar a lista em Gerar jogos: switch
  "Teimosinha (repetir em todo concurso)" e o nome. Um nome novo cria a Teimosinha; o nome de uma ativa acrescenta os
  jogos a ela.
- **Jogos criados a cada concurso:** os jogos da Teimosinha viram jogos comuns em Meus jogos, para o concurso
  informado ao salvar e, a cada atualização ou importação dos resultados, para os concursos seguintes até o próximo.
  Concursos pulados também entram. São conferidos, entram no aviso de jogos premiados e na exportação como qualquer
  jogo. A criação também roda ao iniciar a aplicação.
- **Página "Teimosinhas"** (`/teimosinhas`), entre "Meus jogos" e "Configurações" no menu. Lista as ativas (nome,
  desde qual concurso, jogos, custo por concurso) e tem o botão "Remover", com confirmação. Remover para de criar os
  próximos e mantém os jogos já criados.
- **Meus jogos:** os jogos vindos de uma Teimosinha ganham a marca "Teimosinha · <nome>".
- `CLAUDE.md`: Teimosinha deixa de ser pendência (decisões registradas) e Bolão fica descartado.

## Capabilities

### New Capabilities
- `teimosinha`: registro, criação dos jogos a cada concurso, página Teimosinhas, remoção e marca em Meus jogos.

### Modified Capabilities
- `configuracoes`: "Acesso à página Configurações" deixa de ficar logo abaixo de "Meus jogos"; continua sendo o
  último item do menu, agora abaixo de "Teimosinhas".

## Impact

- Domínio: entidade `Teimosinha` com seus jogos; `Jogo` passa a ter a Teimosinha de origem (opcional);
  repositório novo.
- Aplicação: serviço das Teimosinhas (registrar, listar, remover, criar os jogos até o próximo concurso). O cadastro
  manual e o salvar da geração aceitam o nome da Teimosinha.
- Infraestrutura: tabelas `Teimosinha` e `TeimosinhaJogo`, coluna `TeimosinhaId` em `Jogo` e migration.
- Web: switch e nome no cadastro e em Gerar jogos, página Teimosinhas, item no menu, marca em Meus jogos e aviso dos
  jogos criados depois de atualizar (Resultados e Painel).
- Testes de unidade, bUnit e integração. Sem pacote novo.
- `CLAUDE.md` e `Docs/roteiro.md`.
