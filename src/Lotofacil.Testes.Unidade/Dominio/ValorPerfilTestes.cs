// <copyright file="ValorPerfilTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas.Perfis;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes dos conjuntos e do valor de um concurso em cada perfil.
/// </summary>
public class ValorPerfilTestes
{
    /// <summary>
    /// Dezenas 01 a 15.
    /// </summary>
    private static readonly int[] De01a15 = Enumerable.Range(1, 15).ToArray();

    /// <summary>
    /// Dezenas 11 a 25.
    /// </summary>
    private static readonly int[] De11a25 = Enumerable.Range(11, 15).ToArray();

    /// <summary>
    /// O concurso 01 a 15, com o anterior 11 a 25, tem os valores do exemplo da spec.
    /// </summary>
    /// <param name="tipo">Perfil calculado.</param>
    /// <param name="esperado">Valor esperado.</param>
    [Theory]
    [InlineData(TipoPerfil.Pares, 7)]
    [InlineData(TipoPerfil.Primos, 6)]
    [InlineData(TipoPerfil.MultiplosDe3, 5)]
    [InlineData(TipoPerfil.Fibonacci, 6)]
    [InlineData(TipoPerfil.Soma, 120)]
    [InlineData(TipoPerfil.Repetidas, 5)]
    [InlineData(TipoPerfil.Moldura, 9)]
    public void CalculaOsValoresDoExemplo(TipoPerfil tipo, int esperado)
    {
        Assert.Equal(esperado, ValorPerfil.Calcular(tipo, De01a15, De11a25));
    }

    /// <summary>
    /// Os conjuntos têm 12 pares, 9 primos, 8 múltiplos de 3, 7 de Fibonacci e 16 dezenas na moldura.
    /// </summary>
    [Fact]
    public void ConjuntosTemOsTamanhosDoVolante()
    {
        Assert.Equal(12, ValorPerfil.Pares.Count);
        Assert.Equal(9, ValorPerfil.Primos.Count);
        Assert.Equal(8, ValorPerfil.MultiplosDe3.Count);
        Assert.Equal(7, ValorPerfil.Fibonacci.Count);
        Assert.Equal(16, ValorPerfil.Moldura.Count);
    }

    /// <summary>
    /// O miolo são as 9 dezenas fora da moldura.
    /// </summary>
    [Fact]
    public void MioloEOQueSobraDaMoldura()
    {
        int[] miolo = Enumerable.Range(1, 25).Where(dezena => !ValorPerfil.Moldura.Contains(dezena)).ToArray();

        Assert.Equal([7, 8, 9, 12, 13, 14, 17, 18, 19], miolo);
    }

    /// <summary>
    /// As repetidas exigem o concurso anterior; os outros perfis não.
    /// </summary>
    [Fact]
    public void RepetidasExigemOAnterior()
    {
        Assert.Throws<ArgumentNullException>(() => ValorPerfil.Calcular(TipoPerfil.Repetidas, De01a15, null));
        Assert.Equal(7, ValorPerfil.Calcular(TipoPerfil.Pares, De01a15, null));
    }
}
