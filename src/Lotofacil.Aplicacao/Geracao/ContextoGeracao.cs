// <copyright file="ContextoGeracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Aplicacao.Geracao;

/// <summary>
/// Dados da base usados pela página de geração: último concurso gravado e total de concursos.
/// </summary>
/// <param name="UltimoConcurso">Número do último concurso gravado.</param>
/// <param name="UltimoResultado">Dezenas sorteadas no último concurso gravado.</param>
/// <param name="TotalConcursos">Quantidade de concursos gravados.</param>
public sealed record ContextoGeracao(int UltimoConcurso, IReadOnlyList<int> UltimoResultado, int TotalConcursos)
{
    /// <summary>
    /// Concurso sugerido para os jogos: o seguinte ao último gravado.
    /// </summary>
    public int ProximoConcurso => this.UltimoConcurso + 1;
}
