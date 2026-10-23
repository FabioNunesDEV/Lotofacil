## 1. Domínio

- [x] 1.1 Criar `EstrategiaGeracao` e `PesosDezenas.Calcular`; verificar com testes de unidade (Mais sorteadas: única
  maior com peso 25 e única menor com peso 1; Menos sorteadas invertida; Mais atrasadas com atraso `null` como maior;
  empates com o mesmo peso; Aleatória com peso 1 para todas)
- [x] 1.2 Criar `ParametrosGeracao`, `ResultadoGeracao` e `GeradorJogos` (sorteio ponderado sem reposição, jogos
  distintos, enumeração quando possíveis ≤ quantidade, recusas); verificar com testes de unidade: tamanho e dezenas
  válidas, fixas presentes e excluídas ausentes em várias gerações, jogos distintos, jogo inteiro fixo (1 possível),
  13 fixas e 8 excluídas (6 possíveis), recusa de fixas demais e de livres insuficientes com as mensagens da spec,
  Equilibrada com 3 fixas (6 + 6), dezena de peso muito maior escolhida com mais frequência num `Random` de semente
  fixa, parâmetros fora de faixa rejeitados
- [x] 1.3 Criar `PerfilJogo.Calcular`; verificar com teste de unidade (01 a 15 contra o 3810: 7 pares, 8 ímpares,
  soma 120, 8 repetidas)

## 2. Aplicação

- [x] 2.1 Adicionar `JogosServico.CadastrarVariosAsync` e `ResultadoCadastroVarios`, extraindo as checagens de base
  vazia e concurso já sorteado; verificar com testes de unidade (todos gravados; parte duplicada; todos duplicados;
  base vazia e concurso já sorteado recusam tudo sem gravar) e com os testes atuais de `CadastrarAsync` passando
- [x] 2.2 Criar `GeracaoJogosServico` (contexto, gerar com a janela, salvar) e registrar com `Random.Shared` em
  `Program.cs`; verificar com testes de unidade (contexto nulo com base vazia; janela repassada às estatísticas;
  perfil calculado contra o último concurso; salvar delega ao `JogosServico`)

## 3. Web

- [x] 3.1 Criar `FormatacaoGeracao` (custo, subtítulo, perfil, aviso de combinações, mensagens do salvamento) e as
  descrições das estratégias; verificar com testes de unidade dos textos da spec, inclusive singular e plural
- [x] 3.2 Criar o componente `VolanteGeracao` (livre → fixa → excluída → livre); verificar com testes bUnit (três
  cliques na 07, classes e estado repassados por callback)
- [x] 3.3 Criar a página `GerarJogos` com formulário, aviso fixo, base vazia, contagem de fixas/excluídas e "Limpar",
  custo, geração, avisos e recusas; verificar com testes bUnit dos cenários de "Acesso à página Gerar jogos",
  "Parâmetros da geração", "Dezenas fixas e excluídas" e "Regras da geração"
- [x] 3.4 Lista "Jogos gerados" com perfil, remover, "Gerar novamente" e "Salvar jogos"; verificar com testes bUnit
  dos cenários de "Lista dos jogos gerados" e "Salvar os jogos gerados" (inclusive os jogos aparecendo em Meus jogos
  pelo mesmo repositório falso)
- [x] 3.5 Adicionar o item "Gerar jogos" ao menu e o atalho no Painel; verificar com testes bUnit do menu (ordem dos
  itens) e do cabeçalho do Painel (link `gerar-jogos`)

## 4. Verificação final

- [x] 4.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 4.2 Verificação manual numa base separada com o histórico real: gerar com cada estratégia e janela, conferir
  fixas e excluídas, os avisos de poucas combinações e as recusas, comparar a tendência de "Mais sorteadas" e "Mais
  atrasadas" com os rankings das Estatísticas, remover e gerar novamente, salvar (com e sem duplicados) e ver os
  jogos em Meus jogos e no Painel
- [x] 4.3 Atualizar `CLAUDE.md` (pendência da geração removida, decisões registradas) e `Docs/roteiro.md` (fatia 8
  concluída); verificar relendo as seções
