// <copyright file="IJogoRepositorio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Jogos;

/// <summary>
/// Acesso aos jogos do usuário gravados na base local.
/// </summary>
public interface IJogoRepositorio
{
    /// <summary>
    /// Grava um jogo novo.
    /// </summary>
    /// <param name="jogo">Jogo a gravar.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da gravação.</returns>
    Task AdicionarAsync(Jogo jogo, CancellationToken cancelamento = default);

    /// <summary>
    /// Lista todos os jogos, na ordem de cadastro.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Jogos gravados.</returns>
    Task<IReadOnlyList<Jogo>> ListarAsync(CancellationToken cancelamento = default);

    /// <summary>
    /// Verifica se já existe jogo com as mesmas dezenas para o concurso.
    /// </summary>
    /// <param name="numeroConcurso">Número do concurso.</param>
    /// <param name="dezenas">Dezenas do jogo, em qualquer ordem.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns><c>true</c> se o jogo já estiver cadastrado.</returns>
    Task<bool> ExisteAsync(int numeroConcurso, IEnumerable<int> dezenas, CancellationToken cancelamento = default);

    /// <summary>
    /// Exclui um jogo; não faz nada se ele não existir.
    /// </summary>
    /// <param name="id">Identificador do jogo.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da exclusão.</returns>
    Task ExcluirAsync(int id, CancellationToken cancelamento = default);
}
