// <copyright file="ResultadoSalvarTeimosinha.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Teimosinhas;

/// <summary>
/// Desfecho do salvamento de jogos numa Teimosinha.
/// </summary>
/// <param name="Gravados">Quantidade de jogos gravados para o concurso e acrescentados à Teimosinha.</param>
/// <param name="Duplicados">Jogos recusados por já estarem no concurso ou na Teimosinha.</param>
/// <param name="Recusa">Motivo de nada ter sido gravado, ou <c>null</c> quando algum jogo foi gravado.</param>
/// <param name="Nome">Nome da Teimosinha, como gravado.</param>
public sealed record ResultadoSalvarTeimosinha(int Gravados, int Duplicados, string? Recusa, string Nome);
