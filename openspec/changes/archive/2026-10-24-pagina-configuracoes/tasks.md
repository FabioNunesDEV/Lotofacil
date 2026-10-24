## 1. Domínio e infraestrutura

- [x] 1.1 Criar `ConfiguracaoApostas` (linha única, `PrecoApostaMinima`, `PrecoMinimo`/`PrecoMaximo`,
  `PrecoValido`, construtor que recusa preço inválido) e `IConfiguracaoApostasRepositorio`; verificar com testes de
  unidade: 0,01 e 100,00 aceitos; 0, −1, 100,01 e 3,555 recusados (`PrecoValido` falso e construtor lançando)
- [x] 1.2 Criar o mapeamento `ConfiguracaoApostasConfiguracao`, o `ConfiguracaoApostasRepositorio` (obter e salvar a
  linha única) e o registro em `AddInfraestrutura`; gerar a migration `CriacaoConfiguracaoApostas`; verificar com
  testes de integração em SQLite em memória: obter sem linha devolve `null`, salvar insere, salvar de novo atualiza
  sem criar segunda linha, e a migration aplica numa base existente com concursos e jogos

## 2. Aplicação

- [x] 2.1 Criar `ConfiguracoesServico` (preço padrão recebido no construtor, `PrecoApostaMinima`, `CarregarAsync`,
  `SalvarPrecoApostaMinimaAsync`); verificar com testes de unidade com repositório substituto: sem linha usa o padrão,
  carregar usa o gravado, salvar válido grava e muda o preço na hora, salvar inválido devolve `false` sem chamar o
  repositório

## 3. Web

- [x] 3.1 Trocar `OpcoesApostas` por `ConfiguracoesServico` em `CadastroJogo`, `GerarJogos` e `MeusJogos`, remover
  `OpcoesApostas` e registrar o serviço no `Program.cs` com o padrão de `Lotofacil:PrecoApostaMinima`, chamando
  `CarregarAsync` depois de inicializar a base; verificar com os testes bUnit atuais dessas páginas (registrando o
  serviço) e um teste novo de Meus jogos com preço de R$ 4,00 ("Investido" R$ 72,00) e do cadastro com custo R$ 4,00
- [x] 3.2 Criar a página `Configuracoes` (`/configuracoes`: título, subtítulo, "Salvar", campo do preço com
  validação e tabela de 15 a 20 dezenas) e o item "Configurações" no menu depois de "Meus jogos"; verificar com testes
  bUnit: campo com o preço atual ao abrir, tabela do cenário de R$ 3,50, tabela acompanhando 4,00 antes de salvar,
  "—" com o campo vazio, recusa com a mensagem sem chamar o salvar, salvar com "Configurações salvas." e o preço do
  serviço atualizado

## 4. Verificação final

- [x] 4.1 `dotnet build src/Lotofacil.slnx` sem avisos e `dotnet test --solution src/Lotofacil.slnx` passando
- [x] 4.2 Verificação manual numa cópia da base real: página pelo menu com 3,50 (sem linha gravada), recusas,
  salvar 4,00, custos novos em Meus jogos, cadastro, Gerar jogos e exportação, preço mantido após reiniciar, digitar
  sem salvar não muda os custos, layout em 390px
- [x] 4.3 Atualizar `CLAUDE.md` (preço editável na página Configurações, padrão no `appsettings.json`) e
  `Docs/roteiro.md` (fatia 13 concluída, decisões registradas); verificar relendo os arquivos
