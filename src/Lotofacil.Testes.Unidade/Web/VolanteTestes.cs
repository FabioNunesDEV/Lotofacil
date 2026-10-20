// <copyright file="VolanteTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using AngleSharp.Dom;
using Bunit;
using Lotofacil.Web.Components.Jogos;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes do volante: marcação com limite e situação das dezenas na conferência.
/// </summary>
public class VolanteTestes : ComponenteTesteBase
{
    /// <summary>
    /// Clicar marca e desmarca a dezena, informando as dezenas marcadas.
    /// </summary>
    [Fact]
    public void MarcaEDesmarca()
    {
        IReadOnlyCollection<int> marcadas = [];
        IRenderedComponent<Volante> volante = this.Render<Volante>(parametros => parametros
            .Add(componente => componente.Marcadas, marcadas)
            .Add(componente => componente.MarcadasChanged, novas => marcadas = novas));

        Dezena(volante, 7).Click();
        Assert.Equal([7], marcadas);

        volante.Render(parametros => parametros.Add(componente => componente.Marcadas, marcadas));
        Assert.Contains("marcada", Dezena(volante, 7).ClassList);

        Dezena(volante, 7).Click();
        Assert.Empty(marcadas);
    }

    /// <summary>
    /// Com 15 marcadas, as demais dezenas ficam bloqueadas e as marcadas podem ser desmarcadas.
    /// </summary>
    [Fact]
    public void BloqueiaAoAtingirOLimite()
    {
        IReadOnlyCollection<int> marcadas = Enumerable.Range(1, 15).ToHashSet();
        IRenderedComponent<Volante> volante = this.Render<Volante>(parametros => parametros
            .Add(componente => componente.Marcadas, marcadas)
            .Add(componente => componente.MarcadasChanged, novas => marcadas = novas));

        Assert.True(Dezena(volante, 16).HasAttribute("disabled"));
        Assert.False(Dezena(volante, 15).HasAttribute("disabled"));

        Dezena(volante, 15).Click();
        Assert.Equal(14, marcadas.Count);
    }

    /// <summary>
    /// Na conferência, cada dezena mostra se acertou, se foi marcada sem sair ou se saiu sem ser marcada.
    /// </summary>
    [Fact]
    public void MostraASituacaoNaConferencia()
    {
        IRenderedComponent<Volante> volante = this.Render<Volante>(parametros => parametros
            .Add(componente => componente.Marcadas, [1, 2])
            .Add(componente => componente.Sorteadas, [2, 3]));

        Assert.Empty(volante.FindAll("button"));
        Assert.Contains("erro", Dezena(volante, 1).ClassList);
        Assert.Contains("acerto", Dezena(volante, 2).ClassList);
        Assert.Contains("nao-marcada", Dezena(volante, 3).ClassList);
        Assert.Equal("dezena", Dezena(volante, 4).ClassName);
    }

    /// <summary>
    /// Encontra a dezena no volante.
    /// </summary>
    /// <param name="volante">Volante renderizado.</param>
    /// <param name="dezena">Dezena procurada.</param>
    /// <returns>Elemento da dezena.</returns>
    private static IElement Dezena(IRenderedComponent<Volante> volante, int dezena)
    {
        return volante.FindAll(".dezena").Single(elemento => elemento.TextContent.Trim() == dezena.ToString("00"));
    }
}
