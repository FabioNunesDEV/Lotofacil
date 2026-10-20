## 1. Domínio

- [x] 1.1 Adicionar `RegrasLotofacil.DezenasPorJogo` e `DiasPrescricao` e criar `Jogo` com as regras de validade;
  verificar com testes de unidade dos cenários de "Regras de um jogo" da spec
- [x] 1.2 Criar `ConferenciaJogo.Conferir`; verificar com testes de unidade de "Conferência do jogo" (faixa fixa e
  variável pelo rateio gravado, valor antigo de faixa fixa, menos de 11 acertos, concurso de outro número)
- [x] 1.3 Criar `IJogoRepositorio`; verificar com o build sem avisos

## 2. Infraestrutura

- [x] 2.1 Criar `JogoConfiguracao`, `DbSet<Jogo>` e `JogoRepositorio`, registrar em `AddInfraestrutura` e gerar a
  migration `CriacaoJogos`; verificar com testes de integração (gravar e listar na ordem de cadastro, existe,
  excluir, índice único rejeitando duplicado) e com o teste de migrations em dia passando

## 3. Serviço

- [x] 3.1 Criar `JogosServico` (`CadastrarAsync`, `ObterProximoConcursoAsync`, `ListarAsync`, `ExcluirAsync`),
  `ResultadoCadastro`, `GrupoJogos` e `JogoConferido`, e registrar no `Program.cs`; verificar com testes de unidade
  de cada cenário de "Cadastro de jogo" (mensagens), do agrupamento e ordem, da conferência por grupo, do prêmio
  total, da prescrição (19/10/2026 → 17/01/2027) e da exclusão

## 4. Página Meus jogos

- [x] 4.1 Adicionar a configuração `Lotofacil:PrecoApostaMinima` (3,50) e `OpcoesApostas`, `FormatacaoJogo` e o
  componente `Volante` (marcação com limite e modo de conferência); verificar com testes de unidade dos formatos e
  testes bUnit do volante
- [x] 4.2 Criar a página `/meus-jogos` (lista vazia, grupos, linhas, filtros, indicadores) e o item de menu "Meus
  jogos" abaixo de "Estatísticas"; verificar com testes bUnit de "Acesso à página Meus jogos", "Lista de jogos por
  concurso" e "Filtros e indicadores"
- [x] 4.3 Criar o diálogo `CadastroJogo` ligado ao botão "Novo jogo"; verificar com testes bUnit dos cenários de
  "Cadastro de jogo" (sugestão, contagem, limite, botão Salvar, mensagens de sucesso e de recusa)
- [x] 4.4 Criar o diálogo `ConferenciaJogo` e a exclusão com confirmação; verificar com testes bUnit de "Janela de
  conferência" e "Exclusão de jogo"

## 5. Verificação final

- [x] 5.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 5.2 Verificação manual com uma base separada com o histórico real: cadastrar jogos para o próximo concurso
  (inclusive tentativa duplicada e de concurso já sorteado), ver o grupo aguardando com custo; apagar os últimos
  concursos dessa base, cadastrar jogos para eles, atualizar os resultados e conferir acertos, prêmios pelo rateio,
  indicadores, filtros, janela de conferência e exclusão
- [x] 5.3 Registrar no `CLAUDE.md`, em "Decisões de negócio já definidas", as decisões desta fatia (15 dezenas,
  somente concursos futuros, duplicado rejeitado) e ajustar as pendências; verificar relendo as seções
