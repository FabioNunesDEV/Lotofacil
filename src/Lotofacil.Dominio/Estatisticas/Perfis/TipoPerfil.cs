// <copyright file="TipoPerfil.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Estatisticas.Perfis;

/// <summary>
/// Perfis de um concurso analisados nas estatísticas, na ordem em que são exibidos.
/// </summary>
public enum TipoPerfil
{
    /// <summary>
    /// Quantidade de dezenas pares.
    /// </summary>
    Pares,

    /// <summary>
    /// Quantidade de números primos.
    /// </summary>
    Primos,

    /// <summary>
    /// Quantidade de múltiplos de 3.
    /// </summary>
    MultiplosDe3,

    /// <summary>
    /// Quantidade de dezenas da sequência de Fibonacci.
    /// </summary>
    Fibonacci,

    /// <summary>
    /// Soma das 15 dezenas.
    /// </summary>
    Soma,

    /// <summary>
    /// Quantidade de dezenas que também saíram no concurso anterior.
    /// </summary>
    Repetidas,

    /// <summary>
    /// Quantidade de dezenas da moldura (borda do volante).
    /// </summary>
    Moldura,
}
