// <copyright file="PaginaConcursos.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Uma página da lista de concursos e o total de concursos da base.
/// </summary>
/// <param name="Itens">Concursos da página, do mais recente ao mais antigo.</param>
/// <param name="Total">Total de concursos gravados.</param>
public sealed record PaginaConcursos(IReadOnlyList<Concurso> Itens, int Total);
