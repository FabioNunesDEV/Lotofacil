## Why

A página inicial (`/`, "Painel") mostra hoje apenas o título. Para ver o último resultado, os jogos à espera do
sorteio ou os destaques das dezenas, o usuário precisa abrir três páginas diferentes. Com as fatias de resultados,
estatísticas e jogos prontas, o Painel pode reunir esse resumo, como no protótipo. É a fatia 7 do `Docs/roteiro.md`
e não depende de definição de negócio pendente.

## What Changes

- Cabeçalho do Painel com o subtítulo "Resumo da base local e dos seus jogos" e dois botões:
  - **"Atualizar resultados"**: mesma atualização pela API da Caixa da página Resultados, com o mesmo andamento e
    as mesmas mensagens; ao gravar concursos novos, o Painel é recarregado;
  - **"Novo jogo"**: leva à página Meus jogos com o cadastro já aberto. O atalho "Gerar jogos" fica para a fatia 8.
- **Indicadores**:
  - "Concursos na base", com a data do último sorteio gravado;
  - "Próximo concurso" (último gravado + 1), com a estimativa de prêmio gravada no último concurso;
  - "Jogos aguardando", com o concurso (ou a quantidade de concursos) dos jogos ainda não sorteados;
  - "Prêmios a receber": soma dos prêmios dos jogos ainda dentro do prazo de 90 dias após o sorteio.
- **Último resultado**: número, data com o dia da semana, ganhadores de 15 acertos (ou "Acumulou"), dezenas
  sorteadas e o mesmo detalhe da página Resultados (rateio das faixas, arrecadação e ordem do sorteio, quando
  conhecida).
- **Destaques**: rankings "Mais sorteadas" e "Há mais tempo sem sair" (5 dezenas, todo o histórico), iguais aos da
  página Estatísticas, com link para ela.
- **Seus jogos no último concurso**: os jogos do último concurso gravado, conferidos pelo rateio gravado (acertos e
  prêmio), ou um aviso de que não há jogos nesse concurso; link "Ver todos" para Meus jogos.
- **Base vazia**: no lugar dos indicadores e cartões, mensagem pedindo a importação do histórico na página
  Resultados.
- Página Meus jogos: aberta com `?novo=true`, abre o cadastro de jogo ao carregar.
- Testes de unidade (serviço do resumo, formatos) e de componente com bUnit.

## Capabilities

### New Capabilities
- `painel`: conteúdo da página Painel — indicadores, último resultado, destaques das dezenas, jogos do último
  concurso, atalhos "Atualizar resultados" e "Novo jogo" e o aviso de base vazia.

### Modified Capabilities
- `aplicacao-web`: o requisito "Página inicial" deixa de dizer que o Painel contém apenas o título.
- `jogos`: a página Meus jogos passa a abrir o cadastro quando acessada com `?novo=true` (destino do atalho do
  Painel).

## Impact

- `Lotofacil.Aplicacao`: novo serviço do resumo do Painel, que usa os repositórios de concursos e o
  `JogosServico`/`EstatisticasServico` já existentes; regra "dentro do prazo" no grupo de jogos.
- `Lotofacil.Web`: página `Painel` preenchida; textos do desfecho da atualização extraídos da página Resultados para
  serem usados também pelo Painel (sem mudar as mensagens); `MeusJogos` lê o parâmetro `novo`; registro do novo
  serviço na injeção de dependência.
- `Lotofacil.Dominio` e `Lotofacil.Infraestrutura`: sem mudança; sem migration.
- Sem pacotes novos.
- Fora do escopo: atalho "Gerar jogos" (fatia 8), data do próximo sorteio (não é gravada), aviso de jogos premiados
  após atualizar, marcar prêmio como recebido.
