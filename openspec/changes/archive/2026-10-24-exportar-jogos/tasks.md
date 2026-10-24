## 1. Web

- [x] 1.1 Criar `FormatacaoExportacao` (texto com cabeçalho, quantidade e custo, linha em branco e um jogo por linha;
  nome do arquivo; endereço de download em data URL UTF-8); verificar com testes de unidade: texto do cenário da spec
  (3 jogos, R$ 63,00, aposta de 16 dezenas), "1 jogo · R$ 3,50", sem conferência num concurso sorteado, linhas
  separadas por `\r\n`, "concurso-3813.txt" e data URL que decodifica para o texto
- [x] 1.2 Criar o componente `DialogoExportacao` (texto somente leitura, "Copiar" pela área de transferência com aviso
  de sucesso ou falha, "Baixar .txt" com `download`, "Fechar"); verificar com testes bUnit: texto exibido, chamada a
  `navigator.clipboard.writeText` com o texto e "Jogos copiados.", falha da cópia com a mensagem e o diálogo aberto,
  link de download com o nome e o endereço, fechar
- [x] 1.3 Botão "Exportar" no cabeçalho de cada grupo de Meus jogos, abrindo o diálogo "Exportar · concurso <n>";
  verificar com testes bUnit da página: botão em cada grupo, diálogo com o título e o texto do concurso certo (custo
  pelo preço configurado) e testes atuais da página passando

## 2. Verificação final

- [x] 2.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 2.2 Verificação manual numa cópia da base com jogos de 15 e 16 dezenas, aguardando e conferidos: texto do
  diálogo, "Copiar" (conferir a área de transferência), "Baixar .txt" (conferir o arquivo e os acentos no Bloco de
  Notas) e layout em 390px
- [x] 2.3 Atualizar `Docs/roteiro.md` (fatia 12 concluída, decisões registradas); verificar relendo o arquivo
