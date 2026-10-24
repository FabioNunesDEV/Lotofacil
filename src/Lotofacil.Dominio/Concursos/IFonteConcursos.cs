// <copyright file="IFonteConcursos.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Dominio.Concursos;

/// <summary>
/// Fonte de concursos individuais (API de resultados da Caixa).
/// </summary>
public interface IFonteConcursos
{
    /// <summary>
    /// Obtém o número do último concurso divulgado; lança <see cref="FonteResultadosException"/> se não conseguir.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Número do último concurso divulgado.</returns>
    Task<int> ObterNumeroUltimoConcursoAsync(CancellationToken cancelamento = default);

    /// <summary>
    /// Obtém um concurso já validado; lança <see cref="FonteResultadosException"/> se ele não puder ser obtido,
    /// estiver fora do formato esperado ou ainda não tiver o rateio divulgado.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>O concurso pedido.</returns>
    Task<Concurso> ObterConcursoAsync(int numero, CancellationToken cancelamento = default);

    /// <summary>
    /// Obtém só a ordem do sorteio de um concurso, sem exigir o rateio; lança <see cref="FonteResultadosException"/>
    /// se a resposta não puder ser obtida ou estiver fora do formato esperado.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>As dezenas na ordem do sorteio, ou <c>null</c> se a resposta não trouxer a ordem.</returns>
    Task<IReadOnlyList<int>?> ObterOrdemSorteioAsync(int numero, CancellationToken cancelamento = default);
}
