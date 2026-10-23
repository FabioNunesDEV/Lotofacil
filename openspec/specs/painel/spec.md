# painel Specification

## Purpose

Reúne na página inicial (Painel) o resumo da base local e dos jogos do usuário: indicadores, último resultado,
destaques das dezenas e jogos do último concurso, com atalhos para atualizar os resultados e cadastrar um jogo.

## Requirements

### Requirement: Cabeçalho e atalhos do Painel
A página Painel SHALL mostrar o título "Painel", o subtítulo "Resumo da base local e dos seus jogos" e os botões
"Atualizar resultados", "Gerar jogos" e "Novo jogo". "Gerar jogos" MUST levar à página Gerar jogos. "Novo jogo"
MUST levar à página Meus jogos com o cadastro de jogo já aberto.

#### Scenario: Abrir o Painel
- **WHEN** o usuário abre a rota `/`
- **THEN** a página mostra o título "Painel", o subtítulo e os botões "Atualizar resultados", "Gerar jogos" e "Novo
  jogo"

#### Scenario: Atalho Gerar jogos
- **WHEN** o usuário aciona "Gerar jogos" no Painel
- **THEN** a página Gerar jogos é exibida

#### Scenario: Atalho Novo jogo
- **WHEN** o usuário aciona "Novo jogo" no Painel
- **THEN** a página Meus jogos é exibida com o cadastro de jogo aberto

### Requirement: Atualizar resultados pelo Painel
O botão "Atualizar resultados" do Painel SHALL iniciar a mesma atualização da base pela API da Caixa da página
Resultados, com o mesmo andamento ("Consultando o último concurso da Caixa…" e "Buscando o concurso <número>
(<posição> de <total>)…") e as mesmas mensagens de desfecho, inclusive as de base vazia, base já atualizada e
interrupção. Enquanto a atualização estiver em andamento, o botão MUST ficar desabilitado. Quando algum concurso for
gravado, o Painel MUST ser recarregado, passando a refletir o novo último concurso.

#### Scenario: Atualização com concursos novos
- **WHEN** o último concurso gravado é o 3808 e o usuário aciona "Atualizar resultados" no Painel com o 3809 já
  divulgado
- **THEN** o Painel exibe "Atualização concluída: 1 concurso novo gravado (3809)."
- **AND** o último resultado e o próximo concurso passam a ser 3809 e 3810

#### Scenario: Base já atualizada
- **WHEN** o usuário aciona "Atualizar resultados" no Painel e o 3808 é o último gravado e o último divulgado
- **THEN** o Painel exibe "A base local já está atualizada: último concurso 3808."

#### Scenario: Andamento
- **WHEN** a atualização acionada no Painel está buscando o concurso 3810, segundo de três
- **THEN** o Painel mostra "Buscando o concurso 3810 (2 de 3)…"
- **AND** o botão "Atualizar resultados" está desabilitado

### Requirement: Indicadores do Painel
Com concursos na base, o Painel SHALL mostrar quatro indicadores:
- "Concursos na base": quantidade de concursos gravados (ex.: "3.808") e "Último sorteio em <data do último
  concurso>";
- "Próximo concurso": número do último concurso gravado + 1, sem separador de milhar (ex.: "3809"), e
  "Estimativa <valor>" com a estimativa de prêmio gravada no último concurso, ou "Estimativa não informada" quando
  o valor gravado for zero;
- "Jogos aguardando": quantidade de jogos de concursos ainda não sorteados e, como detalhe, "Concurso <número>"
  quando todos forem do mesmo concurso, "<quantidade> concursos" quando forem de vários, ou "Nenhum jogo aguardando
  sorteio" quando não houver;
- "Prêmios a receber": soma dos prêmios dos jogos conferidos cujo prazo ainda não terminou — o prazo vai até a data
  do sorteio + 90 dias, inclusive — e "Prazo: 90 dias após o sorteio".

Valores em reais MUST usar o formato `R$ 1.234,56`. Os prêmios MUST ser os da conferência pelo rateio gravado de
cada concurso.

#### Scenario: Base com concursos e jogos
- **WHEN** a base tem 3.808 concursos, o último sorteado em 17/10/2026 com estimativa de R$ 1.800.000,00 para o
  próximo, e há 3 jogos para o concurso 3809
- **THEN** o Painel mostra "3.808" e "Último sorteio em 17/10/2026"
- **AND** mostra "3809" e "Estimativa R$ 1.800.000,00"
- **AND** mostra "3" jogos aguardando e "Concurso 3809"

#### Scenario: Jogos aguardando em vários concursos
- **WHEN** há 2 jogos para o concurso 3809 e 1 para o 3810, nenhum deles sorteado
- **THEN** o indicador "Jogos aguardando" mostra "3" e "2 concursos"

#### Scenario: Sem jogos aguardando
- **WHEN** todos os jogos cadastrados são de concursos já sorteados
- **THEN** o indicador "Jogos aguardando" mostra "0" e "Nenhum jogo aguardando sorteio"

#### Scenario: Estimativa não informada
- **WHEN** o último concurso gravado tem estimativa do próximo igual a zero
- **THEN** o indicador "Próximo concurso" mostra "Estimativa não informada"

#### Scenario: Prêmio dentro do prazo
- **WHEN** um jogo do concurso 3808, sorteado em 19/10/2026, ganhou R$ 35,00 e hoje é 17/01/2027
- **THEN** o indicador "Prêmios a receber" inclui os R$ 35,00

#### Scenario: Prêmio prescrito
- **WHEN** um jogo do concurso 3808, sorteado em 19/10/2026, ganhou R$ 35,00 e hoje é 18/01/2027
- **THEN** o indicador "Prêmios a receber" não inclui esse prêmio

### Requirement: Último resultado no Painel
Com concursos na base, o Painel SHALL mostrar o último concurso gravado com:
- o título "Último resultado · concurso <número>" e a data do sorteio com o dia da semana (ex.: "sábado,
  17/10/2026");
- os ganhadores de 15 acertos: "1 ganhador", "<quantidade> ganhadores" ou "Acumulou" quando não houve ganhador;
- as 15 dezenas sorteadas;
- o mesmo detalhe da página Resultados: o rateio das faixas de 15 a 11 acertos, com ganhadores e prêmio ("—" sem
  ganhador), a arrecadação ("Não informada" quando zero) e a ordem do sorteio, quando conhecida.

#### Scenario: Último resultado com ganhador
- **WHEN** o último concurso gravado é o 3808, sorteado num sábado, 17/10/2026, com 2 ganhadores de 15 acertos
- **THEN** o Painel mostra "Último resultado · concurso 3808", "sábado, 17/10/2026" e "2 ganhadores"
- **AND** mostra as 15 dezenas sorteadas e o rateio das cinco faixas

#### Scenario: Último resultado acumulado
- **WHEN** o último concurso gravado não teve ganhador de 15 acertos
- **THEN** o Painel mostra "Acumulou"

#### Scenario: Ordem do sorteio desconhecida
- **WHEN** o último concurso gravado não tem a ordem do sorteio
- **THEN** a ordem do sorteio não é exibida

### Requirement: Destaques das dezenas no Painel
Com concursos na base, o Painel SHALL mostrar os rankings "Mais sorteadas" e "Há mais tempo sem sair", com 5
dezenas cada, calculados sobre todo o histórico, com os mesmos valores, formatos, desempate e indicação de empatadas
dos rankings de mesmo nome da página Estatísticas. O cartão MUST ter um link "Estatísticas" para a página
Estatísticas.

#### Scenario: Rankings iguais aos das Estatísticas
- **WHEN** o usuário abre o Painel
- **THEN** os rankings "Mais sorteadas" e "Há mais tempo sem sair" mostram as mesmas dezenas e valores da página
  Estatísticas com a janela "Todos"

#### Scenario: Link para Estatísticas
- **WHEN** o usuário aciona o link "Estatísticas" do cartão de destaques
- **THEN** a página Estatísticas é exibida

### Requirement: Jogos do último concurso no Painel
Com concursos na base, o Painel SHALL mostrar o cartão "Seus jogos no concurso <número do último concurso
gravado>" com os jogos cadastrados para esse concurso, na ordem de cadastro, numerados "01", "02", ..., cada um com
as suas dezenas (acertadas em destaque), "<n> dezenas" quando tiver mais de 15, "<acertos> acertos" e o prêmio pelo
rateio gravado do concurso ("—" sem prêmio). Sem jogos nesse concurso, o cartão MUST mostrar "Nenhum jogo
cadastrado para o concurso <número>.". O cartão MUST ter um link "Ver todos" para a página Meus jogos.

#### Scenario: Jogos conferidos no último concurso
- **WHEN** o último concurso gravado é o 3808 e há 2 jogos para ele, um com 13 acertos e prêmio de R$ 35,00 e
  outro com 9 acertos
- **THEN** o cartão "Seus jogos no concurso 3808" mostra o jogo "01" com "13 acertos" e "R$ 35,00"
- **AND** o jogo "02" com "9 acertos" e "—"

#### Scenario: Nenhum jogo no último concurso
- **WHEN** o último concurso gravado é o 3808 e não há jogos para ele
- **THEN** o cartão mostra "Nenhum jogo cadastrado para o concurso 3808."

#### Scenario: Link Ver todos
- **WHEN** o usuário aciona o link "Ver todos"
- **THEN** a página Meus jogos é exibida

### Requirement: Painel com a base vazia
Sem concursos na base, o Painel SHALL mostrar, no lugar dos indicadores e cartões, a mensagem "Nenhum concurso
gravado na base local. Use “Importar histórico” na página Resultados para carregar os concursos da Caixa." com um
link para a página Resultados. Os botões "Atualizar resultados" e "Novo jogo" MUST continuar disponíveis.

#### Scenario: Base vazia
- **WHEN** o usuário abre o Painel sem concursos na base
- **THEN** a mensagem de base vazia é exibida com o link para Resultados
- **AND** os indicadores e os cartões não são exibidos
