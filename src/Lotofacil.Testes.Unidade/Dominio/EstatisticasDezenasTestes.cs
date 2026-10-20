// <copyright file="EstatisticasDezenasTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes do cálculo das estatísticas por dezena.
/// </summary>
public class EstatisticasDezenasTestes
{
    /// <summary>
    /// Dezenas 01 a 15.
    /// </summary>
    private static readonly int[] De01a15 = Faixa(1, 15);

    /// <summary>
    /// Dezenas 01 a 10 e 16 a 20.
    /// </summary>
    private static readonly int[] De01a10E16a20 = [.. Faixa(1, 10), .. Faixa(16, 20)];

    /// <summary>
    /// Dezenas 11 a 25.
    /// </summary>
    private static readonly int[] De11a25 = Faixa(11, 25);

    /// <summary>
    /// Histórico de três concursos: 01–15, 01–10 + 16–20 e 11–25.
    /// </summary>
    private static readonly List<ResultadoSorteio> TresConcursos =
    [
        new(1, De01a15),
        new(2, De01a10E16a20),
        new(3, De11a25),
    ];

    /// <summary>
    /// Sorteios, percentual, esperado, diferença e desvio sobre todo o histórico.
    /// </summary>
    [Fact]
    public void CalculaAFrequenciaSobreTodoOHistorico()
    {
        EstatisticasDezenas estatisticas = EstatisticasDezenas.Calcular(TresConcursos, null);

        Assert.Equal(3, estatisticas.ConcursosNaJanela);
        Assert.Equal(1.8m, estatisticas.Esperado);

        EstatisticaDezena dezena01 = estatisticas.Dezenas[0];
        Assert.Equal(1, dezena01.Dezena);
        Assert.Equal(2, dezena01.Sorteios);
        Assert.Equal(66.67m, Math.Round(dezena01.Percentual, 2));
        Assert.Equal(0.2m, dezena01.Diferenca);
        Assert.Equal(0.2357, dezena01.Desvio, 4);

        EstatisticaDezena dezena21 = estatisticas.Dezenas[20];
        Assert.Equal(1, dezena21.Sorteios);
        Assert.Equal(-0.8m, dezena21.Diferenca);
        Assert.Equal(-0.9428, dezena21.Desvio, 4);
    }

    /// <summary>
    /// Valores do exemplo da spec com 3.808 concursos: 2.312 sorteios dão 60,71%, +27,2 e +0,9σ.
    /// </summary>
    [Fact]
    public void ValoresDoExemploComTodoOHistorico()
    {
        // A dezena 20 está nas dezenas 11–25 e não está nas 01–15.
        List<ResultadoSorteio> historico = Enumerable.Range(1, 3808)
            .Select(numero => new ResultadoSorteio(numero, numero <= 2312 ? De11a25 : De01a15))
            .ToList();

        EstatisticaDezena dezena20 = EstatisticasDezenas.Calcular(historico, null).Dezenas[19];

        Assert.Equal(2312, dezena20.Sorteios);
        Assert.Equal(60.71m, Math.Round(dezena20.Percentual, 2));
        Assert.Equal(27.2m, dezena20.Diferenca);
        Assert.Equal(0.9, Math.Round(dezena20.Desvio, 1));
    }

    /// <summary>
    /// Valores do exemplo da spec nos últimos 10: 7 sorteios dão 70%, esperado 6, +1,0 e +0,6σ.
    /// </summary>
    [Fact]
    public void ValoresDoExemploNosUltimosDez()
    {
        // A dezena 05 está nas dezenas 01–15 e não está nas 11–25.
        List<ResultadoSorteio> historico = Enumerable.Range(1, 30)
            .Select(numero => new ResultadoSorteio(numero, numero is > 20 and <= 27 ? De01a15 : De11a25))
            .ToList();

        EstatisticasDezenas estatisticas = EstatisticasDezenas.Calcular(historico, 10);
        EstatisticaDezena dezena05 = estatisticas.Dezenas[4];

        Assert.Equal(6.0m, estatisticas.Esperado);
        Assert.Equal(7, dezena05.Sorteios);
        Assert.Equal(70m, dezena05.Percentual);
        Assert.Equal(1.0m, dezena05.Diferenca);
        Assert.Equal(0.6, Math.Round(dezena05.Desvio, 1));
    }

    /// <summary>
    /// A janela dos últimos N usa os N concursos de maior número, mesmo com o histórico fora de ordem.
    /// </summary>
    [Fact]
    public void JanelaUsaOsUltimosConcursos()
    {
        List<ResultadoSorteio> historico = [TresConcursos[2], TresConcursos[0], TresConcursos[1]];

        EstatisticasDezenas estatisticas = EstatisticasDezenas.Calcular(historico, 2);

        Assert.Equal(2, estatisticas.PrimeiroConcurso);
        Assert.Equal(3, estatisticas.UltimoConcurso);
        Assert.Equal(2, estatisticas.ConcursosNaJanela);
        Assert.Equal(3, estatisticas.TotalConcursos);
        Assert.Equal(1, estatisticas.Dezenas[0].Sorteios);
    }

    /// <summary>
    /// Com menos concursos que a janela, todos os concursos são usados.
    /// </summary>
    [Fact]
    public void BaseMenorQueAJanelaUsaTodos()
    {
        EstatisticasDezenas estatisticas = EstatisticasDezenas.Calcular(TresConcursos, 10);

        Assert.Equal(1, estatisticas.PrimeiroConcurso);
        Assert.Equal(3, estatisticas.ConcursosNaJanela);
    }

    /// <summary>
    /// O atraso e a última vez vêm de todo o histórico, mesmo com janela menor.
    /// </summary>
    [Fact]
    public void AtrasoIndependeDaJanela()
    {
        EstatisticaDezena dezena01 = EstatisticasDezenas.Calcular(TresConcursos, 1).Dezenas[0];

        Assert.Equal(0, dezena01.Sorteios);
        Assert.Equal(1, dezena01.Atraso);
        Assert.Equal(2, dezena01.UltimaVez);
    }

    /// <summary>
    /// Dezena do último concurso tem atraso zero.
    /// </summary>
    [Fact]
    public void DezenaDoUltimoConcursoTemAtrasoZero()
    {
        EstatisticaDezena dezena21 = EstatisticasDezenas.Calcular(TresConcursos, null).Dezenas[20];

        Assert.Equal(0, dezena21.Atraso);
        Assert.Equal(3, dezena21.UltimaVez);
    }

    /// <summary>
    /// O atraso conta pelos números dos concursos, incluindo os que faltam na base.
    /// </summary>
    [Fact]
    public void AtrasoContaConcursosQueFaltamNaBase()
    {
        List<ResultadoSorteio> historico = [new(1, De01a15), new(2, De01a10E16a20), new(5, De11a25)];

        EstatisticaDezena dezena01 = EstatisticasDezenas.Calcular(historico, null).Dezenas[0];

        Assert.Equal(3, dezena01.Atraso);
    }

    /// <summary>
    /// Dezena que nunca saiu tem atraso e última vez indefinidos e lidera o ranking de atraso.
    /// </summary>
    [Fact]
    public void DezenaQueNuncaSaiu()
    {
        EstatisticasDezenas estatisticas = EstatisticasDezenas.Calcular([new(1, De01a15)], null);

        EstatisticaDezena dezena16 = estatisticas.Dezenas[15];
        Assert.Null(dezena16.Atraso);
        Assert.Null(dezena16.UltimaVez);
        Assert.Equal([16, 17, 18, 19, 20], estatisticas.MaisAtrasadas.Itens.Select(item => item.Dezena));
        Assert.Equal(5, estatisticas.MaisAtrasadas.Empatadas);
    }

    /// <summary>
    /// As 15 dezenas do último concurso empatam com atraso zero: mostra as 5 menores e indica 10 empatadas.
    /// </summary>
    [Fact]
    public void MenosAtrasadasComEmpateNoUltimoConcurso()
    {
        EstatisticasDezenas estatisticas = EstatisticasDezenas.Calcular([new(1, De01a15)], null);

        Assert.Equal([1, 2, 3, 4, 5], estatisticas.MenosAtrasadas.Itens.Select(item => item.Dezena));
        Assert.Equal(10, estatisticas.MenosAtrasadas.Empatadas);
    }

    /// <summary>
    /// Os rankings de frequência ordenam pelo valor e desempatam pela dezena menor.
    /// </summary>
    [Fact]
    public void RankingsDeFrequenciaComDesempatePelaDezena()
    {
        EstatisticasDezenas estatisticas = EstatisticasDezenas.Calcular(TresConcursos, null);

        // 01 a 20 saíram 2 vezes; 21 a 25, uma vez.
        Assert.Equal([1, 2, 3, 4, 5], estatisticas.MaisSorteadas.Itens.Select(item => item.Dezena));
        Assert.Equal(15, estatisticas.MaisSorteadas.Empatadas);
        Assert.Equal([21, 22, 23, 24, 25], estatisticas.MenosSorteadas.Itens.Select(item => item.Dezena));
        Assert.Equal(0, estatisticas.MenosSorteadas.Empatadas);
    }

    /// <summary>
    /// O ranking de mais sorteadas segue o valor antes da dezena.
    /// </summary>
    [Fact]
    public void MaisSorteadasSegueOValor()
    {
        List<ResultadoSorteio> historico =
        [
            new(1, De11a25),
            new(2, De11a25),
            new(3, De01a10E16a20),
        ];

        Ranking ranking = EstatisticasDezenas.Calcular(historico, null).MaisSorteadas;

        // 16 a 20 saíram 3 vezes; 11 a 15 e 21 a 25, duas.
        Assert.Equal([16, 17, 18, 19, 20], ranking.Itens.Select(item => item.Dezena));
        Assert.Equal(0, ranking.Empatadas);
        Assert.All(ranking.Itens, item => Assert.Equal(3, item.Sorteios));
    }

    /// <summary>
    /// Histórico vazio e janela menor que 1 são rejeitados.
    /// </summary>
    [Fact]
    public void ValidaHistoricoEJanela()
    {
        Assert.Throws<ArgumentException>(() => EstatisticasDezenas.Calcular([], null));
        Assert.Throws<ArgumentOutOfRangeException>(() => EstatisticasDezenas.Calcular(TresConcursos, 0));
    }

    /// <summary>
    /// Dezenas consecutivas de um intervalo.
    /// </summary>
    /// <param name="inicio">Primeira dezena.</param>
    /// <param name="fim">Última dezena.</param>
    /// <returns>Dezenas do intervalo.</returns>
    private static int[] Faixa(int inicio, int fim)
    {
        return Enumerable.Range(inicio, fim - inicio + 1).ToArray();
    }
}
