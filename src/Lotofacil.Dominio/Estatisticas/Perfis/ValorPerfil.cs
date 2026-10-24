// <copyright file="ValorPerfil.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Estatisticas.Perfis;

/// <summary>
/// Conjuntos de dezenas dos perfis e o valor de um concurso em cada perfil.
/// </summary>
public static class ValorPerfil
{
    /// <summary>
    /// As 12 dezenas pares do volante.
    /// </summary>
    public static readonly IReadOnlySet<int> Pares = new HashSet<int> { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24 };

    /// <summary>
    /// Os 9 números primos do volante.
    /// </summary>
    public static readonly IReadOnlySet<int> Primos = new HashSet<int> { 2, 3, 5, 7, 11, 13, 17, 19, 23 };

    /// <summary>
    /// Os 8 múltiplos de 3 do volante.
    /// </summary>
    public static readonly IReadOnlySet<int> MultiplosDe3 = new HashSet<int> { 3, 6, 9, 12, 15, 18, 21, 24 };

    /// <summary>
    /// As 7 dezenas da sequência de Fibonacci no volante.
    /// </summary>
    public static readonly IReadOnlySet<int> Fibonacci = new HashSet<int> { 1, 2, 3, 5, 8, 13, 21 };

    /// <summary>
    /// As 16 dezenas da borda do volante de 5 × 5; o miolo são as outras 9.
    /// </summary>
    public static readonly IReadOnlySet<int> Moldura =
        new HashSet<int> { 1, 2, 3, 4, 5, 6, 10, 11, 15, 16, 20, 21, 22, 23, 24, 25 };

    /// <summary>
    /// Calcula o valor de um concurso num perfil.
    /// </summary>
    /// <param name="tipo">Perfil calculado.</param>
    /// <param name="dezenas">Dezenas sorteadas no concurso.</param>
    /// <param name="anterior">Dezenas do concurso anterior; obrigatórias só nas repetidas.</param>
    /// <returns>Quantidade de dezenas do perfil, ou a soma das dezenas.</returns>
    public static int Calcular(TipoPerfil tipo, IReadOnlyCollection<int> dezenas, IReadOnlyCollection<int>? anterior)
    {
        return tipo switch
        {
            TipoPerfil.Pares => dezenas.Count(Pares.Contains),
            TipoPerfil.Primos => dezenas.Count(Primos.Contains),
            TipoPerfil.MultiplosDe3 => dezenas.Count(MultiplosDe3.Contains),
            TipoPerfil.Fibonacci => dezenas.Count(Fibonacci.Contains),
            TipoPerfil.Soma => dezenas.Sum(),
            TipoPerfil.Repetidas => ContarRepetidas(dezenas, anterior),
            TipoPerfil.Moldura => dezenas.Count(Moldura.Contains),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };
    }

    /// <summary>
    /// Conta as dezenas que também saíram no concurso anterior.
    /// </summary>
    /// <param name="dezenas">Dezenas sorteadas no concurso.</param>
    /// <param name="anterior">Dezenas do concurso anterior.</param>
    /// <returns>Quantidade de dezenas repetidas.</returns>
    private static int ContarRepetidas(IReadOnlyCollection<int> dezenas, IReadOnlyCollection<int>? anterior)
    {
        ArgumentNullException.ThrowIfNull(anterior);

        return dezenas.Count(anterior.Contains);
    }
}
