## Why

A página Estatísticas mostra só a visão por dezena; a aba "Por perfil do concurso" do protótipo ficou fora da fatia
4. Os perfis (quantos pares, primos, a soma, quantas repetidas do anterior...) são a base dos filtros da fatia 10 e,
comparados com a distribuição teórica, mostram que os perfis mais frequentes saem mais porque existem mais
combinações com eles, não por tendência. É a fatia 9 do `Docs/roteiro.md`; as decisões pendentes (perfis, faixas de
soma, linhas e atraso) foram tomadas pelo usuário no início da fatia.

## What Changes

- A página Estatísticas passa a ter duas abas: **"Por dezena"** (o conteúdo atual, aberta ao entrar) e **"Por perfil
  do concurso"**. A janela da análise e o aviso fixo valem para as duas.
- A aba "Por perfil do concurso" mostra um cartão para cada um dos **7 perfis**:
  - Pares e ímpares (12 pares no volante);
  - Números primos (02 03 05 07 11 13 17 19 23);
  - Múltiplos de 3 (03 06 09 12 15 18 21 24);
  - Fibonacci (01 02 03 05 08 13 21);
  - Soma das dezenas, em **faixas de 15** (… 165–179, 180–194, 195–209 …);
  - Repetidas do concurso anterior;
  - Moldura e miolo (16 dezenas da borda do volante; miolo com as 9 do centro).
- Cada cartão tem uma linha por quantidade (ou faixa de soma), com as **pontas de chance teórica abaixo de 1%
  agrupadas** na linha vizinha ("≤ 4 pares", "≥ 10 pares"), e para cada linha:
  - barra comparando **observado** (na janela) e **teórico** (distribuição hipergeométrica; soma pela contagem das
    3.268.760 combinações);
  - percentuais observado e teórico;
  - **atraso** da faixa sobre todo o histórico ("último" quando saiu no último concurso).
- Testes de unidade (domínio, aplicação, formatos) e de componente com bUnit.

## Capabilities

### New Capabilities
<!-- Nenhuma: a aba faz parte da capacidade de estatísticas. -->

### Modified Capabilities
- `estatisticas`: a página ganha as abas "Por dezena" e "Por perfil do concurso", com os 7 perfis, observado ×
  teórico, pontas agrupadas e atraso por faixa.

## Impact

- `Lotofacil.Dominio`: cálculo dos perfis por concurso, das faixas com pontas agrupadas, da distribuição teórica e do
  atraso por faixa; sem acesso a banco.
- `Lotofacil.Aplicacao`: `EstatisticasServico` ganha o cálculo dos perfis na janela, sobre a mesma consulta leve dos
  resultados.
- `Lotofacil.Web`: abas na página Estatísticas, cartão de perfil e formatos.
- `Lotofacil.Infraestrutura`: sem mudança; sem migration. Sem pacotes novos.
- Fora do escopo: usar os perfis como filtro na geração (fatia 10), linhas/colunas do volante, sequências, ciclos e
  grupos de dezenas.
