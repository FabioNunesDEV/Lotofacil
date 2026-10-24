## 1. Domínio

- [x] 1.1 Criar `TipoPerfil` e `ValorPerfil.Calcular`; verificar com testes de unidade (01 a 15 com anterior 11 a 25:
  7 pares, 6 primos, 5 múltiplos, 6 de Fibonacci, soma 120, 5 repetidas, 9 na moldura; conjuntos com 12, 9, 8, 7 e
  16 dezenas)
- [x] 1.2 Criar `DistribuicaoTeorica` (hipergeométrica e soma por programação dinâmica); verificar com testes de
  unidade (7 pares 31,18%; 5 primos 30,87%; 9 repetidas 32,15%; soma total 3.268.760 combinações; 180–194 29,07% e
  195–209 29,67%; cada perfil somando 100%)
- [x] 1.3 Criar `FaixaPerfil`, `DistribuicaoPerfil` e `EstatisticasPerfis.Calcular` (faixas base, pontas agrupadas,
  observado na janela, repetidas sem o primeiro concurso, atraso sobre todo o histórico); verificar com testes de
  unidade dos cenários da spec (linhas de pares, soma e Fibonacci; teóricos agrupados 1,27%, 2,87%, 4,55%, 5,13%;
  observado 30,00% em 10 concursos; repetidas sobre 9 concursos; atraso "último", 42 com janela de 10 e nunca saiu)

## 2. Aplicação

- [x] 2.1 Adicionar `EstatisticasServico.CalcularPerfisAsync`; verificar com testes de unidade (base vazia devolve
  `null`, janela repassada, janela inválida rejeitada)

## 3. Web

- [x] 3.1 Criar `FormatacaoPerfil` (títulos, descrições, rótulos simples e agrupados, atraso); verificar com testes de
  unidade
- [x] 3.2 Criar o componente `CartaoPerfil` (tabela, barra com observado e marca do teórico); verificar com teste bUnit
  (linhas, percentuais, atraso, largura da barra e posição da marca)
- [x] 3.3 Colocar as abas "Por dezena" e "Por perfil do concurso" na página Estatísticas, com a janela valendo para as
  duas e mantendo a aba ao trocar a janela; verificar com testes bUnit dos cenários de "Abas da página Estatísticas" e
  "Perfis do concurso" e com os testes atuais da página passando

## 4. Verificação final

- [x] 4.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 4.2 Verificação manual numa base separada com o histórico real: abas, janelas, os 7 cartões, observado
  comparado com o Lotodicas (`Docs/lotofacil-analise-lotodicas-estatisticas.md`) e com um cálculo independente sobre o
  XLSX, teórico igual à spec, atraso das faixas do último concurso
- [x] 4.3 Atualizar `Docs/roteiro.md` (fatia 9 concluída, decisões registradas); verificar relendo o arquivo
