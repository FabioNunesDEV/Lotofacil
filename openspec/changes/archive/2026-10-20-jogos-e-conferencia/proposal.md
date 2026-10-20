## Why

O sistema já mantém os resultados em dia e mostra estatísticas, mas o usuário ainda não consegue registrar as
apostas que pretende fazer nem saber, depois do sorteio, quanto acertou e quanto ganhou. Esta fatia fecha o ciclo
"apostar → sortear → conferir" e cria o lugar onde a geração de jogos (fatia futura) vai salvar os jogos gerados.

## What Changes

- Nova página **Meus jogos** (`/meus-jogos`), com item no menu logo abaixo de "Estatísticas", seguindo o protótipo.
- **Cadastro manual de jogo**: escolher o concurso (o próximo já vem sugerido) e marcar exatamente **15 dezenas**
  num volante 5 × 5. Só concursos **futuros** — número maior que o do último concurso gravado. Com a base vazia, o
  cadastro pede a importação do histórico antes.
- Um jogo com as mesmas dezenas já cadastrado para o mesmo concurso é **rejeitado com aviso**.
- **Conferência automática**: quando o concurso do jogo está na base, o jogo mostra os acertos e o prêmio da faixa
  (11 a 15), sempre pelo **rateio gravado do próprio concurso**. Concursos ainda não sorteados aparecem como
  "Aguardando sorteio" e passam a conferidos assim que a base é atualizada.
- Jogos agrupados por concurso, do mais recente ao mais antigo, com situação, quantidade, custo (aguardando) ou
  prêmio total (conferidos), resultado oficial e data de prescrição (90 dias após o sorteio).
- Filtros "Todos", "Aguardando sorteio", "Conferidos" e "Premiados" e indicadores de jogos salvos, valor investido,
  jogos premiados e total em prêmios.
- Janela de conferência de um jogo: volante com acertos, dezenas marcadas que não saíram e sorteadas não marcadas,
  e os detalhes (data, acertos, faixa, prêmio, prescrição).
- Excluir um jogo, com confirmação.
- Preço da aposta mínima (R$ 3,50) em configuração, usado no custo e no valor investido.
- Testes de unidade (regras do jogo e da conferência, serviço, página com bUnit) e de integração (repositório,
  migration).

## Capabilities

### New Capabilities
- `jogos`: cadastro, listagem, exclusão e conferência dos jogos do usuário (15 dezenas, concursos futuros, sem
  duplicados), com prêmio pelo rateio gravado do concurso.

### Modified Capabilities
<!-- Nenhuma: o novo item de menu segue a regra existente de listar apenas páginas disponíveis. -->

## Impact

- `Lotofacil.Dominio`: entidade `Jogo`, conferência de um jogo contra um concurso, interface do repositório de jogos.
- `Lotofacil.Aplicacao`: serviço de jogos (cadastro com as regras, listagem conferida, exclusão, resumo).
- `Lotofacil.Infraestrutura`: tabela `Jogo` (nova migration), repositório e registro na injeção de dependência.
- `Lotofacil.Web`: página Meus jogos, diálogo de cadastro com volante, diálogo de conferência, item de menu, nova
  configuração `Lotofacil:PrecoApostaMinima`.
- Sem pacotes novos.
- Resolve, para esta fatia, as pendências do `CLAUDE.md` sobre quantidade de dezenas por jogo e jogo para concurso
  que já existe na base.
- Fora do escopo: geração de jogos e dezenas fixas, apostas de 16 a 20 dezenas, Teimosinha e Bolão, edição de jogo,
  resumo dos jogos no Painel, aviso de premiação na atualização dos resultados.
