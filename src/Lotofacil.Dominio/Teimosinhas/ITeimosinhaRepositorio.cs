// <copyright file="ITeimosinhaRepositorio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Teimosinhas;

/// <summary>
/// Acesso às Teimosinhas gravadas na base local.
/// </summary>
public interface ITeimosinhaRepositorio
{
    /// <summary>
    /// Lista as Teimosinhas ativas, com os jogos, em ordem de nome.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Teimosinhas ativas.</returns>
    Task<IReadOnlyList<Teimosinha>> ListarAtivasAsync(CancellationToken cancelamento = default);

    /// <summary>
    /// Busca uma Teimosinha pelo identificador, ativa ou removida.
    /// </summary>
    /// <param name="id">Identificador.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>A Teimosinha, ou <c>null</c> se não existir.</returns>
    Task<Teimosinha?> ObterAsync(int id, CancellationToken cancelamento = default);

    /// <summary>
    /// Lista o nome de todas as Teimosinhas, inclusive as removidas.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Nome por identificador.</returns>
    Task<IReadOnlyDictionary<int, string>> ListarNomesAsync(CancellationToken cancelamento = default);

    /// <summary>
    /// Grava a Teimosinha com os jogos: insere se for nova, atualiza se já existir.
    /// </summary>
    /// <param name="teimosinha">Teimosinha a gravar.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da gravação.</returns>
    Task SalvarAsync(Teimosinha teimosinha, CancellationToken cancelamento = default);
}
