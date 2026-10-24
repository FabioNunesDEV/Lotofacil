## Why

O preço da aposta mínima (R$ 3,50) só muda editando o `appsettings.json` e reiniciando a aplicação. Quando a Caixa
reajustar o preço, os custos do cadastro, da geração, de Meus jogos e da exportação ficam errados até alguém achar o
arquivo. É a fatia 13 do `Docs/roteiro.md`.

## What Changes

- Página "Configurações" (`/configuracoes`), último item do menu lateral, com o campo "Aposta mínima (15 dezenas)" e o
  botão "Salvar" ("Configurações salvas.").
- O preço salvo fica gravado na base e passa a valer na hora em todos os custos: cadastro de jogo, Gerar jogos, Meus
  jogos (custo do grupo e "Investido") e exportação. Preço único, sem vigência: jogos já cadastrados também passam a
  usar o preço novo.
- Enquanto a base não tiver preço gravado, vale o da configuração (`Lotofacil:PrecoApostaMinima`, R$ 3,50).
- Preço aceito de R$ 0,01 a R$ 100,00, com até dois decimais; fora disso o salvar é recusado com aviso.
- Tabela só de leitura com as apostas de 15 a 20 dezenas: apostas equivalentes (C(n, 15)) e valor, calculado pelo
  preço digitado, antes mesmo de salvar.
- Fora desta fatia: prêmios fixos de 11 a 13 acertos (nenhum cálculo os usa) e URL/pausa da API da Caixa (continuam
  no `appsettings.json`).

## Capabilities

### New Capabilities
- `configuracoes`: página Configurações, preço da aposta mínima gravado na base com o padrão da configuração,
  validação e tabela de preços de 15 a 20 dezenas.

### Modified Capabilities
- `jogos`: "Filtros e indicadores" passa a dizer que o preço é o atual da página Configurações (não mais só o da
  configuração).

## Impact

- Domínio: regra de validação do preço e interface do repositório da configuração.
- Aplicação: serviço das configurações, que lê o preço (gravado ou padrão) e salva o novo.
- Infraestrutura: tabela nova com a configuração, mapeamento, repositório e migration.
- Web: página Configurações, item no menu; cadastro, Gerar jogos e Meus jogos passam a ler o preço pelo serviço no
  lugar de `OpcoesApostas`, que deixa de existir.
- Testes de unidade, bUnit e integração (SQLite em memória). Sem pacote novo.
- `CLAUDE.md` e `Docs/roteiro.md` atualizados.
