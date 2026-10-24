// <copyright file="JogosCriadosTeimosinha.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Teimosinhas;

/// <summary>
/// Jogos criados pelas Teimosinhas numa rodada de criação.
/// </summary>
/// <param name="Quantidade">Quantidade de jogos criados.</param>
/// <param name="PrimeiroConcurso">Menor concurso com jogo criado (válido quando a quantidade é maior que zero).</param>
/// <param name="UltimoConcurso">Maior concurso com jogo criado (válido quando a quantidade é maior que zero).</param>
public sealed record JogosCriadosTeimosinha(int Quantidade, int PrimeiroConcurso, int UltimoConcurso)
{
    /// <summary>
    /// Nenhum jogo criado.
    /// </summary>
    public static JogosCriadosTeimosinha Nenhum { get; } = new(0, 0, 0);
}
