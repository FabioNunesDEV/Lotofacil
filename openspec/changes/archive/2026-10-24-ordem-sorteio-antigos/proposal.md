## Why

Os concursos importados pelo XLSX da Caixa (1 a 3808 na base atual) não têm a ordem do sorteio, porque a planilha só
traz as dezenas. Só os concursos gravados pela atualização mostram a ordem no detalhe da página Resultados. A API da
Caixa devolve a ordem também para os concursos antigos (conferido no 1, no 1500 e no 3000). É a fatia 14 do
`Docs/roteiro.md`.

## What Changes

- Botão "Preencher ordem do sorteio" na página Resultados, ao lado de "Atualizar resultados" e "Importar histórico",
  habilitado só quando há concursos sem ordem e nenhuma outra operação em andamento.
- Um clique busca na API, concurso a concurso e com a mesma pausa entre requisições, todos os concursos sem ordem,
  do menor para o maior. A ordem de cada um é gravada assim que obtida.
- Andamento "Buscando a ordem do sorteio do concurso <número> (<posição> de <total>)…" e um botão "Interromper". O
  que já foi gravado fica; o próximo clique busca só os que ainda estão sem ordem.
- Concurso que falhar (erro de comunicação, resposta sem ordem ou com dezenas diferentes das gravadas) é pulado, e a
  busca segue. No fim, a página informa quantos foram preenchidos e quantos ficaram sem ordem. Depois de 5 falhas
  seguidas (Caixa fora do ar, por exemplo), a busca é encerrada com aviso.
- Falhas registradas no log de erros.
- Só a ordem do sorteio é gravada; dezenas, rateio e demais dados dos concursos não mudam.

## Capabilities

### New Capabilities
- `ordem-sorteio`: preenchimento da ordem do sorteio dos concursos gravados sem ela, pela API da Caixa, com
  andamento, interrupção, falhas puladas e resumo.

### Modified Capabilities
<!-- Nenhuma: o detalhe do concurso já mostra a ordem quando conhecida, e a atualização já grava a ordem dos
     concursos novos. O bloqueio entre os botões da página Resultados fica descrito na capacidade nova. -->

## Impact

- Domínio: `Concurso` passa a aceitar a definição da ordem depois de criado (mesmas 15 dezenas);
  `IConcursoRepositorio` e `IFonteConcursos` ganham métodos para listar/contar os concursos sem ordem, gravar a
  ordem e buscar só a ordem na API.
- Aplicação: serviço novo de preenchimento da ordem, com andamento, cancelamento e resumo.
- Infraestrutura: leitura só do número, das dezenas e da ordem na resposta da API; consultas e gravação no
  repositório. Sem migration (a coluna já existe).
- Web: botão, andamento com "Interromper" e mensagens na página Resultados.
- Testes de unidade, bUnit e integração. Sem pacote novo.
- `Docs/roteiro.md` atualizado.
