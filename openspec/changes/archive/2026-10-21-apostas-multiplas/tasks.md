## 1. Domínio

- [x] 1.1 Trocar `RegrasLotofacil.DezenasPorJogo` por `MinimoDezenasPorJogo` (15) e `MaximoDezenasPorJogo` (20) e
  criar `Combinatoria.Combinacoes`; verificar com testes de unidade (C(20, 15) = 15.504, C(16, 15) = 16, k > n e
  k < 0 dão 0, C(n, 0) = 1)
- [x] 1.2 Fazer `Jogo` aceitar de 15 a 20 dezenas e expor `QuantidadeApostas`; verificar com testes de unidade dos
  cenários de "Regras de um jogo" (14 e 21 rejeitados, 17 aceito valendo 136 apostas, ordem, repetida, fora do
  volante)
- [x] 1.3 Criar `PremioFaixa` e mudar `ConferenciaJogo` para prêmios por faixa; verificar com os testes atuais da
  conferência passando sem mudança de expectativa e com novos testes: 16 dezenas e 13 acertos (3 × 13 + 13 × 12 =
  R$ 287,00), 16 dezenas e 15 acertos (1 × 15 + 15 × 14), 20 dezenas e 11 acertos (126 × R$ 7,00 = R$ 882,00),
  10 acertos sem prêmio, faixas em ordem decrescente e só com quantidade > 0

## 2. Aplicação e Infraestrutura

- [x] 2.1 Adicionar `GrupoJogos.QuantidadeApostas`; verificar com teste de unidade (2 jogos de 15 + 1 de 16 = 18)
- [x] 2.2 Cobrir o cadastro de apostas múltiplas no serviço e no repositório, sem migration; verificar com testes de
  unidade de `JogosServico` (salvar 20 dezenas, duplicado de 16 dezenas recusado, jogo de 15 contido num de 16
  aceito) e de integração do repositório (gravar e ler 20 dezenas, índice único com 16 dezenas) e com o teste de
  migrations em dia passando

## 3. Web

- [x] 3.1 Adicionar a `FormatacaoJogo` os textos novos (contagem com custo, "<n> dezenas", "Aposta" do diálogo e
  prêmio por faixa); verificar com testes de unidade dos formatos
- [x] 3.2 Adicionar "Dezenas por jogo" ao `CadastroJogo`, ligado ao limite do volante, à contagem com custo e ao
  botão Salvar; verificar com testes bUnit dos cenários de "Cadastro de jogo" (padrão 15, 17 dezenas com
  "12 de 17 dezenas · R$ 476,00", reduzir de 17 para 15 mantendo as marcações e bloqueando novas, salvar 20 sem
  confirmação)
- [x] 3.3 Usar a quantidade de apostas no custo dos grupos e no indicador "Investido" e mostrar "<n> dezenas" na
  linha de jogos múltiplos em `MeusJogos`; verificar com testes bUnit de "Grupo aguardando com aposta múltipla" e
  "Indicadores com aposta múltipla" e com os testes atuais da página passando
- [x] 3.4 Mostrar "Aposta" e os prêmios por faixa no `DialogoConferencia`; verificar com testes bUnit de "Conferir
  um jogo premiado" (sem prêmios por faixa) e "Conferir uma aposta múltipla premiada"

## 4. Verificação final

- [x] 4.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 4.2 Verificação manual com uma base separada com o histórico real: cadastrar jogos de 15, 16 e 20 dezenas
  para o próximo concurso e conferir custo e investido; cadastrar jogos de 16 a 20 dezenas para concursos já gravados
  (apagando os últimos dessa base e atualizando) e comparar acertos e prêmios com o cálculo manual pela fórmula e o
  rateio do concurso; ver a lista com 20 bolas e a janela de conferência com os prêmios por faixa
- [x] 4.3 Atualizar o `CLAUDE.md`: remover a pendência "Apostas de 16 a 20 dezenas" e trocar, em "Decisões de
  negócio já definidas", "somente apostas de 15 dezenas" por apostas de 15 a 20 dezenas com custo C(n, 15) × preço e
  conferência da aposta múltipla; verificar relendo as seções
