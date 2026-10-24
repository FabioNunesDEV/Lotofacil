## MODIFIED Requirements

### Requirement: Filtros e indicadores
A página SHALL oferecer os filtros "Todos" (padrão), "Aguardando sorteio", "Conferidos" e "Premiados" (grupos com
algum jogo premiado), e mostrar os indicadores, sempre sobre todos os jogos:
- "Jogos salvos": total de jogos e "1 concurso" ou "<quantidade> concursos";
- "Investido": soma dos custos de todos os jogos, e "1 aposta de 15 dezenas" ou "<quantidade> apostas de 15
  dezenas", em que a quantidade é a soma das apostas de 15 dezenas que cada jogo vale;
- "Premiados": quantidade de jogos com prêmio;
- "Total em prêmios": soma dos prêmios dos jogos conferidos.

O preço da aposta mínima MUST ser o preço atual da página Configurações (o gravado ou, sem preço gravado, o da
configuração da aplicação, R$ 3,50 por padrão), o mesmo para todos os jogos.

#### Scenario: Indicadores
- **WHEN** há 3 jogos de 15 dezenas aguardando o concurso 3810 e 2 jogos de 15 dezenas conferidos no 3808, um deles
  com R$ 7,00
- **THEN** os indicadores mostram 5 jogos salvos em 2 concursos, R$ 17,50 investido, 1 premiado e R$ 7,00 em prêmios

#### Scenario: Indicadores com aposta múltipla
- **WHEN** há 2 jogos de 15 dezenas e 1 jogo de 16 dezenas, com preço da aposta de R$ 3,50
- **THEN** "Jogos salvos" mostra 3 e "Investido" mostra "R$ 63,00" e "18 apostas de 15 dezenas"

#### Scenario: Investido com preço alterado
- **WHEN** há 2 jogos de 15 dezenas e 1 jogo de 16 dezenas e o preço salvo em Configurações é R$ 4,00
- **THEN** "Investido" mostra "R$ 72,00"

#### Scenario: Filtrar premiados
- **WHEN** o usuário escolhe "Premiados"
- **THEN** só os grupos com algum jogo premiado são exibidos
- **AND** os indicadores continuam considerando todos os jogos

#### Scenario: Filtrar aguardando
- **WHEN** o usuário escolhe "Aguardando sorteio"
- **THEN** só os grupos de concursos ainda não sorteados são exibidos
