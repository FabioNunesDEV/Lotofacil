// <copyright file="DistribuicaoTeoricaTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas.Perfis;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes da distribuição teórica dos perfis sobre as 3.268.760 combinações.
/// </summary>
public class DistribuicaoTeoricaTestes
{
    /// <summary>
    /// Valores de uma linha simples conferidos com cálculo à parte.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="valor">Valor da linha.</param>
    /// <param name="esperado">Percentual esperado, com duas casas.</param>
    [Theory]
    [InlineData(TipoPerfil.Pares, 7, 31.18)]
    [InlineData(TipoPerfil.Primos, 5, 30.87)]
    [InlineData(TipoPerfil.Repetidas, 9, 32.15)]
    public void PercentualDeUmValor(TipoPerfil tipo, int valor, double esperado)
    {
        Assert.Equal((decimal)esperado, Math.Round(DistribuicaoTeorica.Percentual(tipo, valor, valor), 2));
    }

    /// <summary>
    /// As faixas centrais da soma têm 29,07% e 29,67%.
    /// </summary>
    [Fact]
    public void PercentualDasFaixasDeSoma()
    {
        Assert.Equal(29.07m, Math.Round(DistribuicaoTeorica.Percentual(TipoPerfil.Soma, 180, 194), 2));
        Assert.Equal(29.67m, Math.Round(DistribuicaoTeorica.Percentual(TipoPerfil.Soma, 195, 209), 2));
    }

    /// <summary>
    /// A soma vai de 120 a 270, e as combinações somam 3.268.760; a soma 120 só tem uma combinação.
    /// </summary>
    [Fact]
    public void SomaCobreTodasAsCombinacoes()
    {
        int total = Enumerable.Range(0, 300).Sum(soma => DistribuicaoTeorica.Combinacoes(TipoPerfil.Soma, soma));

        Assert.Equal(3_268_760, DistribuicaoTeorica.TotalCombinacoes);
        Assert.Equal(3_268_760, total);
        Assert.Equal(120, DistribuicaoTeorica.MenorValor(TipoPerfil.Soma));
        Assert.Equal(270, DistribuicaoTeorica.MaiorValor(TipoPerfil.Soma));
        Assert.Equal(1, DistribuicaoTeorica.Combinacoes(TipoPerfil.Soma, 120));
        Assert.Equal(1, DistribuicaoTeorica.Combinacoes(TipoPerfil.Soma, 121));
        Assert.Equal(2, DistribuicaoTeorica.Combinacoes(TipoPerfil.Soma, 122));
    }

    /// <summary>
    /// Cada perfil soma 100% entre o menor e o maior valor possível.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    [Theory]
    [InlineData(TipoPerfil.Pares)]
    [InlineData(TipoPerfil.Primos)]
    [InlineData(TipoPerfil.MultiplosDe3)]
    [InlineData(TipoPerfil.Fibonacci)]
    [InlineData(TipoPerfil.Soma)]
    [InlineData(TipoPerfil.Repetidas)]
    [InlineData(TipoPerfil.Moldura)]
    public void CadaPerfilSomaCemPorCento(TipoPerfil tipo)
    {
        decimal total = DistribuicaoTeorica.Percentual(
            tipo,
            DistribuicaoTeorica.MenorValor(tipo),
            DistribuicaoTeorica.MaiorValor(tipo));

        Assert.Equal(100m, Math.Round(total, 10));
    }

    /// <summary>
    /// Os limites das quantidades seguem o tamanho do conjunto: 12 pares dão de 2 a 12; 15 repetidas, de 5 a 15.
    /// </summary>
    [Fact]
    public void LimitesDasQuantidades()
    {
        Assert.Equal(2, DistribuicaoTeorica.MenorValor(TipoPerfil.Pares));
        Assert.Equal(12, DistribuicaoTeorica.MaiorValor(TipoPerfil.Pares));
        Assert.Equal(5, DistribuicaoTeorica.MenorValor(TipoPerfil.Repetidas));
        Assert.Equal(15, DistribuicaoTeorica.MaiorValor(TipoPerfil.Repetidas));
        Assert.Equal(0, DistribuicaoTeorica.MenorValor(TipoPerfil.Fibonacci));
        Assert.Equal(7, DistribuicaoTeorica.MaiorValor(TipoPerfil.Fibonacci));
        Assert.Equal(0, DistribuicaoTeorica.Combinacoes(TipoPerfil.Pares, 13));
    }
}
