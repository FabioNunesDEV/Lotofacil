// <copyright file="GerarJogosTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Aplicacao.Geracao;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Web;
using Lotofacil.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da página Gerar jogos.
/// </summary>
public class GerarJogosTestes : ComponenteTesteBase
{
    /// <summary>
    /// Resultado do último concurso gravado (3810).
    /// </summary>
    private static readonly int[] Resultado3810 = [1, 5, 6, 7, 10, 11, 14, 15, 16, 17, 18, 19, 20, 23, 24];

    /// <summary>
    /// Concursos gravados na base falsa.
    /// </summary>
    private readonly List<Concurso> concursos = [];

    /// <summary>
    /// Jogos gravados na base falsa.
    /// </summary>
    private readonly List<Jogo> jogos = [];

    /// <summary>
    /// Provedor dos popovers, usado para escolher opções das listas.
    /// </summary>
    private IRenderedComponent<MudPopoverProvider> popovers = null!;

    /// <summary>
    /// Provedor das mensagens rápidas.
    /// </summary>
    private IRenderedComponent<MudSnackbarProvider> mensagens = null!;

    /// <summary>
    /// Prepara os serviços da página sobre as listas em memória.
    /// </summary>
    public GerarJogosTestes()
    {
        IConcursoRepositorio repositorioConcursos = Substitute.For<IConcursoRepositorio>();
        repositorioConcursos.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>())
            .Returns(_ => this.concursos.Count == 0 ? null : this.concursos.Max(concurso => concurso.Numero));
        repositorioConcursos.ObterAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.concursos.Find(concurso => concurso.Numero == chamada.ArgAt<int>(0)));
        repositorioConcursos.ContarAsync(Arg.Any<CancellationToken>()).Returns(_ => this.concursos.Count);
        repositorioConcursos.ListarResultadosAsync(Arg.Any<CancellationToken>())
            .Returns(_ => this.concursos.Select(concurso => new ResultadoSorteio(concurso.Numero, concurso.Dezenas)).ToList());

        IJogoRepositorio repositorioJogos = Substitute.For<IJogoRepositorio>();
        repositorioJogos.ListarAsync(Arg.Any<CancellationToken>()).Returns(_ => this.jogos.ToList());
        repositorioJogos.ExisteAsync(Arg.Any<int>(), Arg.Any<IEnumerable<int>>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.jogos.Any(jogo =>
                jogo.NumeroConcurso == chamada.ArgAt<int>(0)
                && jogo.Dezenas.SequenceEqual(chamada.ArgAt<IEnumerable<int>>(1).Order())));
        repositorioJogos.AdicionarAsync(Arg.Any<Jogo>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                this.Gravar(chamada.ArgAt<Jogo>(0));
                return Task.CompletedTask;
            });

        var servicoJogos = new JogosServico(repositorioJogos, repositorioConcursos);
        this.Services.AddSingleton(servicoJogos);
        this.Services.AddSingleton(new GeracaoJogosServico(
            repositorioConcursos,
            new EstatisticasServico(repositorioConcursos),
            servicoJogos,
            new Random(11)));
        this.Services.AddSingleton(new OpcoesApostas(3.50m));
    }

    /// <summary>
    /// A página mostra o título e o aviso fixo.
    /// </summary>
    [Fact]
    public void ExibeTituloEAviso()
    {
        this.AdicionarConcursos();

        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        Assert.Equal("Gerar jogos", Texto(pagina.Find("h1")));
        Assert.Equal(
            "Nenhuma estratégia aumenta a chance de acerto: cada jogo de 15 dezenas tem 1 chance em 3.268.760 em todo "
                + "concurso. As estratégias só organizam a escolha das dezenas pelo critério escolhido.",
            Texto(pagina.Find(".aviso")));
    }

    /// <summary>
    /// Sem concursos, a página mostra a mensagem de base vazia no lugar do formulário.
    /// </summary>
    [Fact]
    public void BaseVazia()
    {
        IRenderedComponent<GerarJogos> pagina = this.Render<GerarJogos>();

        pagina.WaitForAssertion(() => Assert.Equal(
            "Nenhum concurso gravado na base local. Use “Importar histórico” na página Resultados para carregar os concursos da Caixa.",
            Texto(pagina.Find(".sem-registros"))));
        Assert.Empty(pagina.FindAll(".formulario"));
        Assert.Single(pagina.FindAll(".aviso"));
    }

    /// <summary>
    /// Valores do formulário ao abrir.
    /// </summary>
    [Fact]
    public void ValoresAoAbrir()
    {
        this.AdicionarConcursos();

        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        Assert.Equal("3811", pagina.Find(".campo-concurso input").GetAttribute("value"));
        Assert.Equal("10", pagina.Find(".campo-quantidade input").GetAttribute("value"));
        Assert.Equal("15", pagina.Find(".campo-dezenas-por-jogo input").GetAttribute("value"));
        Assert.Equal("Aleatória (Surpresinha)", pagina.Find(".campo-estrategia input").GetAttribute("value"));
        Assert.Equal("Todas as dezenas livres têm a mesma chance.", Texto(pagina.Find(".descricao-estrategia")));
        Assert.Equal("Todos (30)", pagina.Find(".campo-janela input").GetAttribute("value"));
        Assert.Equal("10 jogos de 15 dezenas · R$ 35,00", Texto(pagina.Find(".custo")));
        Assert.Equal("Nenhum jogo gerado ainda", Texto(pagina.Find(".subtitulo-lista")));
        Assert.Equal("Fixas: 0 · Excluídas: 0", Texto(pagina.Find(".contagem-fixas")));
    }

    /// <summary>
    /// O custo acompanha a quantidade e as dezenas por jogo.
    /// </summary>
    [Fact]
    public void CustoDeApostasMultiplas()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        pagina.Find(".campo-quantidade input").Change("5");
        this.Escolher(pagina, ".campo-dezenas-por-jogo", "17");

        pagina.WaitForAssertion(() => Assert.Equal("5 jogos de 17 dezenas · R$ 2.380,00", Texto(pagina.Find(".custo"))));
    }

    /// <summary>
    /// A descrição acompanha a estratégia escolhida, e a janela oferece os últimos concursos.
    /// </summary>
    [Fact]
    public void TrocaEstrategiaEJanela()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        this.Escolher(pagina, ".campo-estrategia", "Mais atrasadas");
        this.Escolher(pagina, ".campo-janela", "Últimos 20");

        Assert.Equal(
            "Mais chance para as dezenas há mais tempo sem sair (todo o histórico).",
            Texto(pagina.Find(".descricao-estrategia")));
    }

    /// <summary>
    /// A contagem acompanha fixas e excluídas, e "Limpar" libera todas.
    /// </summary>
    [Fact]
    public void ContagemDeFixasEExcluidasELimpar()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        Clicar(pagina, 5, 13, 1, 1);

        Assert.Equal("Fixas: 2 · Excluídas: 1", Texto(pagina.Find(".contagem-fixas")));

        pagina.Find(".botao-limpar").Click();

        Assert.Equal("Fixas: 0 · Excluídas: 0", Texto(pagina.Find(".contagem-fixas")));
        Assert.Empty(pagina.FindAll(".volante .fixa, .volante .excluida"));
    }

    /// <summary>
    /// Gera 10 jogos distintos com as fixas, sem as excluídas, com as fixas em destaque.
    /// </summary>
    [Fact]
    public void GeraRespeitandoFixasEExcluidas()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();
        Clicar(pagina, 5, 13, 1, 1, 25, 25);

        pagina.Find(".botao-gerar").Click();

        pagina.WaitForAssertion(() => Assert.Equal(10, pagina.FindAll(".linha-gerado").Count));
        List<List<string>> gerados = JogosExibidos(pagina);
        Assert.Equal(10, gerados.Select(jogo => string.Join(',', jogo)).Distinct().Count());
        Assert.All(gerados, jogo =>
        {
            Assert.Equal(15, jogo.Count);
            Assert.Contains("05", jogo);
            Assert.Contains("13", jogo);
            Assert.DoesNotContain("01", jogo);
            Assert.DoesNotContain("25", jogo);
        });
        Assert.All(pagina.FindAll(".linha-gerado"), linha => Assert.Equal(
            ["05", "13"],
            linha.QuerySelectorAll(".bola.fixa").Select(Texto)));
        Assert.Equal("10 jogos · R$ 35,00", Texto(pagina.Find(".subtitulo-lista")));
    }

    /// <summary>
    /// Com o jogo inteiro fixo, gera um único jogo, avisa e mostra o perfil contra o último concurso.
    /// </summary>
    [Fact]
    public void JogoInteiroFixoComAvisoEPerfil()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();
        Clicar(pagina, [.. Enumerable.Range(1, 15)]);

        pagina.Find(".botao-gerar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains(
            "Só existe 1 jogo diferente com essas fixas e excluídas; foi gerado 1.",
            this.MensagensExibidas()));
        IElement linha = Assert.Single(pagina.FindAll(".linha-gerado"));
        Assert.Equal("01", Texto(linha.QuerySelector(".numero-jogo")!));
        Assert.Equal("7 pares · 8 ímpares", Texto(linha.QuerySelector(".paridade")!));
        Assert.Equal("Soma 120", Texto(linha.QuerySelector(".soma")!));
        Assert.Equal("8 repetidas do 3810", Texto(linha.QuerySelector(".repetidas")!));
        Assert.Empty(linha.QuerySelectorAll(".quantidade-dezenas"));
    }

    /// <summary>
    /// Aposta de 16 dezenas mostra a quantidade de dezenas na linha.
    /// </summary>
    [Fact]
    public void ApostaMultiplaMostraAQuantidade()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();
        this.Escolher(pagina, ".campo-dezenas-por-jogo", "16");

        pagina.Find(".botao-gerar").Click();

        pagina.WaitForAssertion(() => Assert.Equal(10, pagina.FindAll(".linha-gerado").Count));
        Assert.All(pagina.FindAll(".linha-gerado .quantidade-dezenas"), chip => Assert.Equal("16 dezenas", Texto(chip)));
        Assert.Equal("10 jogos · R$ 560,00", Texto(pagina.Find(".subtitulo-lista")));
    }

    /// <summary>
    /// Fixas demais: nenhum jogo é gerado e a página explica.
    /// </summary>
    [Fact]
    public void RecusaFixasDemais()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();
        Clicar(pagina, [.. Enumerable.Range(1, 16)]);

        pagina.Find(".botao-gerar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains(
            "Há 16 dezenas fixas, mais que as 15 dezenas por jogo.",
            this.MensagensExibidas()));
        Assert.Empty(pagina.FindAll(".linha-gerado"));
    }

    /// <summary>
    /// Livres insuficientes: nenhum jogo é gerado e a página explica.
    /// </summary>
    [Fact]
    public void RecusaLivresInsuficientes()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();
        Clicar(pagina, [.. Enumerable.Range(1, 5)]);
        int[] excluir = [.. Enumerable.Range(6, 12)];
        Clicar(pagina, [.. excluir, .. excluir]);

        pagina.Find(".botao-gerar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains(
            "Faltam dezenas livres: com 5 fixas e 12 excluídas sobram 8 livres, e cada jogo precisa de mais 10.",
            this.MensagensExibidas()));
        Assert.Empty(pagina.FindAll(".linha-gerado"));
    }

    /// <summary>
    /// Remover um jogo renumera os demais e atualiza o subtítulo.
    /// </summary>
    [Fact]
    public void RemoverRenumera()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.GerarDez();
        List<List<string>> antes = JogosExibidos(pagina);

        pagina.FindAll(".botao-remover")[2].Click();

        Assert.Equal(9, pagina.FindAll(".linha-gerado").Count);
        Assert.Equal(
            Enumerable.Range(1, 9).Select(numero => numero.ToString("00")),
            pagina.FindAll(".linha-gerado .numero-jogo").Select(Texto));
        Assert.DoesNotContain(string.Join(',', antes[2]), JogosExibidos(pagina).Select(jogo => string.Join(',', jogo)));
        Assert.Equal("9 jogos · R$ 31,50", Texto(pagina.Find(".subtitulo-lista")));
    }

    /// <summary>
    /// "Gerar novamente" substitui a lista por uma nova geração.
    /// </summary>
    [Fact]
    public void GerarNovamenteSubstituiALista()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.GerarDez();
        string antes = string.Join('|', JogosExibidos(pagina).Select(jogo => string.Join(',', jogo)));

        pagina.Find(".botao-gerar-novamente").Click();

        pagina.WaitForAssertion(() => Assert.NotEqual(
            antes,
            string.Join('|', JogosExibidos(pagina).Select(jogo => string.Join(',', jogo)))));
        Assert.Equal(10, pagina.FindAll(".linha-gerado").Count);
    }

    /// <summary>
    /// Salvar grava todos os jogos, esvazia a lista e os jogos aparecem em Meus jogos.
    /// </summary>
    [Fact]
    public void SalvaTodos()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.GerarDez();

        pagina.Find(".botao-salvar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains("10 jogos salvos para o concurso 3811.", this.MensagensExibidas()));
        pagina.WaitForAssertion(() => Assert.Empty(pagina.FindAll(".linha-gerado")));
        Assert.Equal("Nenhum jogo gerado ainda", Texto(pagina.Find(".subtitulo-lista")));
        Assert.Equal(10, this.jogos.Count(jogo => jogo.NumeroConcurso == 3811));

        IRenderedComponent<MeusJogos> meusJogos = this.Render<MeusJogos>();
        meusJogos.WaitForAssertion(() => Assert.Equal(
            "10 jogos",
            meusJogos.FindAll(".grupo[data-concurso='3811'] .cabecalho-grupo .mud-chip").Select(Texto).ElementAt(1)));
    }

    /// <summary>
    /// Jogos já cadastrados para o concurso são recusados e os demais gravados.
    /// </summary>
    [Fact]
    public void SalvaRecusandoDuplicados()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.GerarDez();
        foreach (List<string> jogo in JogosExibidos(pagina).Take(2))
        {
            this.Gravar(new Jogo(3811, jogo.Select(int.Parse), DateTime.Now));
        }

        pagina.Find(".botao-salvar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains(
            "8 jogos salvos para o concurso 3811; 2 já estavam cadastrados.",
            this.MensagensExibidas()));
        Assert.Equal(10, this.jogos.Count(jogo => jogo.NumeroConcurso == 3811));
    }

    /// <summary>
    /// Concurso já sorteado: nada é gravado e a lista continua.
    /// </summary>
    [Fact]
    public void SalvarParaConcursoJaSorteado()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.GerarDez();

        pagina.Find(".campo-concurso input").Change("3810");
        pagina.Find(".botao-salvar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains(
            "Só é possível cadastrar jogos para concursos depois do 3810.",
            this.MensagensExibidas()));
        Assert.Empty(this.jogos);
        Assert.Equal(10, pagina.FindAll(".linha-gerado").Count);
    }

    /// <summary>
    /// Ao abrir, a seção de filtros está fechada e sem filtros ligados.
    /// </summary>
    [Fact]
    public void FiltrosFechadosAoAbrir()
    {
        this.AdicionarConcursos();

        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        Assert.Equal("Nenhum filtro ligado", Texto(pagina.Find(".resumo-filtros")));
        Assert.DoesNotContain("mud-panel-expanded", pagina.Find(".filtros-perfil .mud-expand-panel").ClassList);

        AbrirFiltros(pagina);
        Assert.Contains("mud-panel-expanded", pagina.Find(".filtros-perfil .mud-expand-panel").ClassList);
    }

    /// <summary>
    /// Com pares de 7 a 8 e soma de 180 a 209, todos os jogos gerados atendem aos dois filtros.
    /// </summary>
    [Fact]
    public void GeraJogosQueAtendemAosFiltros()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        AbrirFiltros(pagina);
        Preencher(pagina, "Pares", ".campo-minimo", "7");
        Preencher(pagina, "Pares", ".campo-maximo", "8");
        Preencher(pagina, "Soma", ".campo-minimo", "180");
        Preencher(pagina, "Soma", ".campo-maximo", "209");
        Assert.Equal("2 filtros ligados", Texto(pagina.Find(".resumo-filtros")));

        pagina.Find(".botao-gerar").Click();

        pagina.WaitForAssertion(() => Assert.Equal(10, pagina.FindAll(".linha-gerado").Count));
        Assert.All(JogosExibidos(pagina), jogo =>
        {
            List<int> dezenas = jogo.Select(int.Parse).ToList();
            Assert.InRange(dezenas.Count(dezena => dezena % 2 == 0), 7, 8);
            Assert.InRange(dezenas.Sum(), 180, 209);
        });
    }

    /// <summary>
    /// Mínimo maior que o máximo recusa a geração com o nome do filtro.
    /// </summary>
    [Fact]
    public void RecusaMinimoMaiorQueMaximo()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        AbrirFiltros(pagina);
        Preencher(pagina, "Soma", ".campo-minimo", "210");
        Preencher(pagina, "Soma", ".campo-maximo", "180");
        pagina.Find(".botao-gerar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains(
            "No filtro de soma, o mínimo (210) é maior que o máximo (180).",
            this.MensagensExibidas()));
        Assert.Empty(pagina.FindAll(".linha-gerado"));
    }

    /// <summary>
    /// Mínimo de 13 pares em jogos de 15 dezenas: nenhum jogo atende.
    /// </summary>
    [Fact]
    public void NenhumJogoAtendeAosFiltros()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        AbrirFiltros(pagina);
        Preencher(pagina, "Pares", ".campo-minimo", "13");
        pagina.Find(".botao-gerar").Click();

        this.mensagens.WaitForAssertion(() => Assert.Contains(
            "Nenhum jogo atendeu aos filtros. Afrouxe os limites ou revise as fixas e excluídas.",
            this.MensagensExibidas()));
        Assert.Empty(pagina.FindAll(".linha-gerado"));
    }

    /// <summary>
    /// Ao voltar de 18 para 15 dezenas por jogo, um limite de 17 pares passa a 15.
    /// </summary>
    [Fact]
    public void AjustaLimitesAoTrocarDezenasPorJogo()
    {
        this.AdicionarConcursos();
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();

        this.Escolher(pagina, ".campo-dezenas-por-jogo", "18");
        AbrirFiltros(pagina);
        Preencher(pagina, "Pares", ".campo-minimo", "17");
        this.Escolher(pagina, ".campo-dezenas-por-jogo", "15");

        pagina.WaitForAssertion(() => Assert.Equal(
            "15",
            pagina.Find(".linha-filtro[data-perfil='Pares'] .campo-minimo input").GetAttribute("value")));
    }

    /// <summary>
    /// Abre a seção de filtros de perfil.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    private static void AbrirFiltros(IRenderedComponent<GerarJogos> pagina)
    {
        pagina.Find(".filtros-perfil .mud-expand-panel-header").Click();
        pagina.WaitForAssertion(() => Assert.NotEmpty(pagina.FindAll(".linha-filtro")));
    }

    /// <summary>
    /// Preenche um limite de um perfil na seção de filtros.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="perfil">Nome do perfil no enum (ex.: "Pares").</param>
    /// <param name="campo">Classe do campo: ".campo-minimo" ou ".campo-maximo".</param>
    /// <param name="valor">Valor digitado.</param>
    private static void Preencher(IRenderedComponent<GerarJogos> pagina, string perfil, string campo, string valor)
    {
        string seletor = $".linha-filtro[data-perfil='{perfil}'] {campo} input";
        pagina.Find(seletor).Change(valor);
        pagina.WaitForAssertion(() => Assert.Equal(valor, pagina.Find(seletor).GetAttribute("value")));
    }

    /// <summary>
    /// Clica nas dezenas do volante, na ordem dada.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="dezenas">Dezenas a clicar; repetir uma dezena clica de novo.</param>
    private static void Clicar(IRenderedComponent<GerarJogos> pagina, params int[] dezenas)
    {
        foreach (int dezena in dezenas)
        {
            pagina.FindAll(".volante button").Single(botao => Texto(botao) == dezena.ToString("00")).Click();
        }
    }

    /// <summary>
    /// Dezenas de cada jogo da lista.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <returns>Dezenas exibidas de cada jogo.</returns>
    private static List<List<string>> JogosExibidos(IRenderedComponent<GerarJogos> pagina)
    {
        return pagina.FindAll(".linha-gerado")
            .Select(linha => linha.QuerySelectorAll(".bola").Select(Texto).ToList())
            .ToList();
    }

    /// <summary>
    /// Texto de um elemento, com espaços não separáveis trocados por espaços comuns.
    /// </summary>
    /// <param name="elemento">Elemento da página.</param>
    /// <returns>Texto sem espaços nas pontas.</returns>
    private static string Texto(IElement elemento)
    {
        return elemento.TextContent.Replace(' ', ' ').Trim();
    }

    /// <summary>
    /// Mensagens rápidas exibidas.
    /// </summary>
    /// <returns>Textos das mensagens.</returns>
    private List<string> MensagensExibidas()
    {
        return this.mensagens.FindAll(".mud-snackbar-content-message").Select(Texto).ToList();
    }

    /// <summary>
    /// Escolhe uma opção numa lista do formulário.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="campo">Classe do campo.</param>
    /// <param name="opcao">Texto da opção.</param>
    private void Escolher(IRenderedComponent<GerarJogos> pagina, string campo, string opcao)
    {
        pagina.Find(campo).MouseDown();
        this.popovers.WaitForAssertion(() => Assert.NotEmpty(this.popovers.FindAll(".mud-popover-open .mud-list-item")));
        this.popovers.FindAll(".mud-popover-open .mud-list-item").Single(item => Texto(item) == opcao).Click();

        pagina.WaitForAssertion(() => Assert.Equal(opcao, pagina.Find($"{campo} input").GetAttribute("value")));
    }

    /// <summary>
    /// Renderiza a página e gera 10 jogos de 15 dezenas sem fixas.
    /// </summary>
    /// <returns>Página com os jogos gerados.</returns>
    private IRenderedComponent<GerarJogos> GerarDez()
    {
        IRenderedComponent<GerarJogos> pagina = this.RenderizarPagina();
        pagina.Find(".botao-gerar").Click();
        pagina.WaitForAssertion(() => Assert.Equal(10, pagina.FindAll(".linha-gerado").Count));

        return pagina;
    }

    /// <summary>
    /// Grava os concursos 3781 a 3810; o último com o resultado real do 3810.
    /// </summary>
    private void AdicionarConcursos()
    {
        this.concursos.AddRange(Enumerable.Range(3781, 29).Select(numero => ConcursoFabrica.Criar(
            numero: numero,
            dezenas: numero % 2 == 0 ? null : Enumerable.Range(1, 15))));
        this.concursos.Add(ConcursoFabrica.Criar(numero: 3810, dezenas: Resultado3810));
    }

    /// <summary>
    /// Grava um jogo na lista falsa, com identificador.
    /// </summary>
    /// <param name="jogo">Jogo a gravar.</param>
    private void Gravar(Jogo jogo)
    {
        typeof(Jogo).GetProperty(nameof(Jogo.Id))!.SetValue(jogo, this.jogos.Count + 1);
        this.jogos.Add(jogo);
    }

    /// <summary>
    /// Renderiza os provedores e a página na cultura da aplicação e espera o formulário.
    /// </summary>
    /// <returns>Página renderizada.</returns>
    private IRenderedComponent<GerarJogos> RenderizarPagina()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
        this.popovers = this.Render<MudPopoverProvider>();
        this.mensagens = this.Render<MudSnackbarProvider>();

        IRenderedComponent<GerarJogos> pagina = this.Render<GerarJogos>();
        pagina.WaitForAssertion(() => Assert.NotEmpty(pagina.FindAll(".formulario")));

        return pagina;
    }
}
