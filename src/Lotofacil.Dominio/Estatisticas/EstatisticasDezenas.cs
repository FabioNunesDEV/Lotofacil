// <copyright file="EstatisticasDezenas.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Dominio.Estatisticas;

/// <summary>
/// Estatísticas das 25 dezenas: frequência numa janela de concursos e atraso sobre todo o histórico.
/// </summary>
public sealed class EstatisticasDezenas
{
    /// <summary>
    /// Proporção de dezenas sorteadas por concurso (15 de 25 = 0,6).
    /// </summary>
    private const decimal ProporcaoSorteada = (decimal)RegrasLotofacil.DezenasSorteadas / RegrasLotofacil.QuantidadeDezenas;

    /// <summary>
    /// Cria as estatísticas já calculadas.
    /// </summary>
    /// <param name="primeiroConcurso">Primeiro concurso da janela.</param>
    /// <param name="ultimoConcurso">Último concurso da base, que fecha a janela.</param>
    /// <param name="concursosNaJanela">Quantidade de concursos na janela.</param>
    /// <param name="totalConcursos">Quantidade de concursos na base.</param>
    /// <param name="esperado">Sorteios esperados por dezena na janela.</param>
    /// <param name="dezenas">Estatísticas das 25 dezenas, em ordem de dezena.</param>
    private EstatisticasDezenas(
        int primeiroConcurso,
        int ultimoConcurso,
        int concursosNaJanela,
        int totalConcursos,
        decimal esperado,
        IReadOnlyList<EstatisticaDezena> dezenas)
    {
        this.PrimeiroConcurso = primeiroConcurso;
        this.UltimoConcurso = ultimoConcurso;
        this.ConcursosNaJanela = concursosNaJanela;
        this.TotalConcursos = totalConcursos;
        this.Esperado = esperado;
        this.Dezenas = dezenas;

        // "Nunca saiu" vale como o maior atraso possível.
        Func<EstatisticaDezena, int> atraso = estatistica => estatistica.Atraso ?? int.MaxValue;
        this.MaisSorteadas = Ranking.Criar(dezenas, estatistica => estatistica.Sorteios, decrescente: true);
        this.MenosSorteadas = Ranking.Criar(dezenas, estatistica => estatistica.Sorteios, decrescente: false);
        this.MaisAtrasadas = Ranking.Criar(dezenas, atraso, decrescente: true);
        this.MenosAtrasadas = Ranking.Criar(dezenas, atraso, decrescente: false);
    }

    /// <summary>
    /// Primeiro concurso da janela.
    /// </summary>
    public int PrimeiroConcurso { get; }

    /// <summary>
    /// Último concurso da base, que fecha a janela.
    /// </summary>
    public int UltimoConcurso { get; }

    /// <summary>
    /// Quantidade de concursos na janela.
    /// </summary>
    public int ConcursosNaJanela { get; }

    /// <summary>
    /// Quantidade de concursos na base.
    /// </summary>
    public int TotalConcursos { get; }

    /// <summary>
    /// Sorteios esperados por dezena na janela (60% dos concursos).
    /// </summary>
    public decimal Esperado { get; }

    /// <summary>
    /// Estatísticas das 25 dezenas, em ordem de dezena.
    /// </summary>
    public IReadOnlyList<EstatisticaDezena> Dezenas { get; }

    /// <summary>
    /// Dezenas que mais saíram na janela.
    /// </summary>
    public Ranking MaisSorteadas { get; }

    /// <summary>
    /// Dezenas que menos saíram na janela.
    /// </summary>
    public Ranking MenosSorteadas { get; }

    /// <summary>
    /// Dezenas há mais tempo sem sair.
    /// </summary>
    public Ranking MaisAtrasadas { get; }

    /// <summary>
    /// Dezenas há menos tempo sem sair.
    /// </summary>
    public Ranking MenosAtrasadas { get; }

    /// <summary>
    /// Calcula as estatísticas; lança <see cref="ArgumentException"/> com histórico vazio ou janela menor que 1.
    /// </summary>
    /// <param name="historico">Todos os concursos da base, em qualquer ordem.</param>
    /// <param name="ultimosConcursos">Quantidade de concursos mais recentes da janela, ou <c>null</c> para todos.</param>
    /// <returns>Estatísticas calculadas.</returns>
    public static EstatisticasDezenas Calcular(IReadOnlyList<ResultadoSorteio> historico, int? ultimosConcursos)
    {
        if (historico.Count == 0)
        {
            throw new ArgumentException("O histórico deve ter ao menos um concurso.", nameof(historico));
        }

        if (ultimosConcursos < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ultimosConcursos),
                "A janela deve ter ao menos um concurso.");
        }

        List<ResultadoSorteio> ordenado = historico.OrderBy(resultado => resultado.Numero).ToList();
        int concursosNaJanela = Math.Min(ultimosConcursos ?? ordenado.Count, ordenado.Count);
        List<ResultadoSorteio> janela = ordenado.Skip(ordenado.Count - concursosNaJanela).ToList();
        int ultimoConcurso = ordenado[^1].Numero;

        int[] sorteios = new int[RegrasLotofacil.DezenaMaxima + 1];
        foreach (int dezena in janela.SelectMany(resultado => resultado.Dezenas))
        {
            sorteios[dezena]++;
        }

        // Em ordem crescente, a última atribuição é a aparição mais recente.
        int?[] ultimaVez = new int?[RegrasLotofacil.DezenaMaxima + 1];
        foreach (ResultadoSorteio resultado in ordenado)
        {
            foreach (int dezena in resultado.Dezenas)
            {
                ultimaVez[dezena] = resultado.Numero;
            }
        }

        decimal esperado = concursosNaJanela * ProporcaoSorteada;
        double desvioPadrao = Math.Sqrt(concursosNaJanela * (double)ProporcaoSorteada * (1 - (double)ProporcaoSorteada));

        List<EstatisticaDezena> dezenas = Enumerable
            .Range(RegrasLotofacil.DezenaMinima, RegrasLotofacil.QuantidadeDezenas)
            .Select(dezena =>
            {
                decimal diferenca = sorteios[dezena] - esperado;
                return new EstatisticaDezena(
                    dezena,
                    sorteios[dezena],
                    sorteios[dezena] * 100m / concursosNaJanela,
                    diferenca,
                    (double)diferenca / desvioPadrao,
                    ultimoConcurso - ultimaVez[dezena],
                    ultimaVez[dezena]);
            })
            .ToList();

        return new EstatisticasDezenas(
            janela[0].Numero,
            ultimoConcurso,
            concursosNaJanela,
            ordenado.Count,
            esperado,
            dezenas);
    }
}
