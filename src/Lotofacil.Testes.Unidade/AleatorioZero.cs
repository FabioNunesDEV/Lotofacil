// <copyright file="AleatorioZero.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Testes.Unidade;

/// <summary>
/// Aleatório que sempre devolve zero e anota o limite de cada sorteio: no sorteio ponderado, escolhe sempre a
/// primeira dezena disponível, e os limites mostram o peso total usado em cada escolha.
/// </summary>
internal sealed class AleatorioZero : Random
{
    /// <summary>
    /// Limites recebidos, na ordem dos sorteios.
    /// </summary>
    public List<int> Maximos { get; } = [];

    /// <summary>
    /// Anota o limite e devolve zero.
    /// </summary>
    /// <param name="maxValue">Limite exclusivo.</param>
    /// <returns>Sempre zero.</returns>
    public override int Next(int maxValue)
    {
        this.Maximos.Add(maxValue);
        return 0;
    }
}
