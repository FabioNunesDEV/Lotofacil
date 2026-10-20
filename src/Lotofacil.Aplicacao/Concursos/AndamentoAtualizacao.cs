// <copyright file="AndamentoAtualizacao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Concurso que a atualização está buscando e sua posição entre os que faltam.
/// </summary>
/// <param name="Numero">Número do concurso sendo buscado.</param>
/// <param name="Posicao">Posição do concurso, começando em 1.</param>
/// <param name="Total">Quantidade de concursos a buscar.</param>
public sealed record AndamentoAtualizacao(int Numero, int Posicao, int Total);
