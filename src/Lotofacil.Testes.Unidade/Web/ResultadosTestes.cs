// <copyright file="ResultadosTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da página Resultados, com o serviço real sobre um repositório falso em memória.
/// </summary>
public class ResultadosTestes : ComponenteTesteBase
{
    /// <summary>
    /// Concursos devolvidos pelo repositório falso.
    /// </summary>
    private readonly List<Concurso> concursos = [];

    /// <summary>
    /// Registra o serviço de consulta com um repositório que responde a partir da lista <see cref="concursos"/>.
    /// </summary>
    public ResultadosTestes()
    {
        IConcursoRepositorio repositorio = Substitute.For<IConcursoRepositorio>();
        repositorio.ListarAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.concursos
                .OrderByDescending(concurso => concurso.Numero)
                .Skip(chamada.ArgAt<int>(0))
                .Take(chamada.ArgAt<int>(1))
                .ToList());
        repositorio.ContarAsync(Arg.Any<CancellationToken>()).Returns(_ => this.concursos.Count);
        repositorio.ObterAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => this.concursos.Find(concurso => concurso.Numero == chamada.ArgAt<int>(0)));

        this.Services.AddSingleton(new ConsultaConcursosServico(repositorio));
    }

    /// <summary>
    /// A página exibe o seu título.
    /// </summary>
    [Fact]
    public void ExibeTitulo()
    {
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        Assert.Equal("Resultados", pagina.Find("h1").TextContent.Trim());
    }

    /// <summary>
    /// A primeira página mostra os 10 concursos mais recentes e o total da base.
    /// </summary>
    [Fact]
    public void PrimeiraPaginaMostraOsDezMaisRecentes()
    {
        this.AdicionarConcursos(23);

        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForAssertion(() => Assert.Equal(Enumerable.Range(14, 10).Reverse(), NumerosExibidos(pagina)));
        Assert.Contains(pagina.FindAll(".mud-table-pagination-caption"), legenda => Texto(legenda) == "1–10 de 23");
    }

    /// <summary>
    /// Avançar a página mostra os 10 concursos seguintes.
    /// </summary>
    [Fact]
    public void ProximaPaginaMostraOsConcursosSeguintes()
    {
        this.AdicionarConcursos(23);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));

        pagina.FindAll(".mud-table-pagination-actions button")[2].Click();

        pagina.WaitForAssertion(() => Assert.Equal(Enumerable.Range(4, 10).Reverse(), NumerosExibidos(pagina)));
    }

    /// <summary>
    /// A linha mostra data, dezenas com dois dígitos, ganhadores, prêmio em reais e a situação "Saiu".
    /// </summary>
    [Fact]
    public void LinhaDeConcursoComGanhador()
    {
        this.concursos.Add(ConcursoFabrica.Criar(
            numero: 3804,
            rateios: ConcursoFabrica.CriarRateios(ganhadores15: 2, premio15: 801_455.92m)));

        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        IElement linha = pagina.WaitForElement("tr.linha-clicavel");
        IHtmlCollection<IElement> celulas = linha.QuerySelectorAll("td");
        Assert.Equal("3804", Texto(celulas[0]));
        Assert.Equal("17/10/2026", Texto(celulas[1]));
        Assert.Equal(
            ConcursoFabrica.DezenasPadrao.Select(dezena => dezena.ToString("00")),
            celulas[2].QuerySelectorAll(".bola").Select(Texto));
        Assert.Equal("2", Texto(celulas[3]));
        Assert.Equal("R$ 801.455,92", Texto(celulas[4]));
        Assert.Equal("Saiu", Texto(celulas[5]));
    }

    /// <summary>
    /// Sem ganhador de 15 acertos, o prêmio aparece como "—" e a situação como "Acumulou".
    /// </summary>
    [Fact]
    public void LinhaDeConcursoSemGanhador()
    {
        this.concursos.Add(ConcursoFabrica.Criar(
            rateios: ConcursoFabrica.CriarRateios(ganhadores15: 0, premio15: 0m)));

        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        IHtmlCollection<IElement> celulas = pagina.WaitForElement("tr.linha-clicavel").QuerySelectorAll("td");
        Assert.Equal("0", Texto(celulas[3]));
        Assert.Equal("—", Texto(celulas[4]));
        Assert.Equal("Acumulou", Texto(celulas[5]));
    }

    /// <summary>
    /// Clicar na linha abre o rateio das faixas de 15 a 11; clicar de novo fecha.
    /// </summary>
    [Fact]
    public void CliqueNaLinhaAbreEFechaODetalhe()
    {
        this.concursos.Add(ConcursoFabrica.Criar());
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForElement("tr.linha-clicavel").Click();

        IElement detalhe = pagina.Find(".detalhe-concurso");
        List<string[]> linhasRateio = detalhe.QuerySelectorAll("tr")
            .Select(linha => linha.QuerySelectorAll("td").Select(Texto).ToArray())
            .Where(celulas => celulas.Length > 0)
            .ToList();
        Assert.Equal(
            [
                ["15 acertos", "1", "R$ 1.712.408,63"],
                ["14 acertos", "287", "R$ 1.502,34"],
                ["13 acertos", "9.874", "R$ 35,00"],
                ["12 acertos", "121.045", "R$ 14,00"],
                ["11 acertos", "653.211", "R$ 7,00"],
            ],
            linhasRateio);
        Assert.Contains("R$ 24.873.112,50", Texto(detalhe.QuerySelector(".arrecadacao")!));

        pagina.Find("tr.linha-clicavel").Click();

        Assert.Empty(pagina.FindAll(".detalhe-concurso"));
    }

    /// <summary>
    /// O detalhe mostra a ordem do sorteio quando ela é conhecida.
    /// </summary>
    [Fact]
    public void DetalheMostraOrdemDoSorteio()
    {
        int[] ordem = [13, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21, 4];
        this.concursos.Add(ConcursoFabrica.Criar(ordemSorteio: ordem));
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForElement("tr.linha-clicavel").Click();

        Assert.Equal(
            ordem.Select(dezena => dezena.ToString("00")),
            pagina.FindAll(".ordem-sorteio .bola").Select(Texto));
    }

    /// <summary>
    /// Concurso antigo: arrecadação "Não informada" e sem ordem do sorteio.
    /// </summary>
    [Fact]
    public void DetalheDeConcursoAntigo()
    {
        this.concursos.Add(ConcursoFabrica.Criar(numero: 1, arrecadacao: 0m, ordemSorteio: null));
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForElement("tr.linha-clicavel").Click();

        Assert.Contains("Não informada", Texto(pagina.Find(".arrecadacao")));
        Assert.Empty(pagina.FindAll(".ordem-sorteio"));
    }

    /// <summary>
    /// Buscar um número existente mostra só esse concurso.
    /// </summary>
    [Fact]
    public void BuscaMostraSomenteOConcursoEncontrado()
    {
        this.AdicionarConcursos(3807);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));

        pagina.Find(".campo-busca input").Change("3805");

        pagina.WaitForAssertion(() => Assert.Equal([3805], NumerosExibidos(pagina)));
    }

    /// <summary>
    /// Buscar um número que não está na base mostra a mensagem de não encontrado.
    /// </summary>
    [Fact]
    public void BuscaDeConcursoInexistenteMostraMensagem()
    {
        this.AdicionarConcursos(23);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));

        pagina.Find(".campo-busca input").Change("9999");

        pagina.WaitForAssertion(() => Assert.Equal(
            "Concurso 9999 não encontrado na base local",
            Texto(pagina.Find(".sem-registros"))));
        Assert.Empty(NumerosExibidos(pagina));
    }

    /// <summary>
    /// Limpar a busca volta à lista completa, a partir do mais recente.
    /// </summary>
    [Fact]
    public void LimparBuscaVoltaParaListaCompleta()
    {
        this.AdicionarConcursos(23);
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();
        pagina.WaitForAssertion(() => Assert.Equal(10, NumerosExibidos(pagina).Count));
        pagina.Find(".campo-busca input").Change("5");
        pagina.WaitForAssertion(() => Assert.Equal([5], NumerosExibidos(pagina)));

        pagina.Find(".campo-busca input").Change(string.Empty);

        pagina.WaitForAssertion(() => Assert.Equal(Enumerable.Range(14, 10).Reverse(), NumerosExibidos(pagina)));
    }

    /// <summary>
    /// Com a base vazia, a página mostra a mensagem própria no lugar da lista.
    /// </summary>
    [Fact]
    public void BaseVaziaMostraMensagem()
    {
        IRenderedComponent<Resultados> pagina = this.RenderizarPagina();

        pagina.WaitForAssertion(() => Assert.Equal(
            "Nenhum concurso gravado na base local.",
            Texto(pagina.Find(".sem-registros"))));
    }

    /// <summary>
    /// Números dos concursos nas linhas exibidas, de cima para baixo.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <returns>Números exibidos.</returns>
    private static List<int> NumerosExibidos(IRenderedComponent<Resultados> pagina)
    {
        return pagina.FindAll("tr.linha-clicavel")
            .Select(linha => int.Parse(Texto(linha.QuerySelector("td")!), CultureInfo.InvariantCulture))
            .ToList();
    }

    /// <summary>
    /// Texto do elemento sem espaços nas pontas e com espaço comum no lugar do espaço inseparável do "R$".
    /// </summary>
    /// <param name="elemento">Elemento lido.</param>
    /// <returns>Texto normalizado.</returns>
    private static string Texto(IElement elemento)
    {
        return elemento.TextContent.Replace(' ', ' ').Trim();
    }

    /// <summary>
    /// Adiciona os concursos de 1 até o número informado.
    /// </summary>
    /// <param name="ultimo">Número do último concurso.</param>
    private void AdicionarConcursos(int ultimo)
    {
        this.concursos.AddRange(Enumerable.Range(1, ultimo).Select(numero => ConcursoFabrica.Criar(numero: numero)));
    }

    /// <summary>
    /// Renderiza a página na cultura pt-BR, a mesma da aplicação.
    /// </summary>
    /// <returns>Página renderizada.</returns>
    private IRenderedComponent<Resultados> RenderizarPagina()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

        return this.Render<Resultados>();
    }
}
