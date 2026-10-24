## Purpose

Avisa, logo depois de "Atualizar resultados", o que os jogos cadastrados para os concursos recém-gravados renderam,
conferidos pelo rateio gravado de cada concurso, sem que o usuário precise procurar em Meus jogos.

## ADDED Requirements

### Requirement: Conferência após a atualização
Quando "Atualizar resultados" gravar algum concurso, seja na página Resultados ou no Painel, o sistema SHALL conferir
os jogos cadastrados para os concursos gravados **nessa atualização**: do último concurso gravado antes dela + 1 até
o último gravado ao final. Isso vale também quando a atualização foi interrompida depois de gravar algum concurso.
A conferência MUST seguir as regras de "Conferência do jogo" de Meus jogos (aposta múltipla, rateio gravado do
próprio concurso). Jogos de concursos que já estavam na base antes da atualização MUST NOT entrar. O desfecho atual
da atualização MUST continuar sendo exibido como hoje, antes do aviso. Nada novo é gravado.

O resultado da conferência decide o aviso:
- algum jogo premiado: o diálogo "Jogos premiados";
- jogos conferidos e nenhum premiado: o aviso "Seus <n> jogos dos concursos novos foram conferidos; nenhum foi
  premiado." ("Seu jogo dos concursos novos foi conferido e não foi premiado." com um jogo);
- nenhum jogo cadastrado para esses concursos: nenhum aviso além do desfecho.

#### Scenario: Atualização com jogo premiado
- **WHEN** o último concurso gravado é o 3808, há jogos cadastrados para o 3809, a atualização grava o 3809 e um dos
  jogos acerta 13 dezenas
- **THEN** a página exibe "Atualização concluída: 1 concurso novo gravado (3809)."
- **AND** abre o diálogo "Jogos premiados"

#### Scenario: Jogos sem prêmio
- **WHEN** a atualização grava os concursos 3809 e 3810, há 4 jogos cadastrados para eles e nenhum faz 11 acertos
- **THEN** a página exibe "Seus 4 jogos dos concursos novos foram conferidos; nenhum foi premiado."
- **AND** o diálogo "Jogos premiados" não é aberto

#### Scenario: Um jogo sem prêmio
- **WHEN** a atualização grava o concurso 3809 e há um único jogo cadastrado para ele, com 9 acertos
- **THEN** a página exibe "Seu jogo dos concursos novos foi conferido e não foi premiado."

#### Scenario: Sem jogos nos concursos novos
- **WHEN** a atualização grava o concurso 3809 e não há jogos cadastrados para ele
- **THEN** a página exibe só "Atualização concluída: 1 concurso novo gravado (3809)."

#### Scenario: Jogos de concursos antigos não entram
- **WHEN** há um jogo premiado do concurso 3808, que já estava na base, e a atualização grava o 3809, sem jogos
- **THEN** nenhum aviso de jogos é exibido

#### Scenario: Atualização interrompida depois de gravar
- **WHEN** a atualização grava o 3809, falha ao buscar o 3810 e há um jogo premiado do 3809
- **THEN** a página exibe a mensagem de interrupção
- **AND** abre o diálogo "Jogos premiados" com o jogo do 3809

#### Scenario: Pelo Painel
- **WHEN** o usuário aciona "Atualizar resultados" no Painel, a atualização grava o 3809 e há um jogo premiado dele
- **THEN** o Painel abre o diálogo "Jogos premiados"

#### Scenario: Nada gravado
- **WHEN** a base já está atualizada, ou está vazia, e o usuário aciona "Atualizar resultados"
- **THEN** nenhum jogo é conferido e nenhum aviso de jogos é exibido

### Requirement: Diálogo "Jogos premiados"
O diálogo SHALL ter o título "Jogos premiados" e mostrar, nesta ordem:
- o resumo "<premiados> de <conferidos> jogos conferidos foram premiados." ("1 de 1 jogo conferido foi premiado." e
  "1 de <conferidos> jogos conferidos foi premiado." no singular);
- para cada concurso com jogo premiado, em ordem crescente de número: "Concurso <número> · <data do sorteio>" e o
  prazo: "Receber até <data do sorteio + 90 dias>", ou "Prazo encerrado em <data>" quando hoje já passou dessa data;
- abaixo de cada concurso, os jogos premiados dele na ordem de cadastro: "Jogo <número no concurso, com dois
  dígitos, como em Meus jogos>", "<n> dezenas" quando o jogo tiver mais de 15, "<acertos> acertos" e o prêmio do
  jogo;
- o total: "Total: <premiados> jogos · <soma dos prêmios>" ("1 jogo" no singular).

Valores em reais no formato `R$ 1.234,56` e datas no formato `dd/MM/aaaa`. O diálogo MUST ter os botões "Ver em Meus
jogos", que fecha o diálogo e abre a página Meus jogos, e "Fechar".

#### Scenario: Dois concursos premiados
- **WHEN** a atualização grava o 3809 (sorteado em 20/10/2026) e o 3810 (21/10/2026), há 6 jogos para eles (5 do
  3809 e 1 do 3810), e só o jogo 02 do 3809 (13 acertos, R$ 35,00), o jogo 05 do 3809 (11 acertos, R$ 7,00) e o
  jogo 01 do 3810 (11 acertos, R$ 7,00) foram premiados
- **THEN** o diálogo mostra "3 de 6 jogos conferidos foram premiados."
- **AND** "Concurso 3809 · 20/10/2026" e "Receber até 18/01/2027", com "Jogo 02 · 13 acertos · R$ 35,00" e
  "Jogo 05 · 11 acertos · R$ 7,00"
- **AND** "Concurso 3810 · 21/10/2026" e "Receber até 19/01/2027", com "Jogo 01 · 11 acertos · R$ 7,00"
- **AND** "Total: 3 jogos · R$ 49,00"

#### Scenario: Aposta múltipla premiada
- **WHEN** o único jogo premiado é o jogo 01 do 3809, de 16 dezenas, com 13 acertos e prêmio de R$ 287,00
- **THEN** o diálogo mostra "Jogo 01 · 16 dezenas · 13 acertos · R$ 287,00" e "Total: 1 jogo · R$ 287,00"

#### Scenario: Concurso sem prêmio fica de fora
- **WHEN** a atualização grava o 3809 e o 3810 e só os jogos do 3810 foram premiados
- **THEN** o diálogo não mostra o concurso 3809

#### Scenario: Prazo encerrado
- **WHEN** hoje é 20/01/2027 e um jogo premiado é de um concurso sorteado em 20/10/2026
- **THEN** o concurso mostra "Prazo encerrado em 18/01/2027"

#### Scenario: Ver em Meus jogos
- **WHEN** o usuário aciona "Ver em Meus jogos" no diálogo
- **THEN** o diálogo fecha e a página Meus jogos é exibida

#### Scenario: Fechar
- **WHEN** o usuário aciona "Fechar"
- **THEN** o diálogo fecha e a página continua onde estava
