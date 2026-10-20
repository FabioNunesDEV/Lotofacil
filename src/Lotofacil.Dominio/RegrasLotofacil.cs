// <copyright file="RegrasLotofacil.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio;

/// <summary>
/// Números fixos das regras da Lotofácil.
/// </summary>
public static class RegrasLotofacil
{
    /// <summary>
    /// Menor dezena do volante.
    /// </summary>
    public const int DezenaMinima = 1;

    /// <summary>
    /// Maior dezena do volante.
    /// </summary>
    public const int DezenaMaxima = 25;

    /// <summary>
    /// Quantidade de dezenas do volante.
    /// </summary>
    public const int QuantidadeDezenas = 25;

    /// <summary>
    /// Quantidade de dezenas sorteadas em cada concurso.
    /// </summary>
    public const int DezenasSorteadas = 15;

    /// <summary>
    /// Menor quantidade de acertos que recebe prêmio.
    /// </summary>
    public const int MenorFaixaPremiada = 11;

    /// <summary>
    /// Maior quantidade de acertos que recebe prêmio.
    /// </summary>
    public const int MaiorFaixaPremiada = 15;
}
