// <copyright file="PerfilJogo.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Geracao;

/// <summary>
/// Perfil de um jogo: pares, ímpares, soma das dezenas e repetidas de um resultado de referência.
/// </summary>
/// <param name="Pares">Quantidade de dezenas pares.</param>
/// <param name="Impares">Quantidade de dezenas ímpares.</param>
/// <param name="Soma">Soma das dezenas.</param>
/// <param name="Repetidas">Quantidade de dezenas que também estão no resultado de referência.</param>
public sealed record PerfilJogo(int Pares, int Impares, int Soma, int Repetidas)
{
    /// <summary>
    /// Calcula o perfil de um jogo em relação a um resultado.
    /// </summary>
    /// <param name="dezenas">Dezenas do jogo.</param>
    /// <param name="resultado">Dezenas sorteadas no concurso de referência.</param>
    /// <returns>O perfil do jogo.</returns>
    public static PerfilJogo Calcular(IReadOnlyCollection<int> dezenas, IReadOnlyCollection<int> resultado)
    {
        int pares = dezenas.Count(dezena => dezena % 2 == 0);

        return new PerfilJogo(pares, dezenas.Count - pares, dezenas.Sum(), dezenas.Count(resultado.Contains));
    }
}
