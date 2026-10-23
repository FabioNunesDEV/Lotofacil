// <copyright file="JogosGerados.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Geracao;

/// <summary>
/// Desfecho de uma geração: os jogos com perfil, o aviso de poucas combinações ou o motivo da recusa.
/// </summary>
/// <param name="Jogos">Jogos gerados.</param>
/// <param name="CombinacoesPossiveis">
/// Quantidade de jogos diferentes possíveis, preenchida só quando é menor que a quantidade pedida.
/// </param>
/// <param name="Recusa">Motivo da recusa, ou <c>null</c> quando a geração foi feita.</param>
public sealed record JogosGerados(IReadOnlyList<JogoGerado> Jogos, int? CombinacoesPossiveis, string? Recusa);
