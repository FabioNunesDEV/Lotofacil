// <copyright file="ConsultaConcursosServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Consulta os concursos gravados para exibição: lista paginada e busca por número.
/// </summary>
public class ConsultaConcursosServico
{
    /// <summary>
    /// Maior quantidade de concursos aceita em uma página.
    /// </summary>
    public const int TamanhoMaximoPagina = 100;

    /// <summary>
    /// Repositório dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio;

    /// <summary>
    /// Cria o serviço de consulta.
    /// </summary>
    /// <param name="repositorio">Repositório dos concursos.</param>
    public ConsultaConcursosServico(IConcursoRepositorio repositorio)
    {
        this.repositorio = repositorio;
    }

    /// <summary>
    /// Devolve uma página da lista de concursos, do mais recente ao mais antigo.
    /// </summary>
    /// <param name="pagina">Índice da página, começando em zero.</param>
    /// <param name="tamanhoPagina">Quantidade de concursos por página (1 a 100).</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Concursos da página e total da base.</returns>
    public async Task<PaginaConcursos> ListarPaginaAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancelamento = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(pagina);
        ArgumentOutOfRangeException.ThrowIfLessThan(tamanhoPagina, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tamanhoPagina, TamanhoMaximoPagina);

        IReadOnlyList<Concurso> itens = await this.repositorio.ListarAsync(
            pagina * tamanhoPagina,
            tamanhoPagina,
            cancelamento);
        int total = await this.repositorio.ContarAsync(cancelamento);

        return new PaginaConcursos(itens, total);
    }

    /// <summary>
    /// Busca um concurso pelo número.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>O concurso, ou <c>null</c> se não estiver na base.</returns>
    public Task<Concurso?> ObterAsync(int numero, CancellationToken cancelamento = default)
    {
        return this.repositorio.ObterAsync(numero, cancelamento);
    }
}
