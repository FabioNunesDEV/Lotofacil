// <copyright file="Combinatoria.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio;

/// <summary>
/// Cálculos de combinação usados no preço e na conferência das apostas múltiplas.
/// </summary>
public static class Combinatoria
{
    /// <summary>
    /// Calcula C(n, k): de quantas formas é possível escolher k itens entre n.
    /// </summary>
    /// <param name="n">Quantidade de itens disponíveis.</param>
    /// <param name="k">Quantidade de itens escolhidos.</param>
    /// <returns>Quantidade de combinações, ou zero quando k é negativo ou maior que n.</returns>
    public static int Combinacoes(int n, int k)
    {
        if (k < 0 || k > n)
        {
            return 0;
        }

        // C(n, k) = C(n, n − k); usar o menor dos dois reduz as multiplicações.
        int menor = Math.Min(k, n - k);
        long resultado = 1;
        for (int i = 1; i <= menor; i++)
        {
            // O produto parcial dividido por i é sempre inteiro: é C(n − menor + i, i).
            resultado = resultado * (n - menor + i) / i;
        }

        return (int)resultado;
    }
}
