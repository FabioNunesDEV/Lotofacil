// <copyright file="ResultadoGeracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Geracao;

/// <summary>
/// Jogos gerados ou o motivo da recusa.
/// </summary>
/// <param name="Jogos">Jogos gerados, cada um com as dezenas em ordem crescente.</param>
/// <param name="CombinacoesPossiveis">
/// Quantidade de jogos diferentes possíveis, preenchida só quando é menor que a quantidade pedida.
/// </param>
/// <param name="Recusa">Motivo da recusa, ou <c>null</c> quando a geração foi feita.</param>
public sealed record ResultadoGeracao(
    IReadOnlyList<IReadOnlyList<int>> Jogos,
    int? CombinacoesPossiveis,
    string? Recusa)
{
    /// <summary>
    /// Cria o resultado de uma geração recusada.
    /// </summary>
    /// <param name="motivo">Motivo da recusa.</param>
    /// <returns>Resultado sem jogos.</returns>
    public static ResultadoGeracao Recusada(string motivo)
    {
        return new ResultadoGeracao([], null, motivo);
    }
}
