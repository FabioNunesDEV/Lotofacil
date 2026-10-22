// <copyright file="CombinatoriaTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes do cálculo de combinações.
/// </summary>
public class CombinatoriaTestes
{
    /// <summary>
    /// Apostas equivalentes de 15 a 20 dezenas, conforme a tabela de preços da Caixa.
    /// </summary>
    /// <param name="dezenas">Quantidade de dezenas da aposta.</param>
    /// <param name="esperado">Quantidade de apostas de 15 dezenas.</param>
    [Theory]
    [InlineData(15, 1)]
    [InlineData(16, 16)]
    [InlineData(17, 136)]
    [InlineData(18, 816)]
    [InlineData(19, 3_876)]
    [InlineData(20, 15_504)]
    public void ApostasEquivalentes(int dezenas, int esperado)
    {
        Assert.Equal(esperado, Combinatoria.Combinacoes(dezenas, 15));
    }

    /// <summary>
    /// Valores usados na conferência das apostas múltiplas.
    /// </summary>
    /// <param name="n">Itens disponíveis.</param>
    /// <param name="k">Itens escolhidos.</param>
    /// <param name="esperado">Combinações esperadas.</param>
    [Theory]
    [InlineData(13, 12, 13)]
    [InlineData(3, 2, 3)]
    [InlineData(9, 4, 126)]
    [InlineData(25, 15, 3_268_760)]
    public void CalculaCombinacoes(int n, int k, int esperado)
    {
        Assert.Equal(esperado, Combinatoria.Combinacoes(n, k));
    }

    /// <summary>
    /// Escolher nenhum item tem uma única forma.
    /// </summary>
    /// <param name="n">Itens disponíveis.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void EscolherNenhumDaUm(int n)
    {
        Assert.Equal(1, Combinatoria.Combinacoes(n, 0));
    }

    /// <summary>
    /// Escolher mais itens do que há, ou uma quantidade negativa, não tem nenhuma forma.
    /// </summary>
    /// <param name="n">Itens disponíveis.</param>
    /// <param name="k">Itens escolhidos.</param>
    [Theory]
    [InlineData(3, 4)]
    [InlineData(3, -1)]
    public void ForaDoIntervaloDaZero(int n, int k)
    {
        Assert.Equal(0, Combinatoria.Combinacoes(n, k));
    }
}
