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
    /// Menor quantidade de dezenas de um jogo (aposta mínima).
    /// </summary>
    public const int MinimoDezenasPorJogo = 15;

    /// <summary>
    /// Maior quantidade de dezenas de um jogo.
    /// </summary>
    public const int MaximoDezenasPorJogo = 20;

    /// <summary>
    /// Dias, a partir do sorteio, para o prêmio prescrever.
    /// </summary>
    public const int DiasPrescricao = 90;

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
