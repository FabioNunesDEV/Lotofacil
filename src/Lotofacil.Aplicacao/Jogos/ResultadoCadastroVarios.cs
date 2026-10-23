// <copyright file="ResultadoCadastroVarios.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Jogos;

/// <summary>
/// Desfecho do cadastro de vários jogos de um concurso.
/// </summary>
/// <param name="Gravados">Quantidade de jogos gravados.</param>
/// <param name="Duplicados">Quantidade de jogos recusados por já estarem cadastrados para o concurso.</param>
/// <param name="Recusa">Motivo da recusa de todos os jogos, ou <c>null</c> quando o concurso aceitou jogos.</param>
public sealed record ResultadoCadastroVarios(int Gravados, int Duplicados, string? Recusa);
