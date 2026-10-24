## 1. Domínio e infraestrutura

- [x] 1.1 Extrair `Jogo.OrdenarEValidar`, acrescentar `Jogo.TeimosinhaId` e criar `Teimosinha`/`JogoTeimosinha` e
  `ITeimosinhaRepositorio`; verificar com testes de unidade: nome aparado e de 1 a 40 caracteres, comparação sem
  maiúsculas, dezenas repetidas recusadas, `JogosAte` pelo início, `Remover` desativa, testes atuais do `Jogo`
  passando
- [x] 1.2 Mapeamentos (`Teimosinha` com `TeimosinhaJogo`, `Jogo.TeimosinhaId`), `TeimosinhaRepositorio`, registro em
  `AddInfraestrutura` e migration `CriacaoTeimosinha`; verificar com testes de integração em SQLite em memória:
  salvar e ler com os jogos, ativas por nome, nomes incluindo removidas, jogo gravado com `TeimosinhaId`, migration
  aplicada numa base com concursos e jogos

## 2. Aplicação

- [x] 2.1 `JogosServico`: `VerificarConcursoAsync` público e `CadastrarVariosAsync` com `teimosinhaId` opcional;
  criar `TeimosinhaServico.SalvarAsync` e registrar no `Program.cs`; verificar com testes de unidade: Teimosinha nova
  com o jogo e início, nome existente acrescenta, jogo já na Teimosinha recusado, duplicado no concurso recusado,
  concurso passado recusa sem gravar a Teimosinha
- [x] 2.2 `TeimosinhaServico.CriarJogosAsync`, `ListarAtivasAsync`, `RemoverAsync` e `ListarNomesAsync`; chamada na
  inicialização; verificar com testes de unidade: próximo concurso, concursos pulados, duplicado pulado, jogo com
  início posterior, removida não cria, idempotência (segunda chamada não cria nada) e resultado com quantidade e
  faixa de concursos

## 3. Web

- [x] 3.1 `FormatacaoTeimosinha` e o componente `CampoTeimosinha` (switch e nome com sugestões); verificar com testes
  de unidade dos textos e bUnit do componente (desligado ao abrir, nome aparece ao ligar, sugestões das ativas)
- [x] 3.2 Cadastro manual e Gerar jogos com o campo; verificar com testes bUnit: salvar como Teimosinha com a
  mensagem, nome obrigatório, recusa sem mudar a Teimosinha, Gerar jogos salvando na Teimosinha, testes atuais
  passando
- [x] 3.3 Página Teimosinhas (lista, custo, remover com confirmação) e item no menu; verificar com testes bUnit: lista
  do cenário da spec (R$ 59,50), nenhuma ativa, remover e cancelar, ordem do menu com "Configurações" por último
- [x] 3.4 Marca em Meus jogos e criação depois de atualizar/importar em Resultados e Painel (mensagem e ordem antes
  do aviso); verificar com testes bUnit: marca só nos jogos da Teimosinha (também removida), mensagem de um e de
  vários concursos, jogo criado para concurso sorteado entrando no aviso

## 4. Verificação final

- [x] 4.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 4.2 Verificação manual numa cópia da base: Teimosinha pelo cadastro e por Gerar jogos, página Teimosinhas,
  atualização com a API real criando os jogos do próximo concurso (base com o último concurso removido da cópia para
  ter o que atualizar), marca em Meus jogos, remover, reiniciar sem duplicar, layout em 390 px
- [x] 4.3 Atualizar `CLAUDE.md` (Teimosinha decidida, Bolão descartado, sem pendências de negócio) e
  `Docs/roteiro.md` (fatia 15 concluída); verificar relendo os arquivos
