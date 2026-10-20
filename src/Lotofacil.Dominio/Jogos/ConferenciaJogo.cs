// <copyright file="ConferenciaJogo.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Dominio.Jogos;

/// <summary>
/// Resultado de um jogo em um concurso sorteado: acertos e prêmio pelo rateio gravado do concurso.
/// </summary>
/// <param name="Acertos">Quantidade de dezenas do jogo que foram sorteadas.</param>
/// <param name="Premio">Prêmio da faixa atingida, ou zero sem prêmio.</param>
public sealed record ConferenciaJogo(int Acertos, decimal Premio)
{
    /// <summary>
    /// Indica se o jogo atingiu uma faixa premiada.
    /// </summary>
    public bool Premiado => this.Acertos >= RegrasLotofacil.MenorFaixaPremiada;

    /// <summary>
    /// Confere o jogo contra o concurso, usando sempre o rateio gravado desse concurso.
    /// </summary>
    /// <param name="jogo">Jogo conferido.</param>
    /// <param name="concurso">Concurso a que o jogo se destina.</param>
    /// <returns>Acertos e prêmio do jogo.</returns>
    public static ConferenciaJogo Conferir(Jogo jogo, Concurso concurso)
    {
        if (jogo.NumeroConcurso != concurso.Numero)
        {
            throw new ArgumentException(
                $"O jogo é do concurso {jogo.NumeroConcurso} e não pode ser conferido com o concurso {concurso.Numero}.",
                nameof(concurso));
        }

        int acertos = jogo.Dezenas.Count(dezena => concurso.Dezenas.Contains(dezena));
        decimal premio = acertos >= RegrasLotofacil.MenorFaixaPremiada
            ? concurso.ObterRateio(acertos).ValorPremio
            : 0m;

        return new ConferenciaJogo(acertos, premio);
    }
}
