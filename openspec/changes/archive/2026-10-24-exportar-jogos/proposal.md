## Why

Os jogos ficam só na aplicação. Para levar à lotérica, ou guardar fora dela, o usuário precisa copiar as dezenas uma a
uma de Meus jogos. Esta é a fatia 12, segundo item da ordem de esforço combinada com o usuário (`Docs/roteiro.md`):
exportar como texto os jogos de um concurso.

## What Changes

- Em **Meus jogos**, cada concurso ganha o botão **"Exportar"** no cabeçalho, que abre o diálogo **"Exportar ·
  concurso <n>"** com:
  - o texto pronto, somente leitura:
    - `Lotofácil · concurso <n>`;
    - `<quantidade de jogos> · <custo total>` (o mesmo custo do grupo: apostas de 15 × preço da aposta mínima);
    - uma linha em branco;
    - um jogo por linha: `01) 01 02 04 …`, com dezenas de dois dígitos separadas por espaço e ` (16 dezenas)` nas
      apostas de 16 a 20;
  - **"Copiar"**: copia o texto para a área de transferência e avisa "Jogos copiados.", ou mostra a falha;
  - **"Baixar .txt"**: baixa o arquivo `concurso-<n>.txt` com o mesmo texto, em UTF-8;
  - **"Fechar"**.
- O texto não traz conferência, nem nos concursos já sorteados. A exportação fica só em Meus jogos, não em Gerar jogos.
- `Docs/roteiro.md`: fatia 12 concluída.
- Testes de unidade (formato do texto) e de componente com bUnit.

## Capabilities

### New Capabilities
<!-- nenhuma -->

### Modified Capabilities
- `jogos`: novo requisito "Exportar os jogos de um concurso" na página Meus jogos.

## Impact

- `Lotofacil.Web`: botão no cabeçalho dos grupos de Meus jogos, diálogo de exportação e classe de formatação do texto,
  do nome do arquivo e do endereço de download. A cópia usa a área de transferência do navegador por interop do
  Blazor, sem arquivo `.js` próprio.
- `Lotofacil.Dominio`, `Lotofacil.Aplicacao` e `Lotofacil.Infraestrutura`: sem mudança; sem migration.
- Sem pacotes novos.
- Fora do escopo: exportar a conferência, exportar vários concursos de uma vez, outros formatos (CSV, PDF), exportar
  em Gerar jogos.
