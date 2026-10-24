## Why

Hoje, depois de "Atualizar resultados", o usuário só descobre se algum jogo foi premiado abrindo Meus jogos e
procurando os concursos novos. A conferência já é calculada pelo rateio gravado de cada concurso; falta avisar, logo
após a atualização, o que os concursos recém-gravados renderam. Esta é a fatia 11, o primeiro item de "Depois" do
`Docs/roteiro.md` na ordem de esforço combinada com o usuário.

## What Changes

- Depois de uma atualização que gravou concursos (inclusive quando ela foi interrompida depois de gravar alguns), as
  páginas **Resultados** e **Painel** conferem os jogos cadastrados para os concursos gravados **nessa atualização**:
  - se algum foi premiado, abrem o diálogo **"Jogos premiados"**: por concurso, a data do sorteio e o prazo para
    receber (sorteio + 90 dias), e cada jogo premiado com o número dele no concurso (como em Meus jogos), as
    dezenas a mais quando tiver mais de 15, os acertos e o prêmio. Também mostram quantos dos jogos conferidos foram
    premiados e o total ("<n> jogos · R$ x"), além dos botões **"Ver em Meus jogos"** e **"Fechar"**;
  - se havia jogos mas nenhum foi premiado, exibem o aviso "Seus <n> jogos dos concursos novos foram conferidos;
    nenhum foi premiado." (com singular);
  - sem jogos nesses concursos, nada além do desfecho atual da atualização.
- O desfecho da atualização (mensagens atuais) continua igual e aparece antes do diálogo ou do aviso.
- A conferência continua calculada na leitura, pelas regras existentes (aposta múltipla, rateio do próprio
  concurso). Nada novo é gravado no banco.
- `Docs/roteiro.md`: fatia 11 concluída e plano das fatias 12 a 15 (exportar jogos, Configurações, ordem do sorteio
  dos concursos antigos, Teimosinha e Bolão).
- Testes de unidade (aplicação, formatos) e de componente com bUnit.

## Capabilities

### New Capabilities
- `aviso-jogos-premiados`: conferência dos jogos dos concursos gravados por uma atualização e o aviso do resultado
  (diálogo "Jogos premiados" ou aviso de nenhum prêmio) nas páginas Resultados e Painel.

### Modified Capabilities
<!-- nenhuma: os desfechos da atualização em `resultados` e `painel` não mudam; o aviso é um passo a mais, descrito
     na capacidade nova -->

## Impact

- `Lotofacil.Aplicacao`: `JogosServico` ganha a consulta dos jogos conferidos de um intervalo de concursos,
  reaproveitando o agrupamento e a conferência de `ListarAsync`.
- `Lotofacil.Web`: componente do diálogo "Jogos premiados", formatos dos textos e chamada após a atualização nas
  páginas Resultados e Painel.
- `Lotofacil.Dominio` e `Lotofacil.Infraestrutura`: sem mudança; sem migration.
- Sem pacotes novos.
- Fora do escopo: avisar prêmios de concursos trazidos pela importação do histórico, guardar quais prêmios já foram
  avisados, notificações fora da aplicação.
