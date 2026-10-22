// <copyright file="GrupoJogos.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio;
using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Aplicacao.Jogos;

/// <summary>
/// Jogos de um mesmo concurso e, se ele já foi sorteado, o concurso com o resultado.
/// </summary>
/// <param name="NumeroConcurso">Número do concurso.</param>
/// <param name="Concurso">Concurso gravado, ou <c>null</c> enquanto não foi sorteado.</param>
/// <param name="Jogos">Jogos do concurso, na ordem de cadastro.</param>
public sealed record GrupoJogos(int NumeroConcurso, Concurso? Concurso, IReadOnlyList<JogoConferido> Jogos)
{
    /// <summary>
    /// Indica se o concurso já foi sorteado e os jogos foram conferidos.
    /// </summary>
    public bool Conferido => this.Concurso is not null;

    /// <summary>
    /// Indica se algum jogo do grupo foi premiado.
    /// </summary>
    public bool Premiado => this.Jogos.Any(jogo => jogo.Conferencia?.Premiado == true);

    /// <summary>
    /// Soma das apostas de 15 dezenas que os jogos do grupo valem, usada no custo do grupo.
    /// </summary>
    public int QuantidadeApostas => this.Jogos.Sum(jogo => jogo.Jogo.QuantidadeApostas);

    /// <summary>
    /// Soma dos prêmios dos jogos do grupo.
    /// </summary>
    public decimal PremioTotal => this.Jogos.Sum(jogo => jogo.Conferencia?.Premio ?? 0m);

    /// <summary>
    /// Data em que os prêmios do concurso prescrevem, ou <c>null</c> enquanto não foi sorteado.
    /// </summary>
    public DateOnly? Prescricao => this.Concurso?.Data.AddDays(RegrasLotofacil.DiasPrescricao);
}
