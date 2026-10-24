// <copyright file="ConferenciaConcursos.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Jogos;

/// <summary>
/// Jogos conferidos de um intervalo de concursos, com as contas usadas no aviso de prêmios.
/// </summary>
/// <param name="Grupos">Jogos de cada concurso do intervalo, em ordem crescente de concurso.</param>
public sealed record ConferenciaConcursos(IReadOnlyList<GrupoJogos> Grupos)
{
    /// <summary>
    /// Quantidade de jogos conferidos.
    /// </summary>
    public int JogosConferidos => this.Grupos.Sum(grupo => grupo.Jogos.Count(jogo => jogo.Conferencia is not null));

    /// <summary>
    /// Quantidade de jogos premiados.
    /// </summary>
    public int JogosPremiados => this.Grupos.Sum(grupo => grupo.Jogos.Count(Premiado));

    /// <summary>
    /// Soma dos prêmios dos jogos.
    /// </summary>
    public decimal PremioTotal => this.Grupos.Sum(grupo => grupo.PremioTotal);

    /// <summary>
    /// Grupos com algum jogo premiado, cada um só com os jogos premiados (mantendo o número do jogo no concurso).
    /// </summary>
    public IReadOnlyList<GrupoJogos> GruposPremiados => this.Grupos
        .Where(grupo => grupo.Premiado)
        .Select(grupo => grupo with { Jogos = grupo.Jogos.Where(Premiado).ToList() })
        .ToList();

    /// <summary>
    /// Indica se o jogo foi conferido e premiado.
    /// </summary>
    /// <param name="jogo">Jogo do grupo.</param>
    /// <returns><c>true</c> quando há prêmio.</returns>
    private static bool Premiado(JogoConferido jogo)
    {
        return jogo.Conferencia?.Premiado == true;
    }
}
