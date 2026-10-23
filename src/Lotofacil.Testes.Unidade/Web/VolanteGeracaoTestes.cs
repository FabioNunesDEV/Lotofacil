// <copyright file="VolanteGeracaoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using AngleSharp.Dom;
using Bunit;
using Lotofacil.Web.Components.Geracao;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes do volante de dezenas fixas e excluídas.
/// </summary>
public class VolanteGeracaoTestes : ComponenteTesteBase
{
    /// <summary>
    /// Três cliques levam a dezena de livre a fixa, a excluída e de volta a livre.
    /// </summary>
    [Fact]
    public void AlternaLivreFixaExcluida()
    {
        IReadOnlyCollection<int> fixas = [];
        IReadOnlyCollection<int> excluidas = [];
        IRenderedComponent<VolanteGeracao> volante = this.Render<VolanteGeracao>(parametros => parametros
            .Add(componente => componente.Fixas, fixas)
            .Add(componente => componente.FixasChanged, novas => fixas = novas)
            .Add(componente => componente.Excluidas, excluidas)
            .Add(componente => componente.ExcluidasChanged, novas => excluidas = novas));

        Assert.Equal("07 livre", Dezena(volante, 7).GetAttribute("aria-label"));

        Dezena(volante, 7).Click();
        Assert.Equal([7], fixas);
        Assert.Empty(excluidas);
        Repassar(volante, fixas, excluidas);
        Assert.Contains("fixa", Dezena(volante, 7).ClassList);
        Assert.Equal("07 fixa", Dezena(volante, 7).GetAttribute("aria-label"));

        Dezena(volante, 7).Click();
        Assert.Empty(fixas);
        Assert.Equal([7], excluidas);
        Repassar(volante, fixas, excluidas);
        Assert.Contains("excluida", Dezena(volante, 7).ClassList);
        Assert.Equal("07 excluída", Dezena(volante, 7).GetAttribute("aria-label"));

        Dezena(volante, 7).Click();
        Assert.Empty(fixas);
        Assert.Empty(excluidas);
    }

    /// <summary>
    /// O clique numa dezena não muda as outras.
    /// </summary>
    [Fact]
    public void MantemAsOutrasDezenas()
    {
        IReadOnlyCollection<int> fixas = [3, 9];
        IReadOnlyCollection<int> excluidas = [20];
        IRenderedComponent<VolanteGeracao> volante = this.Render<VolanteGeracao>(parametros => parametros
            .Add(componente => componente.Fixas, fixas)
            .Add(componente => componente.FixasChanged, novas => fixas = novas)
            .Add(componente => componente.Excluidas, excluidas)
            .Add(componente => componente.ExcluidasChanged, novas => excluidas = novas));

        Dezena(volante, 9).Click();

        Assert.Equal([3], fixas.Order());
        Assert.Equal([9, 20], excluidas.Order());
    }

    /// <summary>
    /// Encontra o botão de uma dezena.
    /// </summary>
    /// <param name="volante">Volante renderizado.</param>
    /// <param name="dezena">Dezena procurada.</param>
    /// <returns>O botão.</returns>
    private static IElement Dezena(IRenderedComponent<VolanteGeracao> volante, int dezena)
    {
        return volante.FindAll("button").Single(botao => botao.TextContent.Trim() == dezena.ToString("00"));
    }

    /// <summary>
    /// Repassa ao volante as coleções atualizadas, como a página faz.
    /// </summary>
    /// <param name="volante">Volante renderizado.</param>
    /// <param name="fixas">Fixas atuais.</param>
    /// <param name="excluidas">Excluídas atuais.</param>
    private static void Repassar(
        IRenderedComponent<VolanteGeracao> volante,
        IReadOnlyCollection<int> fixas,
        IReadOnlyCollection<int> excluidas)
    {
        volante.Render(parametros => parametros
            .Add(componente => componente.Fixas, fixas)
            .Add(componente => componente.Excluidas, excluidas));
    }
}
