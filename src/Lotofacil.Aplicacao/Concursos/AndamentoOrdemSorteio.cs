// <copyright file="AndamentoOrdemSorteio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Concurso cuja ordem está sendo buscada, sua posição e, numa pausa pedida pela Caixa, a espera até a nova tentativa.
/// </summary>
/// <param name="Numero">Número do concurso.</param>
/// <param name="Posicao">Posição do concurso, começando em 1.</param>
/// <param name="Total">Quantidade de concursos a buscar.</param>
/// <param name="Espera">Espera até a nova tentativa, ou <c>null</c> fora das pausas.</param>
public sealed record AndamentoOrdemSorteio(int Numero, int Posicao, int Total, TimeSpan? Espera);
