// <copyright file="PesosDezenasTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas;
using Lotofacil.Dominio.Geracao;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes dos pesos das dezenas no sorteio ponderado.
/// </summary>
public class PesosDezenasTestes
{
    /// <summary>
    /// Na Aleatória, todas as dezenas têm peso 1.
    /// </summary>
    [Fact]
    public void AleatoriaTemPesoUm()
    {
        IReadOnlyDictionary<int, int> pesos = PesosDezenas.Calcular(EstatisticasDistintas(), EstrategiaGeracao.Aleatoria);

        Assert.Equal(25, pesos.Count);
        Assert.All(pesos.Values, peso => Assert.Equal(1, peso));
    }

    /// <summary>
    /// Em Mais sorteadas, a única com mais sorteios tem peso 25 e a única com menos tem peso 1.
    /// </summary>
    [Fact]
    public void MaisSorteadasPelaPosicao()
    {
        IReadOnlyDictionary<int, int> pesos = PesosDezenas.Calcular(EstatisticasDistintas(), EstrategiaGeracao.MaisSorteadas);

        // Em EstatisticasDistintas, a dezena d tem 100 + d sorteios.
        Assert.Equal(25, pesos[25]);
        Assert.Equal(1, pesos[1]);
        Assert.Equal(13, pesos[13]);
    }

    /// <summary>
    /// Em Menos sorteadas, a ordem se inverte.
    /// </summary>
    [Fact]
    public void MenosSorteadasInvertida()
    {
        IReadOnlyDictionary<int, int> pesos = PesosDezenas.Calcular(EstatisticasDistintas(), EstrategiaGeracao.MenosSorteadas);

        Assert.Equal(25, pesos[1]);
        Assert.Equal(1, pesos[25]);
    }

    /// <summary>
    /// Em Mais atrasadas, a dezena que nunca saiu tem o maior peso.
    /// </summary>
    [Fact]
    public void MaisAtrasadasComNuncaSaiu()
    {
        List<EstatisticaDezena> estatisticas = EstatisticasDistintas();
        estatisticas[6] = Estatistica(7, 0, null);

        IReadOnlyDictionary<int, int> pesos = PesosDezenas.Calcular(estatisticas, EstrategiaGeracao.MaisAtrasadas);

        // Atraso da dezena d é 25 − d; a 07 nunca saiu.
        Assert.Equal(25, pesos[7]);
        Assert.Equal(24, pesos[1]);
        Assert.Equal(1, pesos[25]);
    }

    /// <summary>
    /// Dezenas com o mesmo valor recebem o mesmo peso.
    /// </summary>
    [Fact]
    public void EmpateTemOMesmoPeso()
    {
        List<EstatisticaDezena> estatisticas = EstatisticasDistintas();
        estatisticas[2] = Estatistica(3, 107, 0);

        IReadOnlyDictionary<int, int> mais = PesosDezenas.Calcular(estatisticas, EstrategiaGeracao.MaisSorteadas);
        IReadOnlyDictionary<int, int> menos = PesosDezenas.Calcular(estatisticas, EstrategiaGeracao.MenosSorteadas);

        Assert.Equal(mais[7], mais[3]);
        Assert.Equal(menos[7], menos[3]);
    }

    /// <summary>
    /// A Equilibrada não tem pesos próprios.
    /// </summary>
    [Fact]
    public void EquilibradaERejeitada()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PesosDezenas.Calcular(EstatisticasDistintas(), EstrategiaGeracao.Equilibrada));
    }

    /// <summary>
    /// Estatísticas em que a dezena d tem 100 + d sorteios e atraso 25 − d.
    /// </summary>
    /// <returns>Estatísticas das 25 dezenas.</returns>
    private static List<EstatisticaDezena> EstatisticasDistintas()
    {
        return Enumerable.Range(1, 25).Select(dezena => Estatistica(dezena, 100 + dezena, 25 - dezena)).ToList();
    }

    /// <summary>
    /// Cria a estatística de uma dezena só com os valores usados nos pesos.
    /// </summary>
    /// <param name="dezena">Dezena.</param>
    /// <param name="sorteios">Sorteios na janela.</param>
    /// <param name="atraso">Atraso, ou <c>null</c> se nunca saiu.</param>
    /// <returns>A estatística.</returns>
    private static EstatisticaDezena Estatistica(int dezena, int sorteios, int? atraso)
    {
        return new EstatisticaDezena(dezena, sorteios, 0m, 0m, 0d, atraso, null);
    }
}
