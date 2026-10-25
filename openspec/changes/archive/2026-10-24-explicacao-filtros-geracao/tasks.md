## 1. Domínio

- [x] 1.1 `DistribuicaoTeorica.FaixaMaisComum` e `LarguraFaixaSoma` pública (usada também por `EstatisticasPerfis`);
  verificar com testes de unidade: Pares 7–8, Primos 5–6, Múltiplos de 3 4–5, Fibonacci 4–5, Repetidas 8–9,
  Moldura 9–10, Soma 180–209, com os percentuais do cenário da spec; testes atuais das estatísticas passando

## 2. Web

- [x] 2.1 `FormatacaoGeracao.InstrucaoFiltros`, `ExplicacaoFiltro` e `MaisComum`; verificar com testes de unidade
  dos textos do cenário da spec (15 dezenas) e da Soma com 18 dezenas (de 171 a 297)
- [x] 2.2 `FiltrosPerfil.razor`: frase do topo e explicação em cada linha, com o "mais comum" só com 15 dezenas;
  verificar com testes bUnit: frase, as sete explicações com 15 dezenas, sem "mais comum" com 18 dezenas, contagem
  de filtros ligados igual sem limites, testes atuais de Gerar jogos passando

## 3. Verificação final

- [x] 3.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 3.2 Verificação manual numa cópia da base: textos com 15 e com 18 dezenas, layout em 390 px sem rolagem
  horizontal e no desktop
