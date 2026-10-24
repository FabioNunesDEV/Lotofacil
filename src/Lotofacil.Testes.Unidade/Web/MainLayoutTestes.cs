// <copyright file="MainLayoutTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Bunit;
using Lotofacil.Web.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes do layout principal.
/// </summary>
public class MainLayoutTestes : ComponenteTesteBase
{
    /// <summary>
    /// A barra superior mostra o título da aplicação.
    /// </summary>
    [Fact]
    public void ExibeTituloNaBarraSuperior()
    {
        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        Assert.Contains("Lotofácil", layout.Find(".mud-appbar").TextContent);
    }

    /// <summary>
    /// O menu lateral mostra o item Painel, selecionado quando a rota atual é a raiz.
    /// </summary>
    [Fact]
    public void ExibeItemPainelSelecionadoNaRaiz()
    {
        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        var itemPainel = layout.Find(".mud-drawer a.mud-nav-link");
        Assert.Equal("Painel", itemPainel.TextContent.Trim());
        Assert.Contains("active", itemPainel.ClassList);
    }

    /// <summary>
    /// O item Resultados vem logo abaixo de Painel e fica selecionado na rota /resultados.
    /// </summary>
    [Fact]
    public void ExibeItemResultadosSelecionadoNaRotaResultados()
    {
        this.Services.GetRequiredService<NavigationManager>().NavigateTo("resultados");

        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        var itens = layout.FindAll(".mud-drawer a.mud-nav-link");
        Assert.Equal(["Painel", "Resultados", "Estatísticas", "Gerar jogos", "Meus jogos", "Teimosinhas", "Configurações"], itens.Select(item => item.TextContent.Trim()));
        Assert.Equal("resultados", itens[1].GetAttribute("href"));
        Assert.Contains("active", itens[1].ClassList);
        Assert.DoesNotContain("active", itens[0].ClassList);
    }

    /// <summary>
    /// O item Gerar jogos vem logo abaixo de Estatísticas e fica selecionado na rota /gerar-jogos.
    /// </summary>
    [Fact]
    public void ExibeItemGerarJogosSelecionadoNaRotaGerarJogos()
    {
        this.Services.GetRequiredService<NavigationManager>().NavigateTo("gerar-jogos");

        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        var itens = layout.FindAll(".mud-drawer a.mud-nav-link");
        Assert.Equal("Gerar jogos", itens[3].TextContent.Trim());
        Assert.Equal("gerar-jogos", itens[3].GetAttribute("href"));
        Assert.Contains("active", itens[3].ClassList);
        Assert.DoesNotContain("active", itens[2].ClassList);
    }

    /// <summary>
    /// O item Meus jogos vem logo abaixo de Gerar jogos e fica selecionado na rota /meus-jogos.
    /// </summary>
    [Fact]
    public void ExibeItemMeusJogosSelecionadoNaRotaMeusJogos()
    {
        this.Services.GetRequiredService<NavigationManager>().NavigateTo("meus-jogos");

        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        var itens = layout.FindAll(".mud-drawer a.mud-nav-link");
        Assert.Equal("Meus jogos", itens[4].TextContent.Trim());
        Assert.Equal("meus-jogos", itens[4].GetAttribute("href"));
        Assert.Contains("active", itens[4].ClassList);
        Assert.DoesNotContain("active", itens[3].ClassList);
    }

    /// <summary>
    /// O item Teimosinhas vem logo abaixo de Meus jogos e fica selecionado na rota /teimosinhas.
    /// </summary>
    [Fact]
    public void ExibeItemTeimosinhasSelecionadoNaRotaTeimosinhas()
    {
        this.Services.GetRequiredService<NavigationManager>().NavigateTo("teimosinhas");

        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        var itens = layout.FindAll(".mud-drawer a.mud-nav-link");
        Assert.Equal("Teimosinhas", itens[5].TextContent.Trim());
        Assert.Equal("teimosinhas", itens[5].GetAttribute("href"));
        Assert.Contains("active", itens[5].ClassList);
        Assert.DoesNotContain("active", itens[4].ClassList);
    }

    /// <summary>
    /// O item Configurações é o último, logo abaixo de Teimosinhas, e fica selecionado na rota /configuracoes.
    /// </summary>
    [Fact]
    public void ExibeItemConfiguracoesSelecionadoNaRotaConfiguracoes()
    {
        this.Services.GetRequiredService<NavigationManager>().NavigateTo("configuracoes");

        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        var itens = layout.FindAll(".mud-drawer a.mud-nav-link");
        Assert.Equal(7, itens.Count);
        Assert.Equal("Configurações", itens[6].TextContent.Trim());
        Assert.Equal("configuracoes", itens[6].GetAttribute("href"));
        Assert.Contains("active", itens[6].ClassList);
        Assert.DoesNotContain("active", itens[5].ClassList);
    }

    /// <summary>
    /// O item Estatísticas vem logo abaixo de Resultados e fica selecionado na rota /estatisticas.
    /// </summary>
    [Fact]
    public void ExibeItemEstatisticasSelecionadoNaRotaEstatisticas()
    {
        this.Services.GetRequiredService<NavigationManager>().NavigateTo("estatisticas");

        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        var itens = layout.FindAll(".mud-drawer a.mud-nav-link");
        Assert.Equal("Estatísticas", itens[2].TextContent.Trim());
        Assert.Equal("estatisticas", itens[2].GetAttribute("href"));
        Assert.Contains("active", itens[2].ClassList);
        Assert.DoesNotContain("active", itens[1].ClassList);
    }

    /// <summary>
    /// Cada acionamento do botão de menu inverte o estado do menu lateral; o segundo volta ao estado inicial.
    /// </summary>
    [Fact]
    public void BotaoDeMenuAlternaMenuLateral()
    {
        // O estado inicial depende da largura da tela, que o MudBlazor obtém por JavaScript.
        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();
        var botaoMenu = layout.Find("button[aria-label='Abrir ou fechar menu']");
        bool abertoNoInicio = MenuLateralAberto(layout);

        botaoMenu.Click();
        Assert.Equal(!abertoNoInicio, MenuLateralAberto(layout));

        botaoMenu.Click();
        Assert.Equal(abertoNoInicio, MenuLateralAberto(layout));
    }

    /// <summary>
    /// O menu lateral usa a variante mini, que recolhe deixando só os ícones em telas largas.
    /// </summary>
    [Fact]
    public void MenuLateralUsaAVarianteMini()
    {
        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        Assert.Equal(DrawerVariant.Mini, layout.FindComponent<MudDrawer>().Instance.Variant);
    }

    /// <summary>
    /// Cada item do menu tem a dica com o nome da página, que só funciona com o menu recolhido.
    /// </summary>
    [Fact]
    public void DicaComONomeSoNoMenuRecolhido()
    {
        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();
        IReadOnlyList<IRenderedComponent<MudTooltip>> dicas = layout.FindComponents<MudTooltip>();
        bool abertoNoInicio = MenuLateralAberto(layout);

        Assert.Equal(
            ["Painel", "Resultados", "Estatísticas", "Gerar jogos", "Meus jogos", "Teimosinhas", "Configurações"],
            dicas.Select(dica => dica.Instance.Text));
        Assert.All(dicas, dica => Assert.Equal(abertoNoInicio, dica.Instance.Disabled));

        layout.Find("button[aria-label='Abrir ou fechar menu']").Click();

        Assert.All(layout.FindComponents<MudTooltip>(), dica => Assert.Equal(!abertoNoInicio, dica.Instance.Disabled));
    }

    /// <summary>
    /// O conteúdo da página é exibido dentro do layout.
    /// </summary>
    [Fact]
    public void ExibeConteudoDaPagina()
    {
        IRenderedComponent<MainLayout> layout = this.RenderizarLayout();

        Assert.Contains("conteúdo da página", layout.Find(".mud-main-content").TextContent);
    }

    /// <summary>
    /// Indica se o menu lateral está aberto no layout renderizado.
    /// </summary>
    /// <param name="layout">Layout renderizado.</param>
    /// <returns><c>true</c> se o menu lateral estiver aberto.</returns>
    private static bool MenuLateralAberto(IRenderedComponent<MainLayout> layout)
    {
        return layout.Find(".mud-drawer").ClassList.Contains("mud-drawer--open");
    }

    /// <summary>
    /// Renderiza o layout com um conteúdo de página fictício.
    /// </summary>
    /// <returns>Layout renderizado.</returns>
    private IRenderedComponent<MainLayout> RenderizarLayout()
    {
        RenderFragment conteudo = construtor => construtor.AddContent(0, "conteúdo da página");

        return this.Render<MainLayout>(parametros => parametros.Add(layout => layout.Body, conteudo));
    }
}
