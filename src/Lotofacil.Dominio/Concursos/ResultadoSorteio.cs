// <copyright file="ResultadoSorteio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Concursos;

/// <summary>
/// Número e dezenas de um concurso: o necessário para as estatísticas, sem rateio nem valores.
/// </summary>
/// <param name="Numero">Número do concurso.</param>
/// <param name="Dezenas">As 15 dezenas sorteadas.</param>
public sealed record ResultadoSorteio(int Numero, IReadOnlyList<int> Dezenas);
