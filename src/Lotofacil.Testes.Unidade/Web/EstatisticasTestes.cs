// <copyright file="EstatisticasTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Dominio.Concursos;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da página Estatísticas, com o serviço real sobre um repositório falso em memória.
/// </summary>
public class EstatisticasTestes : ComponenteTesteBase
{
    /// <summary>
    /// Dezenas 01 a 15.
    /// </summary>
    private static readonly int[] De01a15 = Enumerable.Range(1, 15).ToArray();

    /// <summary>
    /// Dezenas 11 a 25.
    /// </summary>
    private static readonly int[] De11a25 = Enumerable.Range(11, 15).ToArray();

    /// <summary>
    /// Resultados devolvidos pelo repositório falso.
    /// </summary>
    private readonly List<ResultadoSorteio> historico = [];

    /// <summary>
    /// Registra o serviço de estatísticas sobre um repositório que devolve <see cref="historico"/>.
    /// </summary>
    public EstatisticasTestes()
    {
        IConcursoRepositorio repositorio = Substitute.For<IConcursoRepositorio>();
        repositorio.ListarResultadosAsync(Arg.Any<CancellationToken>()).Returns(_ => this.historico.ToList());
        this.Services.AddSingleton(new EstatisticasServico(repositorio));
    }

    /// <summary>
    /// A página exibe o título e o aviso fixo.
    /// </summary>
    [Fact]
    public void ExibeTituloEAviso()
    {
        this.AdicionarHistoricoPadrao();

        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        Assert.Equal("Estatísticas", Texto(pagina.Find("h1")));
        Assert.StartsWith("As estatísticas descrevem o passado.", Texto(pagina.Find(".aviso")));
    }

    /// <summary>
    /// Ao abrir, a janela é todo o histórico e o intervalo é informado.
    /// </summary>
    [Fact]
    public void AbreComTodoOHistorico()
    {
        this.AdicionarHistoricoPadrao();

        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        Assert.Equal("Concursos 1 a 3808 · 3.808 concursos", Texto(pagina.Find(".intervalo")));
        IElement todos = Chip(pagina, "Todos (3.808)");
        Assert.Contains(todos.ClassList, classe => classe.Contains("selected", StringComparison.Ordinal));
        Assert.Equal(
            ["Todos (3.808)", "Últimos 10", "Últimos 20", "Últimos 50", "Últimos 100"],
            pagina.FindAll(".janelas .mud-chip").Select(Texto));
    }

    /// <summary>
    /// Escolher "Últimos 10" recalcula sobre os 10 concursos mais recentes.
    /// </summary>
    [Fact]
    public void TrocaParaOsUltimosDez()
    {
        this.AdicionarHistoricoPadrao();
        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        Chip(pagina, "Últimos 10").Click();

        pagina.WaitForAssertion(() => Assert.Equal(
            "Concursos 3799 a 3808 · 10 concursos",
            Texto(pagina.Find(".intervalo"))));
        Assert.Equal("Esperado por dezena: 6,0 (60% dos concursos)", Texto(pagina.Find(".esperado")));
    }

    /// <summary>
    /// Com a base vazia, a mensagem substitui as estatísticas e o aviso continua.
    /// </summary>
    [Fact]
    public void BaseVaziaMostraMensagem()
    {
        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        Assert.Equal(
            "Nenhum concurso gravado na base local. Use “Importar histórico” na página Resultados.",
            Texto(pagina.Find(".sem-registros")));
        Assert.Single(pagina.FindAll(".aviso"));
        Assert.Empty(pagina.FindAll(".mapa-calor"));
        Assert.Empty(pagina.FindAll(".tabela-dezenas"));
        Assert.Empty(pagina.FindAll(".ranking"));
    }

    /// <summary>
    /// A linha da dezena 20 mostra os valores do exemplo da spec e o esperado é informado.
    /// </summary>
    [Fact]
    public void TabelaMostraOsValoresDaDezena()
    {
        this.AdicionarHistoricoPadrao();

        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        Assert.Equal("Esperado por dezena: 2.284,8 (60% dos concursos)", Texto(pagina.Find(".esperado")));
        IReadOnlyList<IElement> linhas = pagina.FindAll(".tabela-dezenas tbody tr");
        Assert.Equal(25, linhas.Count);
        Assert.Equal(
            ["20", "2.312", "60,71%", "+27,2", "+0,9", "3", "3805"],
            linhas[19].QuerySelectorAll("td").Select(Texto));
        Assert.Contains("acima", linhas[19].QuerySelectorAll("td")[3].ClassList);
        Assert.Contains("abaixo", linhas[0].QuerySelectorAll("td")[3].ClassList);
    }

    /// <summary>
    /// Os rankings mostram as 5 primeiras dezenas, o valor de cada uma e a indicação de empate.
    /// </summary>
    [Fact]
    public void RankingsMostramDezenasValoresEEmpate()
    {
        this.AdicionarHistoricoPadrao();

        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        // 11 a 15 saíram em todos os 3.808 concursos; 16 a 25, como a 20, em 2.312.
        IElement maisSorteadas = pagina.Find(".mais-sorteadas");
        Assert.Equal("Mais sorteadas", Texto(maisSorteadas.QuerySelector("p")!));
        Assert.Equal(["11", "12", "13", "14", "15"], maisSorteadas.QuerySelectorAll(".bola").Select(Texto));
        Assert.All(maisSorteadas.QuerySelectorAll(".valor"), valor => Assert.Equal("3.808", Texto(valor)));
        Assert.Empty(maisSorteadas.QuerySelectorAll(".empate"));

        // O último concurso tem as dezenas 01 a 15: atraso 0 para todas elas.
        IElement menosAtrasadas = pagina.Find(".menos-atrasadas");
        Assert.Equal(["01", "02", "03", "04", "05"], menosAtrasadas.QuerySelectorAll(".bola").Select(Texto));
        Assert.All(menosAtrasadas.QuerySelectorAll(".valor"), valor => Assert.Equal("último concurso", Texto(valor)));
        Assert.Equal("+10 empatadas com atraso 0", Texto(menosAtrasadas.QuerySelector(".empate")!));

        IElement maisAtrasadas = pagina.Find(".mais-atrasadas");
        Assert.Equal("3 conc.", Texto(maisAtrasadas.QuerySelectorAll(".valor")[0]));
        Assert.Equal("+5 empatadas com atraso 3", Texto(maisAtrasadas.QuerySelector(".empate")!));
    }

    /// <summary>
    /// O mapa começa na frequência, com as 25 dezenas na ordem do volante e a legenda da métrica.
    /// </summary>
    [Fact]
    public void MapaComecaNaFrequencia()
    {
        this.AdicionarHistoricoPadrao();

        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        IReadOnlyList<IElement> celulas = pagina.FindAll(".mapa-calor .celula");
        Assert.Equal(25, celulas.Count);
        Assert.Equal("01", Texto(celulas[0].QuerySelector("strong")!));
        Assert.Equal("2.312", Texto(celulas[19].QuerySelector("span")!));
        Assert.Equal(
            "Quantidade de concursos em que a dezena saiu. Mais escuro = mais sorteada.",
            Texto(pagina.Find(".legenda")));
    }

    /// <summary>
    /// Trocar para o atraso mostra o atraso de cada dezena e a legenda do atraso.
    /// </summary>
    [Fact]
    public void MapaTrocaParaOAtraso()
    {
        this.AdicionarHistoricoPadrao();
        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        BotaoMetrica(pagina, "Atraso").Click();

        IReadOnlyList<IElement> celulas = pagina.FindAll(".mapa-calor .celula");
        Assert.Equal("0 conc.", Texto(celulas[0].QuerySelector("span")!));
        Assert.Equal("3 conc.", Texto(celulas[19].QuerySelector("span")!));
        Assert.Equal(
            "Concursos desde a última vez que a dezena saiu. Mais escuro = mais atrasada.",
            Texto(pagina.Find(".legenda")));
    }

    /// <summary>
    /// No desvio, cada célula mostra o valor em σ e indica se está acima ou abaixo do esperado.
    /// </summary>
    [Fact]
    public void MapaMostraODesvio()
    {
        this.AdicionarHistoricoPadrao();
        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        BotaoMetrica(pagina, "Desvio").Click();

        IReadOnlyList<IElement> celulas = pagina.FindAll(".mapa-calor .celula");
        Assert.Equal("+0,9σ", Texto(celulas[19].QuerySelector("span")!));
        Assert.Contains("acima", celulas[19].ClassList);
        Assert.StartsWith("-", Texto(celulas[0].QuerySelector("span")!));
        Assert.Contains("abaixo", celulas[0].ClassList);
        Assert.Equal(
            "Distância da frequência esperada em desvios-padrão. Verde acima, vermelho abaixo do esperado.",
            Texto(pagina.Find(".legenda")));
    }

    /// <summary>
    /// Ao abrir, a aba "Por dezena" está selecionada e mostra o mapa, os rankings e a tabela.
    /// </summary>
    [Fact]
    public void AbreNaAbaPorDezena()
    {
        this.AdicionarHistoricoPadrao();

        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        Assert.Equal(["Por dezena", "Por perfil do concurso"], pagina.FindAll(".abas .mud-tab").Select(Texto));
        Assert.Equal("Por dezena", Texto(pagina.Find(".abas .mud-tab-active")));
        Assert.Single(pagina.FindAll(".mapa-calor"));
        Assert.Equal(4, pagina.FindAll(".ranking").Count);
        Assert.Single(pagina.FindAll(".tabela-dezenas"));
        Assert.Empty(pagina.FindAll(".cartao-perfil"));
    }

    /// <summary>
    /// A aba de perfis mostra os 7 cartões na ordem da spec, com o aviso fixo fora das abas.
    /// </summary>
    [Fact]
    public void AbaDePerfisMostraOsCartoesNaOrdem()
    {
        this.AdicionarHistoricoPadrao();
        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        Aba(pagina, "Por perfil do concurso").Click();

        Assert.Equal(
            [
                "Pares e ímpares",
                "Números primos",
                "Múltiplos de 3",
                "Fibonacci",
                "Soma das dezenas",
                "Repetidas do concurso anterior",
                "Moldura e miolo",
            ],
            pagina.FindAll(".cartao-perfil h2").Select(Texto));
        Assert.Single(pagina.FindAll(".aviso"));
        Assert.Empty(pagina.FindAll(".mapa-calor"));
    }

    /// <summary>
    /// Na soma, sobre todo o histórico, 2.312 dos 3.808 concursos (as dezenas 11 a 25) caem em "≥ 225".
    /// </summary>
    [Fact]
    public void AbaDePerfisMostraOObservadoDeTodoOHistorico()
    {
        this.AdicionarHistoricoPadrao();
        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();

        Aba(pagina, "Por perfil do concurso").Click();

        IReadOnlyList<IElement> linhas = LinhasDoCartao(pagina, "Soma das dezenas");
        Assert.Equal(
            ["≤ 164", "165–179", "180–194", "195–209", "210–224", "≥ 225"],
            linhas.Select(linha => Texto(linha.QuerySelector(".rotulo")!)));
        Assert.Equal("39,29%", Texto(linhas[0].QuerySelectorAll("td")[2]));
        Assert.Equal("60,71%", Texto(linhas[^1].QuerySelectorAll("td")[2]));
        Assert.Equal("último", Texto(linhas[0].QuerySelectorAll("td")[4]));
        Assert.Equal("3", Texto(linhas[^1].QuerySelectorAll("td")[4]));
        Assert.Equal("—", Texto(linhas[1].QuerySelectorAll("td")[4]));
    }

    /// <summary>
    /// Trocar para "Últimos 10" na aba de perfis mantém a aba e recalcula o observado nos 10 concursos.
    /// </summary>
    [Fact]
    public void TrocarAJanelaMantemAAbaDePerfis()
    {
        this.AdicionarHistoricoPadrao();
        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina = this.RenderizarPagina();
        Aba(pagina, "Por perfil do concurso").Click();

        Chip(pagina, "Últimos 10").Click();

        // Dos concursos 3799 a 3808, só 3803 a 3805 têm as dezenas 11 a 25 (soma 270).
        pagina.WaitForAssertion(() => Assert.Equal(
            "30,00%",
            Texto(LinhasDoCartao(pagina, "Soma das dezenas")[^1].QuerySelectorAll("td")[2])));
        Assert.Equal("Por perfil do concurso", Texto(pagina.Find(".abas .mud-tab-active")));
        Assert.Equal("70,00%", Texto(LinhasDoCartao(pagina, "Soma das dezenas")[0].QuerySelectorAll("td")[2]));
    }

    /// <summary>
    /// Texto do elemento sem espaços nas pontas.
    /// </summary>
    /// <param name="elemento">Elemento lido.</param>
    /// <returns>Texto normalizado.</returns>
    private static string Texto(IElement elemento)
    {
        return elemento.TextContent.Trim();
    }

    /// <summary>
    /// Encontra o chip de janela pelo texto.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="texto">Texto do chip.</param>
    /// <returns>Elemento do chip.</returns>
    private static IElement Chip(IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina, string texto)
    {
        return pagina.FindAll(".janelas .mud-chip").Single(chip => Texto(chip) == texto);
    }

    /// <summary>
    /// Encontra a aba pelo texto.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="texto">Texto da aba.</param>
    /// <returns>Elemento da aba.</returns>
    private static IElement Aba(IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina, string texto)
    {
        return pagina.FindAll(".abas .mud-tab").Single(aba => Texto(aba) == texto);
    }

    /// <summary>
    /// Linhas do cartão de perfil com o título informado.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="titulo">Título do cartão.</param>
    /// <returns>Linhas da tabela do cartão.</returns>
    private static IReadOnlyList<IElement> LinhasDoCartao(
        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina,
        string titulo)
    {
        IElement cartao = pagina.FindAll(".cartao-perfil").Single(item => Texto(item.QuerySelector("h2")!) == titulo);

        return cartao.QuerySelectorAll("tbody tr").ToList();
    }

    /// <summary>
    /// Encontra o botão de métrica do mapa pelo texto.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <param name="texto">Texto do botão.</param>
    /// <returns>Elemento do botão.</returns>
    private static IElement BotaoMetrica(IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina, string texto)
    {
        return pagina.FindAll(".metricas .mud-toggle-item").Single(botao => Texto(botao) == texto);
    }

    /// <summary>
    /// Monta o histórico do exemplo da spec: concursos 1 a 3808, com a dezena 20 (e as demais de 11 a 25) em 2.312
    /// deles, pela última vez no 3805; os outros concursos têm as dezenas 01 a 15, inclusive o último.
    /// </summary>
    private void AdicionarHistoricoPadrao()
    {
        this.historico.AddRange(Enumerable.Range(1, 3808).Select(numero =>
        {
            bool comDezena20 = numero <= 2309 || numero is >= 3803 and <= 3805;
            return new ResultadoSorteio(numero, comDezena20 ? De11a25 : De01a15);
        }));
    }

    /// <summary>
    /// Renderiza a página na cultura pt-BR, a mesma da aplicação.
    /// </summary>
    /// <returns>Página renderizada.</returns>
    private IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> RenderizarPagina()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

        IRenderedComponent<Lotofacil.Web.Components.Pages.Estatisticas> pagina =
            this.Render<Lotofacil.Web.Components.Pages.Estatisticas>();
        pagina.WaitForAssertion(() => Assert.True(pagina.FindAll(".intervalo").Count > 0));

        return pagina;
    }
}
