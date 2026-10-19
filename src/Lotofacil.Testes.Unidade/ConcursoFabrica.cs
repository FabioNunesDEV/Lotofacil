// <copyright file="ConcursoFabrica.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Testes.Unidade;

/// <summary>
/// Cria concursos válidos para os testes, permitindo trocar só o dado que interessa a cada teste.
/// </summary>
internal static class ConcursoFabrica
{
    /// <summary>
    /// Dezenas do concurso padrão, em ordem crescente.
    /// </summary>
    public static readonly int[] DezenasPadrao = [1, 2, 4, 5, 7, 10, 11, 13, 14, 18, 20, 21, 22, 24, 25];

    /// <summary>
    /// Cria um concurso válido.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <param name="dezenas">Dezenas sorteadas; as padrão quando <c>null</c>.</param>
    /// <param name="ordemSorteio">Ordem do sorteio; desconhecida quando <c>null</c>.</param>
    /// <param name="rateios">Rateio das faixas; o de <see cref="CriarRateios"/> quando <c>null</c>.</param>
    /// <param name="arrecadacao">Arrecadação total.</param>
    /// <param name="acumulado15">Acumulado da faixa de 15 acertos.</param>
    /// <param name="estimativaProximo">Estimativa do próximo concurso.</param>
    /// <param name="acumuladoIndependencia">Acumulado da Lotofácil da Independência.</param>
    /// <returns>Concurso criado.</returns>
    public static Concurso Criar(
        int numero = 3807,
        IEnumerable<int>? dezenas = null,
        IEnumerable<int>? ordemSorteio = null,
        IEnumerable<Rateio>? rateios = null,
        decimal arrecadacao = 24_873_112.50m,
        decimal acumulado15 = 0m,
        decimal estimativaProximo = 1_800_000m,
        decimal acumuladoIndependencia = 120_500_300.25m)
    {
        return new Concurso(
            numero,
            new DateOnly(2026, 10, 17),
            dezenas ?? DezenasPadrao,
            ordemSorteio,
            rateios ?? CriarRateios(),
            arrecadacao,
            acumulado15,
            estimativaProximo,
            acumuladoIndependencia);
    }

    /// <summary>
    /// Cria o rateio das cinco faixas com os prêmios fixos atuais para 11, 12 e 13 acertos.
    /// </summary>
    /// <param name="ganhadores15">Ganhadores de 15 acertos.</param>
    /// <param name="premio15">Prêmio de 15 acertos.</param>
    /// <returns>Rateio das faixas de 11 a 15.</returns>
    public static List<Rateio> CriarRateios(int ganhadores15 = 1, decimal premio15 = 1_712_408.63m)
    {
        return
        [
            new Rateio(15, ganhadores15, premio15),
            new Rateio(14, 287, 1_502.34m),
            new Rateio(13, 9_874, 35m),
            new Rateio(12, 121_045, 14m),
            new Rateio(11, 653_211, 7m),
        ];
    }
}
