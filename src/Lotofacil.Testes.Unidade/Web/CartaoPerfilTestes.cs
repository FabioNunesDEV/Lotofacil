// <copyright file="CartaoPerfilTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using AngleSharp.Dom;
using Bunit;
using Lotofacil.Dominio.Estatisticas.Perfis;
using Lotofacil.Web.Components.Estatisticas;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes do cartão de um perfil do concurso.
/// </summary>
public class CartaoPerfilTestes : ComponenteTesteBase
{
    /// <summary>
    /// Perfil de pares com três linhas: a maior porcentagem (50% observado) define a escala de 55%.
    /// </summary>
    private static readonly DistribuicaoPerfil Pares = new(
        TipoPerfil.Pares,
        [
            new FaixaPerfil(2, 4, AbertaAbaixo: true, AbertaAcima: false, Observado: 0, Teorico: 1.27m, Atraso: null),
            new FaixaPerfil(7, 7, AbertaAbaixo: false, AbertaAcima: false, Observado: 50, Teorico: 25, Atraso: 0),
            new FaixaPerfil(10, 12, AbertaAbaixo: false, AbertaAcima: true, Observado: 2.5m, Teorico: 2.87m, Atraso: 42),
        ]);

    /// <summary>
    /// O cartão mostra título, descrição e uma linha por faixa com rótulo, percentuais e atraso.
    /// </summary>
    [Fact]
    public void MostraAsLinhasDoPerfil()
    {
        IRenderedComponent<CartaoPerfil> cartao = this.Renderizar(Pares);

        Assert.Equal("Pares e ímpares", Texto(cartao.Find("h2")));
        Assert.Equal("12 pares e 13 ímpares no volante", Texto(cartao.Find(".descricao")));
        IReadOnlyList<IElement> linhas = cartao.FindAll("tbody tr");
        Assert.Equal(3, linhas.Count);
        Assert.Equal(["≤ 4 pares", string.Empty, "0,00%", "1,27%", "—"], Celulas(linhas[0]));
        Assert.Equal(["7 pares", string.Empty, "50,00%", "25,00%", "último"], Celulas(linhas[1]));
        Assert.Equal(["≥ 10 pares", string.Empty, "2,50%", "2,87%", "42"], Celulas(linhas[2]));
        Assert.Single(linhas[1].QuerySelectorAll(".ultimo"));
    }

    /// <summary>
    /// A barra tem a largura do observado e a marca na posição do teórico, na escala do cartão.
    /// </summary>
    [Fact]
    public void BarraUsaAEscalaDoCartao()
    {
        IRenderedComponent<CartaoPerfil> cartao = this.Renderizar(Pares);

        IElement barra = cartao.FindAll("tbody tr")[1].QuerySelector(".barra-perfil")!;

        // Escala = 50 × 1,1 = 55: 50 / 55 = 90,91% e 25 / 55 = 45,45%.
        Assert.Equal("width: 90.91%", barra.QuerySelector(".observado")!.GetAttribute("style"));
        Assert.Equal("left: 45.45%", barra.QuerySelector(".teorico")!.GetAttribute("style"));
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
    /// Textos das células de uma linha.
    /// </summary>
    /// <param name="linha">Linha da tabela.</param>
    /// <returns>Texto de cada célula.</returns>
    private static IEnumerable<string> Celulas(IElement linha)
    {
        return linha.QuerySelectorAll("td").Select(Texto);
    }

    /// <summary>
    /// Renderiza o cartão na cultura pt-BR, a mesma da aplicação.
    /// </summary>
    /// <param name="distribuicao">Perfil exibido.</param>
    /// <returns>Cartão renderizado.</returns>
    private IRenderedComponent<CartaoPerfil> Renderizar(DistribuicaoPerfil distribuicao)
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");

        return this.Render<CartaoPerfil>(parametros => parametros.Add(cartao => cartao.Distribuicao, distribuicao));
    }
}
