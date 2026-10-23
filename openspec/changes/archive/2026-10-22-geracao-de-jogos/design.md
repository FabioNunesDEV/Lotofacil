## Context

Motivação em `proposal.md`; comportamento em `specs/geracao-jogos/spec.md`, `specs/painel/spec.md` e
`specs/jogos/spec.md`. Estado atual relevante:

- `EstatisticasServico.CalcularAsync(janela)` devolve `EstatisticasDezenas` com, por dezena, `Sorteios` na janela e
  `Atraso` sobre todo o histórico (`null` quando nunca saiu), ou `null` com a base vazia.
- `JogosServico.CadastrarAsync(concurso, dezenas)` aplica as regras do cadastro (base vazia, concurso já sorteado,
  duplicado) e devolve `ResultadoCadastro(Sucesso, Mensagem)`; não há cadastro de vários jogos.
- `Combinatoria.Combinacoes` e `Jogo.CalcularQuantidadeApostas` já existem; C(25, k) cabe em `int`.
- `OpcoesApostas.PrecoApostaMinima` está na Web, como no custo de Meus jogos.
- O `Volante` atual é de dois estados (marcada ou não); `BolasJogo` mostra as dezenas de um jogo.

## Goals / Non-Goals

**Goals:**
- Regra da geração no domínio, determinística com um `Random` fornecido, testável sem banco e sem tela.
- Gravação dos jogos gerados pelas mesmas regras do cadastro manual, num lugar só.
- Página no padrão das demais (MudBlazor, code-behind, formatos em classe estática testável).

**Non-Goals:**
- Garantir que a distribuição dos jogos "acerte mais": a página só aplica o critério escolhido.
- Filtros de perfil (fatia 10), evitar padrões populares, guardar os parâmetros da geração.

## Decisions

### Domínio (`Lotofacil.Dominio/Geracao`)
- `EstrategiaGeracao` (enum): `Aleatoria`, `MaisSorteadas`, `MenosSorteadas`, `MaisAtrasadas`, `Equilibrada`.
- `PesosDezenas` (estática): `Calcular(estatisticas, criterio)` devolve o peso de cada dezena de 1 a 25:
  `1 + quantidade de dezenas com valor pior` (menos sorteios, mais sorteios ou atraso menor, conforme o critério).
  Empates recebem o mesmo peso. Atraso `null` conta como o maior.
  - Alternativa: peso igual ao próprio valor (sorteios ou atraso). Descartada: as frequências históricas ficam todas
    perto de 60%, o que deixaria "Mais sorteadas" quase igual a "Aleatória", e atraso 0 zeraria a chance das 15
    dezenas do último concurso. O peso pela posição dá efeito visível e trata frequência e atraso do mesmo jeito.
- `GeradorJogos` (classe com `Random` no construtor) com `Gerar(ParametrosGeracao, EstatisticasDezenas)`, que
  devolve `ResultadoGeracao(IReadOnlyList<IReadOnlyList<int>> Jogos, int? CombinacoesPossiveis, string? Recusa)`:
  - `ParametrosGeracao(Quantidade, DezenasPorJogo, Estrategia, Fixas, Excluidas)` valida faixas (1–100, 15–20,
    dezenas de 1 a 25, fixas e excluídas sem interseção) com `ArgumentException`; as recusas de negócio (fixas demais,
    livres insuficientes) saem como `ResultadoGeracao` com `Recusa` (mensagem), para a página mostrar.
  - possíveis = C(livres, n − fixas). Se possíveis ≤ quantidade, enumera todas as combinações das livres (em ordem);
    com no máximo 100 jogos, a enumeração é pequena. `CombinacoesPossiveis` só vem preenchido quando possíveis <
    quantidade, para o aviso.
  - Senão, sorteia jogos com sorteio ponderado sem reposição (peso total, número aleatório, escolhe e retira) e
    descarta repetidos (conjunto de jogos já gerados) até chegar à quantidade. Como possíveis > quantidade, sempre
    termina; um limite de tentativas (quantidade × 1.000) protege contra pesos muito concentrados e, se atingido,
    devolve o que gerou (não esperado na prática; coberto por teste com `Random` controlado).
  - Equilibrada: metade arredondada para cima das dezenas a sortear com os pesos de `MaisSorteadas`, o restante com
    os de `MaisAtrasadas`, entre as livres ainda não escolhidas.
- `PerfilJogo` (record) com `Calcular(dezenas, ultimoResultado)`: `Pares`, `Impares`, `Soma`, `Repetidas`.

### Aplicação (`Lotofacil.Aplicacao/Geracao`)
- `GeracaoJogosServico`, com `IConcursoRepositorio`, `EstatisticasServico`, `JogosServico` e `Random`:
  - `ObterContextoAsync()`: último concurso gravado (número e dezenas) e total de concursos, ou `null` com a base
    vazia; a página usa para o concurso sugerido, o "Todos (<total>)" e o perfil;
  - `GerarAsync(parametros, janela)`: calcula as estatísticas da janela e chama o `GeradorJogos`; devolve os jogos
    com o `PerfilJogo` de cada um em relação ao último concurso;
  - `SalvarAsync(concurso, jogos)`: delega a `JogosServico.CadastrarVariosAsync`.
- `JogosServico.CadastrarVariosAsync(concurso, jogos)` devolve `ResultadoCadastroVarios(Gravados, Duplicados,
  Recusa)`: as checagens de base vazia e concurso já sorteado (as mesmas mensagens) são extraídas de
  `CadastrarAsync` para um método privado usado pelos dois; depois, cada jogo é recusado se duplicado ou gravado.
  As mensagens finais ("<n> jogos salvos ...") ficam na Web, em `FormatacaoGeracao`.
  - Alternativa: a página chamar `CadastrarAsync` jogo a jogo. Descartada: a página teria de distinguir pela
    mensagem uma recusa de concurso de uma de duplicado.
- `Random` registrado como singleton com `Random.Shared` em `Program.cs`; os testes passam um `Random` com semente.

### Web
- Página `GerarJogos` (`/gerar-jogos`), com `GerarJogos.razor`, `.razor.cs` e `.razor.css`: duas colunas como no
  protótipo (formulário à esquerda, "Jogos gerados" à direita), `MudAlert` com o aviso fixo, mensagem de base vazia.
- Componente `VolanteGeracao` (`Components/Geracao`): 25 botões com três estados, parâmetros `Fixas`/`Excluidas`
  com `EventCallback`s, classes `fixa` e `excluida` e `aria-label` com o estado; contagem e botão "Limpar" na página.
- `FormatacaoGeracao`: custo da geração, subtítulo da lista, perfil ("7 pares · 8 ímpares", "Soma 120", "8 repetidas
  do 3810"), aviso de combinações e mensagens do salvamento, com singular e plural.
- Lista gerada guardada só no estado da página (circuito); remover um jogo é local, sem serviço.
- Menu: item "Gerar jogos" (ícone `Casino`) entre "Estatísticas" e "Meus jogos".
- Painel: botão "Gerar jogos" (`Href="gerar-jogos"`) entre "Atualizar resultados" e "Novo jogo"; o teste que exigia
  a ausência do atalho passa a exigir a presença.

### `CLAUDE.md`
- Remove a pendência da geração e registra em "Decisões de negócio já definidas": estratégias, sorteio ponderado
  pela posição, janela, quantidade, fixas até as dezenas por jogo e regras de salvamento.

## Risks / Trade-offs

- [O peso pela posição é uma escolha de projeto, não algo dos documentos] → registrado no `CLAUDE.md` e na descrição
  das estratégias; trocar a fórmula fica restrito a `PesosDezenas`.
- [Testar aleatoriedade] → testes com `Random` de semente fixa para resultados exatos e testes de propriedade
  (fixas presentes, excluídas ausentes, jogos distintos, tamanho certo) sobre várias gerações.
- [Lista perdida ao sair da página] → aceitável: os jogos só valem depois de salvos; a página não guarda rascunho.
- [Salvar 100 jogos de 20 dezenas faz 100 consultas de duplicado] → base local SQLite; aceitável.
