// <copyright file="PesosDezenas.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas;

namespace Lotofacil.Dominio.Geracao;

/// <summary>
/// Calcula o peso de cada dezena no sorteio ponderado, pela posição dela no critério da estratégia.
/// </summary>
public static class PesosDezenas
{
    /// <summary>
    /// Calcula os pesos: 1 + quantidade de dezenas com valor pior no critério; empates recebem o mesmo peso.
    /// </summary>
    /// <param name="estatisticas">Estatísticas das 25 dezenas.</param>
    /// <param name="estrategia">Estratégia de critério único (todas, menos a Equilibrada).</param>
    /// <returns>Peso de cada dezena, indexado pela dezena.</returns>
    public static IReadOnlyDictionary<int, int> Calcular(
        IReadOnlyList<EstatisticaDezena> estatisticas,
        EstrategiaGeracao estrategia)
    {
        Func<EstatisticaDezena, EstatisticaDezena, bool> pior = estrategia switch
        {
            EstrategiaGeracao.Aleatoria => (_, _) => false,
            EstrategiaGeracao.MaisSorteadas => (outra, dezena) => outra.Sorteios < dezena.Sorteios,
            EstrategiaGeracao.MenosSorteadas => (outra, dezena) => outra.Sorteios > dezena.Sorteios,
            EstrategiaGeracao.MaisAtrasadas => (outra, dezena) => AtrasoComparavel(outra) < AtrasoComparavel(dezena),
            _ => throw new ArgumentOutOfRangeException(
                nameof(estrategia),
                "A estratégia Equilibrada combina os pesos de Mais sorteadas e Mais atrasadas."),
        };

        return estatisticas.ToDictionary(
            dezena => dezena.Dezena,
            dezena => 1 + estatisticas.Count(outra => pior(outra, dezena)));
    }

    /// <summary>
    /// Atraso usado na comparação: a dezena que nunca saiu tem o maior atraso.
    /// </summary>
    /// <param name="dezena">Estatística da dezena.</param>
    /// <returns>Atraso, ou o maior valor possível quando nunca saiu.</returns>
    private static int AtrasoComparavel(EstatisticaDezena dezena)
    {
        return dezena.Atraso ?? int.MaxValue;
    }
}
