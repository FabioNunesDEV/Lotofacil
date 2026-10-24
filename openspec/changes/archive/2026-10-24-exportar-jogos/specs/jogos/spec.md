## ADDED Requirements

### Requirement: Exportar os jogos de um concurso
Cada grupo de concurso da página Meus jogos SHALL ter, no cabeçalho, o botão "Exportar". Ele abre o diálogo
"Exportar · concurso <número>" com o texto dos jogos do concurso, somente leitura, e os botões "Copiar", "Baixar .txt"
e "Fechar". O texto MUST ter, nesta ordem e uma informação por linha:
1. `Lotofácil · concurso <número>`;
2. `<quantidade> · <custo>`: a quantidade como no grupo ("1 jogo" ou "<n> jogos") e o custo total dos jogos do
   concurso (soma das apostas de 15 de cada jogo × preço da aposta mínima), no formato `R$ 1.234,56`;
3. uma linha em branco;
4. um jogo por linha, na ordem de cadastro: `<número do jogo com dois dígitos>) ` seguido das dezenas em ordem
   crescente, com dois dígitos e separadas por um espaço; nas apostas de 16 a 20 dezenas, ` (<n> dezenas)` no final.

O texto MUST NOT trazer conferência (acertos, prêmio ou resultado), mesmo quando o concurso já foi sorteado, e MUST
conter todos os jogos do concurso. "Copiar" MUST copiar o texto para a área de transferência e exibir "Jogos
copiados."; se o navegador recusar a cópia, MUST exibir "Não foi possível copiar. Selecione o texto e copie
manualmente.". "Baixar .txt" MUST baixar o arquivo `concurso-<número>.txt` com o mesmo texto, em UTF-8. "Fechar"
fecha o diálogo.

#### Scenario: Texto dos jogos
- **WHEN** o concurso 3813 tem, nesta ordem de cadastro, os jogos 01 02 04 05 07 10 11 13 14 18 20 21 22 24 25,
  02 03 05 06 08 09 12 15 16 17 19 20 21 23 25 e um de 16 dezenas 01 03 04 06 07 08 10 11 12 14 15 17 18 22 24 25,
  com preço da aposta de R$ 3,50, e o usuário aciona "Exportar" no grupo do 3813
- **THEN** o diálogo "Exportar · concurso 3813" mostra o texto:
  ```
  Lotofácil · concurso 3813
  3 jogos · R$ 63,00

  01) 01 02 04 05 07 10 11 13 14 18 20 21 22 24 25
  02) 02 03 05 06 08 09 12 15 16 17 19 20 21 23 25
  03) 01 03 04 06 07 08 10 11 12 14 15 17 18 22 24 25 (16 dezenas)
  ```

#### Scenario: Um jogo
- **WHEN** o concurso 3814 tem um único jogo de 15 dezenas e o usuário o exporta
- **THEN** a segunda linha do texto é "1 jogo · R$ 3,50"

#### Scenario: Concurso já sorteado
- **WHEN** o usuário exporta os jogos do concurso 3808, já conferido, com um jogo premiado
- **THEN** o texto traz só o cabeçalho e as dezenas, sem acertos, prêmio nem resultado

#### Scenario: Copiar
- **WHEN** o usuário aciona "Copiar" no diálogo
- **THEN** o texto vai para a área de transferência e a página exibe "Jogos copiados."

#### Scenario: Cópia recusada
- **WHEN** o navegador recusa a cópia para a área de transferência
- **THEN** a página exibe "Não foi possível copiar. Selecione o texto e copie manualmente." e o diálogo continua aberto

#### Scenario: Baixar
- **WHEN** o usuário aciona "Baixar .txt" no diálogo do concurso 3813
- **THEN** o navegador baixa o arquivo "concurso-3813.txt" com o mesmo texto do diálogo

#### Scenario: Fechar
- **WHEN** o usuário aciona "Fechar"
- **THEN** o diálogo fecha e a lista continua como estava
