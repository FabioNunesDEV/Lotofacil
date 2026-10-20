// <copyright file="IFonteHistoricoConcursos.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Concursos;

/// <summary>
/// Fonte do histórico completo de concursos (arquivo oficial da Caixa).
/// </summary>
public interface IFonteHistoricoConcursos
{
    /// <summary>
    /// Obtém todos os concursos do histórico, já validados; lança <see cref="FonteResultadosException"/> se o
    /// histórico não puder ser obtido ou tiver algum problema.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Concursos do histórico, na ordem do arquivo.</returns>
    Task<IReadOnlyList<Concurso>> ObterHistoricoAsync(CancellationToken cancelamento = default);
}
