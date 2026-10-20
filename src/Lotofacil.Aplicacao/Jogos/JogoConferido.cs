// <copyright file="JogoConferido.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Jogos;

namespace Lotofacil.Aplicacao.Jogos;

/// <summary>
/// Jogo com sua posição no concurso e, se o concurso já foi sorteado, a conferência.
/// </summary>
/// <param name="Jogo">Jogo cadastrado.</param>
/// <param name="Numero">Posição do jogo no concurso, na ordem de cadastro, começando em 1.</param>
/// <param name="Conferencia">Acertos e prêmio, ou <c>null</c> enquanto o concurso não foi sorteado.</param>
public sealed record JogoConferido(Jogo Jogo, int Numero, ConferenciaJogo? Conferencia);
