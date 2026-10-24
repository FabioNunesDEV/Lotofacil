// <copyright file="FalhaOrdemSorteio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Concurso cuja ordem do sorteio não pôde ser preenchida, com o motivo.
/// </summary>
/// <param name="Numero">Número do concurso.</param>
/// <param name="Mensagem">Motivo para o usuário e o log.</param>
/// <param name="Causa">Exceção que causou a falha, quando houver.</param>
public sealed record FalhaOrdemSorteio(int Numero, string Mensagem, Exception? Causa);
