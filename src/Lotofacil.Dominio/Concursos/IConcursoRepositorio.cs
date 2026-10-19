// <copyright file="IConcursoRepositorio.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Concursos;

/// <summary>
/// Acesso aos concursos gravados na base local.
/// </summary>
public interface IConcursoRepositorio
{
    /// <summary>
    /// Grava um lote de concursos de uma só vez; se algum número já existir, nenhum concurso do lote é gravado.
    /// </summary>
    /// <param name="concursos">Concursos a gravar.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da gravação.</returns>
    Task AdicionarAsync(IReadOnlyCollection<Concurso> concursos, CancellationToken cancelamento = default);

    /// <summary>
    /// Busca um concurso pelo número.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>O concurso, ou <c>null</c> se não estiver na base.</returns>
    Task<Concurso?> ObterAsync(int numero, CancellationToken cancelamento = default);

    /// <summary>
    /// Lista concursos do mais recente ao mais antigo.
    /// </summary>
    /// <param name="ignorar">Quantidade de concursos mais recentes a pular.</param>
    /// <param name="quantidade">Quantidade máxima de concursos a devolver.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Concursos em ordem decrescente de número.</returns>
    Task<IReadOnlyList<Concurso>> ListarAsync(int ignorar, int quantidade, CancellationToken cancelamento = default);

    /// <summary>
    /// Conta os concursos gravados.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Total de concursos na base.</returns>
    Task<int> ContarAsync(CancellationToken cancelamento = default);
}
