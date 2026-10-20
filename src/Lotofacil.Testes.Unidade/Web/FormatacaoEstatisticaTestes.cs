// <copyright file="FormatacaoEstatisticaTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Web.Components.Estatisticas;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes dos formatos de exibição das estatísticas, na cultura pt-BR.
/// </summary>
public class FormatacaoEstatisticaTestes
{
    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public FormatacaoEstatisticaTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Diferença com uma casa, sinal nos positivos e zero sem sinal.
    /// </summary>
    /// <param name="valor">Diferença.</param>
    /// <param name="esperado">Texto esperado.</param>
    [Theory]
    [InlineData("27.2", "+27,2")]
    [InlineData("-0.8", "-0,8")]
    [InlineData("0", "0,0")]
    [InlineData("-0.04", "0,0")]
    [InlineData("0.25", "+0,3")]
    public void Diferenca(string valor, string esperado)
    {
        Assert.Equal(esperado, FormatacaoEstatistica.Diferenca(decimal.Parse(valor, CultureInfo.InvariantCulture)));
    }

    /// <summary>
    /// Desvio com uma casa, sinal nos positivos e zero sem sinal.
    /// </summary>
    /// <param name="valor">Desvio.</param>
    /// <param name="esperado">Texto esperado.</param>
    [Theory]
    [InlineData(0.8997, "+0,9")]
    [InlineData(0.6455, "+0,6")]
    [InlineData(-1.3, "-1,3")]
    [InlineData(-0.04, "0,0")]
    public void Desvio(double valor, string esperado)
    {
        Assert.Equal(esperado, FormatacaoEstatistica.Desvio(valor));
    }

    /// <summary>
    /// Quantidade, percentual e esperado nos formatos da tabela.
    /// </summary>
    [Fact]
    public void QuantidadePercentualEEsperado()
    {
        Assert.Equal("2.312", FormatacaoEstatistica.Quantidade(2312));
        Assert.Equal("60,71%", FormatacaoEstatistica.Percentual(2312 * 100m / 3808));
        Assert.Equal("70,00%", FormatacaoEstatistica.Percentual(70m));
        Assert.Equal("2.284,8", FormatacaoEstatistica.Esperado(2284.8m));
        Assert.Equal("6,0", FormatacaoEstatistica.Esperado(6m));
    }

    /// <summary>
    /// Destaque acima ou abaixo do esperado; a diferença zero fica sem destaque.
    /// </summary>
    [Fact]
    public void ClasseDiferenca()
    {
        Assert.Equal("acima", FormatacaoEstatistica.ClasseDiferenca(1m));
        Assert.Equal("abaixo", FormatacaoEstatistica.ClasseDiferenca(-0.2m));
        Assert.Equal(string.Empty, FormatacaoEstatistica.ClasseDiferenca(0m));
    }

    /// <summary>
    /// Números de concurso e atraso na tabela, sem separador de milhar, ou "—".
    /// </summary>
    [Fact]
    public void NumeroOuIndefinido()
    {
        Assert.Equal("3805", FormatacaoEstatistica.NumeroOuIndefinido(3805));
        Assert.Equal("—", FormatacaoEstatistica.NumeroOuIndefinido(null));
    }

    /// <summary>
    /// Atraso no mapa e nos rankings.
    /// </summary>
    [Fact]
    public void Atraso()
    {
        Assert.Equal("18 conc.", FormatacaoEstatistica.AtrasoNoMapa(18));
        Assert.Equal("—", FormatacaoEstatistica.AtrasoNoMapa(null));
        Assert.Equal("último concurso", FormatacaoEstatistica.AtrasoNoRanking(0));
        Assert.Equal("18 conc.", FormatacaoEstatistica.AtrasoNoRanking(18));
        Assert.Equal("nunca saiu", FormatacaoEstatistica.AtrasoNoRanking(null));
    }
}
