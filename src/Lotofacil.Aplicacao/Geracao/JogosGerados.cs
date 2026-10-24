// <copyright file="JogosGerados.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Geracao;

/// <summary>
/// Desfecho de uma geração: os jogos com perfil, os avisos de jogos a menos ou o motivo da recusa.
/// </summary>
/// <param name="Jogos">Jogos gerados.</param>
/// <param name="CombinacoesPossiveis">
/// Quantidade de jogos diferentes possíveis (que atendem aos filtros), preenchida só quando é menor que a quantidade
/// pedida.
/// </param>
/// <param name="Recusa">Motivo da recusa, ou <c>null</c> quando a geração foi feita.</param>
/// <param name="ComFiltros">Indica que a geração tinha algum filtro ligado.</param>
/// <param name="JogosPedidos">
/// Quantidade pedida, preenchida só quando o sorteio com filtros achou menos jogos que o pedido.
/// </param>
public sealed record JogosGerados(
    IReadOnlyList<JogoGerado> Jogos,
    int? CombinacoesPossiveis,
    string? Recusa,
    bool ComFiltros = false,
    int? JogosPedidos = null);
