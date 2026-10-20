# Lotofácil — Guia do Projeto

Sistema **local** para apoiar a escolha de dezenas da Lotofácil (loteria federal da Caixa).

## O que o sistema faz
- Busca na fonte oficial da Caixa os resultados dos concursos, **somente quando o usuário solicitar**.
- Persiste os resultados localmente e gera estatísticas:
  - dezenas mais sorteadas / menos sorteadas;
  - dezenas há mais tempo sem sair / há menos tempo sem sair (atraso).
- Gera jogos a partir dessas estatísticas, com opção de dezenas fixas (presentes em todos os jogos).
- Salva os jogos vinculados a um **concurso futuro** e, após a atualização dos resultados, confere os
  acertos e mostra o valor do prêmio (quando houver).

> Estatística **não aumenta** a probabilidade de acerto: cada sorteio é independente
> (1 em 3.268.760 para 15 acertos com aposta de 15 dezenas). O gerador aplica critérios
> estatísticos escolhidos pelo usuário; nunca apresentar isso como garantia ou aumento de chance.

## Regras da Lotofácil relevantes ao código
- 25 dezenas disponíveis (01 a 25); são sorteadas 15 por concurso.
- Aposta de 15 a 20 dezenas; preço da aposta mínima configurável (hoje R$ 3,50).
- Faixas de prêmio: 11, 12, 13, 14 e 15 acertos.
  - 11, 12 e 13: valores fixos hoje (R$ 7, R$ 14, R$ 35), mas já foram diferentes no passado.
  - 14 e 15: variam conforme o rateio do concurso.
  - A conferência de um jogo usa **sempre o rateio gravado do próprio concurso**, em todas as faixas.
    Os valores fixos da configuração servem apenas para estimativas.
- Fonte dos resultados: API usada pelo portal de loterias da Caixa
  (`servicebus3.caixa.gov.br/portaldeloterias/api/lotofacil/{concurso}`). Não é documentada e o
  host já mudou (era `servicebus2`): manter a URL base em configuração, tratar falhas, buscar de
  forma incremental e com pausa entre requisições.
- Referências detalhadas sobre regras, dados e estatísticas ficam em `docs/`.

## Pendências de negócio (não implementar sem definição do usuário)
- Estratégia de geração de jogos (o usuário enviará material específico).
- Apostas de 16 a 20 dezenas (hoje só 15; ver abaixo).
- Dezenas fixas na geração: regras de validação, dezenas excluídas.
- Janela da análise para a geração de jogos (para as estatísticas, já definida abaixo).
- Teimosinha e Bolão.

## Decisões de negócio já definidas
- Janela das estatísticas: todo o histórico (padrão) ou os últimos 10, 20, 50 ou 100 concursos, sem intervalo
  personalizado. A janela afeta a frequência; o atraso é sempre calculado sobre todo o histórico.
- Jogos: somente apostas de 15 dezenas; cadastro manual num volante (a geração vai salvar pelo mesmo serviço).
- Jogos só para concursos futuros (número maior que o último concurso gravado); com a base vazia, o cadastro pede
  a importação do histórico.
- Jogo com as mesmas dezenas no mesmo concurso é rejeitado com aviso (também por índice único no banco).
- Conferência calculada ao exibir, pelo rateio gravado do próprio concurso; prêmio prescreve 90 dias após o sorteio.

## Stack
- .NET 10, C#, Blazor Server com MudBlazor. Roda apenas em `localhost`.
- **Sem** controllers e **sem** minimal API. A interface usa o próprio circuito SignalR do
  Blazor Server: componentes chamam serviços via injeção de dependência. Não criar Hubs próprios.
- Entity Framework Core com SQLite (arquivo local). O banco é apenas repositório: sem regra de
  negócio em procedures, triggers ou views.

## Dependências
Autorizadas:
- `MudBlazor`, `Newtonsoft.Json`, `StyleCop.Analyzers`
- `Microsoft.EntityFrameworkCore.Sqlite` (e ferramentas do EF Core para migrations)
- Log: `Serilog.Extensions.Logging`, `Serilog.Sinks.File` (somente erros, em arquivo na pasta `log`)
- Testes: `xunit` (`xunit.v3`, executado pela Microsoft.Testing.Platform), `NSubstitute`, `bunit`

Regras:
- **Nunca** adicionar pacote fora desta lista sem pedir autorização e explicar o motivo.
- Não usar jQuery nem Bootstrap (MudBlazor cobre a interface). Remover o Bootstrap do template.
- Serialização JSON feita pelo nosso código usa Newtonsoft.Json.

## Arquitetura
Arquitetura limpa, enxuta. Projetos:

| Projeto | Responsabilidade | Depende de |
|---|---|---|
| `Lotofacil.Dominio` | Entidades, regras de negócio, interfaces (repositórios, fonte de resultados) | nada |
| `Lotofacil.Aplicacao` | Casos de uso / serviços que orquestram o domínio | Dominio |
| `Lotofacil.Infraestrutura` | EF Core/SQLite, cliente da API da Caixa, implementações das interfaces | Dominio, Aplicacao |
| `Lotofacil.Web` | Blazor Server + MudBlazor, composição da injeção de dependência | Dominio (somente leitura dos tipos), Aplicacao, Infraestrutura |
| `Lotofacil.Testes.Unidade` | Testes de unidade (Dominio, Aplicacao, componentes com bUnit) | — |
| `Lotofacil.Testes.Integracao` | Testes com SQLite em memória e cliente HTTP falso | — |

Princípios:
- Inversão de dependência: camadas internas definem interfaces; a infraestrutura implementa.
- Baixo acoplamento; cada classe com uma responsabilidade clara.
- Entre duas soluções, escolher a mais simples de entender, sem abrir mão da separação de responsabilidades.
- Sem over engineering: nada de camadas, padrões ou abstrações sem necessidade concreta.
- Implementação incremental, sempre acompanhada de testes de unidade e/ou integração.

## Convenções de código
Seguir os princípios de *Código Limpo*. Código legível vem antes de código "esperto".

Idioma:
- Tudo em **português**: classes, métodos, variáveis, constantes, propriedades, comentários.
- Identificadores **sem acentos** nem cedilha (`Numero`, `Sequencia`, `Premiacao`).
  Acentos apenas em comentários e textos exibidos.

Nomenclatura (padrão .NET):
- Classes, métodos, propriedades, constantes: PascalCase — `ConcursoServico`, `SortearDezena`, `QuantidadeDezenas`.
- Variáveis locais e parâmetros: camelCase — `dezena`, `numeroConcurso`.
- Campos privados: camelCase **sem** underscore, acessados com `this.` — `this.repositorio`.
- Interfaces com prefixo `I` — `IConcursoRepositorio`.
- Métodos assíncronos com sufixo `Async` — `BuscarConcursoAsync`.

Comentários:
- Toda classe tem um `<summary>` curto dizendo o que ela faz.
- Todo método tem `<summary>`, `<param>` para cada argumento e `<returns>` quando retorna valor.
- Comentários curtos; não explicar o óbvio. Comentário interno só quando o *porquê* não é evidente.

Proibido:
- Encadeamentos obscuros ou código difícil de ler (ex.: `Console.WriteLine("x").ToString().Length.ToString()`).
- Excesso de comentários.
- Lógica de negócio em componentes Blazor ou no `DbContext`.

## StyleCop
- `StyleCop.Analyzers` em todos os projetos, configurado por `stylecop.json` e `.editorconfig` na raiz.
- Regras desativadas por exigirem texto em inglês: **SA1623** (summary de propriedade) e
  **SA1642** (summary de construtor). As demais seguem o padrão.
- O build não deve gerar avisos do StyleCop.

## Fluxo de trabalho
- Mudanças passam pelo OpenSpec (`openspec/`): `/opsx:propose` → `/opsx:apply` → `/opsx:archive`.
- Artefatos do OpenSpec em pt-BR.
- Antes de concluir uma tarefa: `dotnet build src/Lotofacil.slnx` sem avisos e
  `dotnet test --solution src/Lotofacil.slnx` passando.
- Fixar o SDK 10 com `global.json` (há SDK 11 RC instalado na máquina).
- Não fazer commit sem o usuário pedir.
