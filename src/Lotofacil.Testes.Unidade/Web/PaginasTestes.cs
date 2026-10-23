// <copyright file="PaginasTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Bunit;
using Lotofacil.Web.Components.Pages;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes da página Página não encontrada.
/// </summary>
public class PaginasTestes : ComponenteTesteBase
{
    /// <summary>
    /// A página não encontrada exibe a mensagem e um link de volta para o Painel.
    /// </summary>
    [Fact]
    public void PaginaNaoEncontradaExibeMensagemELinkParaPainel()
    {
        IRenderedComponent<PaginaNaoEncontrada> pagina = this.Render<PaginaNaoEncontrada>();

        Assert.Equal("Página não encontrada", pagina.Find("h1").TextContent.Trim());
        var link = pagina.Find("a");
        Assert.Equal("/", link.GetAttribute("href"));
        Assert.Equal("Voltar para o Painel", link.TextContent.Trim());
    }
}
