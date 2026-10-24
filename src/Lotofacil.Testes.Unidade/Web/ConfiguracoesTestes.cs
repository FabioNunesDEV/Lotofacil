// <copyright file="ConfiguracoesTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Aplicacao.Configuracoes;
using Lotofacil.Dominio.Configuracoes;
using Lotofacil.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da página Configurações.
/// </summary>
public class ConfiguracoesTestes : ComponenteTesteBase
{
    /// <summary>
    /// Repositório falso da configuração das apostas, sem nada gravado.
    /// </summary>
    private readonly IConfiguracaoApostasRepositorio repositorio = Substitute.For<IConfiguracaoApostasRepositorio>();

    /// <summary>
    /// Serviço das configurações com o preço padrão de R$ 3,50.
    /// </summary>
    private readonly ConfiguracoesServico servico;

    /// <summary>
    /// Registra o serviço das configurações.
    /// </summary>
    public ConfiguracoesTestes()
    {
        this.servico = new ConfiguracoesServico(this.repositorio, 3.50m);
        this.Services.AddSingleton(this.servico);
    }

    /// <summary>
    /// Ao abrir, a página mostra o título e o preço em uso no campo.
    /// </summary>
    [Fact]
    public void AbreComOPrecoEmUso()
    {
        IRenderedComponent<Configuracoes> pagina = this.RenderizarPagina();

        Assert.Equal("Configurações", Texto(pagina.Find("h1")));
        Assert.Equal("3,50", pagina.Find(".campo-preco input").GetAttribute("value"));
    }

    /// <summary>
    /// Com R$ 3,50, a tabela mostra as apostas equivalentes e o valor de 15 a 20 dezenas.
    /// </summary>
    [Fact]
    public void MostraATabelaDePrecos()
    {
        IRenderedComponent<Configuracoes> pagina = this.RenderizarPagina();

        Assert.Equal(
            [
                ["15", "1", "R$ 3,50"],
                ["16", "16", "R$ 56,00"],
                ["17", "136", "R$ 476,00"],
                ["18", "816", "R$ 2.856,00"],
                ["19", "3.876", "R$ 13.566,00"],
                ["20", "15.504", "R$ 54.264,00"],
            ],
            Linhas(pagina));
    }

    /// <summary>
    /// A tabela acompanha o preço digitado antes de salvar, sem mudar o preço em uso.
    /// </summary>
    [Fact]
    public void TabelaAcompanhaOPrecoDigitado()
    {
        IRenderedComponent<Configuracoes> pagina = this.RenderizarPagina();

        pagina.Find(".campo-preco input").Change("4");

        List<string[]> linhas = Linhas(pagina);
        Assert.Equal("R$ 64,00", linhas[1][2]);
        Assert.Equal("R$ 62.016,00", linhas[5][2]);
        Assert.Equal(3.50m, this.servico.PrecoApostaMinima);
    }

    /// <summary>
    /// Com o campo vazio, a coluna Valor mostra "—" e o campo mostra o aviso.
    /// </summary>
    [Fact]
    public void CampoVazioMostraTracoEAviso()
    {
        IRenderedComponent<Configuracoes> pagina = this.RenderizarPagina();

        pagina.Find(".campo-preco input").Change(string.Empty);

        Assert.All(Linhas(pagina), linha => Assert.Equal("—", linha[2]));
        Assert.Contains(Configuracoes.MensagemPrecoInvalido, Texto(pagina.Find(".campo-preco")));
    }

    /// <summary>
    /// Com preço inválido, salvar mostra o aviso e não grava nada.
    /// </summary>
    /// <param name="digitado">Texto digitado no campo.</param>
    /// <returns>Tarefa do teste.</returns>
    [Theory]
    [InlineData("0")]
    [InlineData("100,01")]
    [InlineData("3,555")]
    [InlineData("")]
    public async Task RecusaPrecoInvalido(string digitado)
    {
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Configuracoes> pagina = this.RenderizarPagina();

        pagina.Find(".campo-preco input").Change(digitado);
        pagina.Find(".botao-salvar").Click();

        Assert.Contains(Configuracoes.MensagemPrecoInvalido, Texto(pagina.Find(".campo-preco")));
        Assert.Empty(mensagens.FindAll(".mud-snackbar-content-message"));
        Assert.Equal(3.50m, this.servico.PrecoApostaMinima);
        await this.repositorio.DidNotReceiveWithAnyArgs().SalvarAsync(default!, Xunit.TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Com preço válido, salvar grava, avisa e muda o preço em uso.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SalvaOPreco()
    {
        IRenderedComponent<MudSnackbarProvider> mensagens = this.Render<MudSnackbarProvider>();
        IRenderedComponent<Configuracoes> pagina = this.RenderizarPagina();

        pagina.Find(".campo-preco input").Change("4,00");
        pagina.Find(".botao-salvar").Click();

        mensagens.WaitForAssertion(() => Assert.Equal(
            [Configuracoes.MensagemSalvo],
            mensagens.FindAll(".mud-snackbar-content-message").Select(Texto)));
        Assert.Equal(4.00m, this.servico.PrecoApostaMinima);
        Assert.DoesNotContain(Configuracoes.MensagemPrecoInvalido, Texto(pagina.Find(".campo-preco")));
        await this.repositorio.Received(1).SalvarAsync(
            Arg.Is<ConfiguracaoApostas>(configuracao => configuracao.PrecoApostaMinima == 4.00m),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Texto de um elemento, sem espaços nas pontas e com espaço comum no lugar do não separável.
    /// </summary>
    /// <param name="elemento">Elemento.</param>
    /// <returns>Texto do elemento.</returns>
    private static string Texto(IElement elemento)
    {
        return elemento.TextContent.Replace(' ', ' ').Trim();
    }

    /// <summary>
    /// Células de cada linha da tabela de preços.
    /// </summary>
    /// <param name="pagina">Página renderizada.</param>
    /// <returns>Textos das células, linha a linha.</returns>
    private static List<string[]> Linhas(IRenderedComponent<Configuracoes> pagina)
    {
        return pagina.FindAll(".tabela-precos tbody tr")
            .Select(linha => linha.QuerySelectorAll("td").Select(Texto).ToArray())
            .ToList();
    }

    /// <summary>
    /// Renderiza a página na cultura pt-BR, a mesma da aplicação.
    /// </summary>
    /// <returns>Página renderizada.</returns>
    private IRenderedComponent<Configuracoes> RenderizarPagina()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

        return this.Render<Configuracoes>();
    }
}
