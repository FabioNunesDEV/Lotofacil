// <copyright file="TeimosinhasTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Aplicacao.Configuracoes;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Aplicacao.Teimosinhas;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Configuracoes;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;
using Lotofacil.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da página Teimosinhas.
/// </summary>
public class TeimosinhasTestes : ComponenteTesteBase
{
    /// <summary>
    /// Teimosinhas gravadas em memória.
    /// </summary>
    private readonly TeimosinhaRepositorioFalso teimosinhas = new();

    /// <summary>
    /// Registra os serviços da página, com o preço de R$ 3,50.
    /// </summary>
    public TeimosinhasTestes()
    {
        IJogoRepositorio jogos = Substitute.For<IJogoRepositorio>();
        this.Services.AddSingleton(new TeimosinhaServico(
            this.teimosinhas,
            jogos,
            new JogosServico(jogos, Substitute.For<IConcursoRepositorio>())));
        this.Services.AddSingleton(new ConfiguracoesServico(Substitute.For<IConfiguracaoApostasRepositorio>(), 3.50m));
    }

    /// <summary>
    /// Sem Teimosinhas ativas, a página mostra a mensagem própria.
    /// </summary>
    [Fact]
    public void SemAtivasMostraMensagem()
    {
        IRenderedComponent<Teimosinhas> pagina = this.RenderizarPagina();

        Assert.Equal("Teimosinhas", Texto(pagina.Find("h1")));
        pagina.WaitForAssertion(() => Assert.Equal(
            "Nenhuma Teimosinha ativa. Ligue “Teimosinha” ao salvar jogos em Meus jogos ou em Gerar jogos.",
            Texto(pagina.Find(".sem-registros"))));
    }

    /// <summary>
    /// Cada ativa mostra o nome, desde quando vale, os jogos com o custo por concurso e as dezenas.
    /// </summary>
    [Fact]
    public void ListaAsAtivas()
    {
        this.Gravar("Minhas fixas", (Enumerable.Range(1, 15), 3813), (Enumerable.Range(1, 16), 3815));
        this.Gravar("Antiga", (Enumerable.Range(10, 15), 3800)).Remover();

        IRenderedComponent<Teimosinhas> pagina = this.RenderizarPagina();

        IElement cartao = Assert.Single(pagina.FindAll(".teimosinha"));
        Assert.Equal("Minhas fixas", Texto(cartao.QuerySelector(".nome-teimosinha")!));
        Assert.Equal("Desde o concurso 3813", Texto(cartao.QuerySelector(".desde")!));
        Assert.Equal("2 jogos · R$ 59,50 por concurso", Texto(cartao.QuerySelector(".custo")!));
        Assert.Equal(2, cartao.QuerySelectorAll(".jogo-teimosinha").Length);
        Assert.Equal("16 dezenas", Texto(cartao.QuerySelectorAll(".jogo-teimosinha")[1].QuerySelector(".quantidade-dezenas")!));
    }

    /// <summary>
    /// Remover pede confirmação; confirmada, a Teimosinha some da lista e fica inativa.
    /// </summary>
    [Fact]
    public void RemoveComConfirmacao()
    {
        Teimosinha teimosinha = this.Gravar("Minhas fixas", (Enumerable.Range(1, 15), 3813));
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Teimosinhas> pagina = this.RenderizarPagina();

        pagina.Find(".botao-remover").Click();

        dialogos.WaitForAssertion(() => Assert.Contains(
            "Remover a Teimosinha Minhas fixas? Os jogos já criados continuam em Meus jogos.",
            Texto(dialogos.Find(".mud-dialog"))));
        dialogos.FindAll(".mud-dialog button").Single(botao => Texto(botao) == "Remover").Click();

        mensagens.WaitForAssertion(() => Assert.Contains(
            "Teimosinha removida.",
            mensagens.FindAll(".mud-snackbar-content-message").Select(Texto)));
        pagina.WaitForAssertion(() => Assert.Empty(pagina.FindAll(".teimosinha")));
        Assert.False(teimosinha.Ativa);
    }

    /// <summary>
    /// Cancelar a confirmação não muda nada.
    /// </summary>
    [Fact]
    public void CancelarMantemATeimosinha()
    {
        Teimosinha teimosinha = this.Gravar("Minhas fixas", (Enumerable.Range(1, 15), 3813));
        IRenderedComponent<MudDialogProvider> dialogos = this.Render<MudDialogProvider>();
        IRenderedComponent<Teimosinhas> pagina = this.RenderizarPagina();

        pagina.Find(".botao-remover").Click();
        dialogos.WaitForAssertion(() => Assert.NotEmpty(dialogos.FindAll(".mud-dialog")));
        dialogos.FindAll(".mud-dialog button").Single(botao => Texto(botao) == "Cancelar").Click();

        dialogos.WaitForAssertion(() => Assert.Empty(dialogos.FindAll(".mud-dialog")));
        Assert.True(teimosinha.Ativa);
        Assert.Single(pagina.FindAll(".teimosinha"));
    }

    /// <summary>
    /// Texto do elemento sem espaços nas pontas e com espaço comum no lugar do não separável.
    /// </summary>
    /// <param name="elemento">Elemento lido.</param>
    /// <returns>Texto normalizado.</returns>
    private static string Texto(IElement elemento)
    {
        return elemento.TextContent.Replace(' ', ' ').Trim();
    }

    /// <summary>
    /// Grava uma Teimosinha ativa com os jogos informados.
    /// </summary>
    /// <param name="nome">Nome da Teimosinha.</param>
    /// <param name="jogos">Dezenas e início de cada jogo.</param>
    /// <returns>Teimosinha gravada.</returns>
    private Teimosinha Gravar(string nome, params (IEnumerable<int> Dezenas, int Inicio)[] jogos)
    {
        var teimosinha = new Teimosinha(nome, jogos[0].Inicio);
        foreach ((IEnumerable<int> dezenas, int inicio) in jogos)
        {
            teimosinha.Acrescentar(dezenas, inicio);
        }

        this.teimosinhas.SalvarAsync(teimosinha, Xunit.TestContext.Current.CancellationToken).GetAwaiter().GetResult();
        return teimosinha;
    }

    /// <summary>
    /// Renderiza a página na cultura pt-BR, a mesma da aplicação.
    /// </summary>
    /// <returns>Página renderizada.</returns>
    private IRenderedComponent<Teimosinhas> RenderizarPagina()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

        return this.Render<Teimosinhas>();
    }
}
