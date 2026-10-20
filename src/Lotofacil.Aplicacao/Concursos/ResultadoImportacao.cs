// <copyright file="ResultadoImportacao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Resumo de uma importação do histórico concluída sem falha.
/// </summary>
/// <param name="ConcursosNoArquivo">Quantidade de concursos lidos do arquivo.</param>
/// <param name="ConcursosGravados">Quantidade de concursos novos gravados na base.</param>
public sealed record ResultadoImportacao(int ConcursosNoArquivo, int ConcursosGravados);
