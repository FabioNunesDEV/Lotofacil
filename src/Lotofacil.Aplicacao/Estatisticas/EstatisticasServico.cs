// <copyright file="EstatisticasServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;

namespace Lotofacil.Aplicacao.Estatisticas;

/// <summary>
/// Calcula as estatísticas por dezena a partir dos concursos gravados.
/// </summary>
public class EstatisticasServico
{
    /// <summary>
    /// Janelas oferecidas além de todo o histórico: quantidade de concursos mais recentes.
    /// </summary>
    public static readonly IReadOnlyList<int> JanelasDisponiveis = [10, 20, 50, 100];

    /// <summary>
    /// Repositório dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio;

    /// <summary>
    /// Cria o serviço de estatísticas.
    /// </summary>
    /// <param name="repositorio">Repositório dos concursos.</param>
    public EstatisticasServico(IConcursoRepositorio repositorio)
    {
        this.repositorio = repositorio;
    }

    /// <summary>
    /// Calcula as estatísticas na janela pedida.
    /// </summary>
    /// <param name="ultimosConcursos">Quantidade de concursos mais recentes, ou <c>null</c> para todo o histórico.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>As estatísticas, ou <c>null</c> com a base vazia.</returns>
    public async Task<EstatisticasDezenas?> CalcularAsync(int? ultimosConcursos, CancellationToken cancelamento = default)
    {
        if (ultimosConcursos is int quantidade)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(quantidade, 1, nameof(ultimosConcursos));
        }

        IReadOnlyList<ResultadoSorteio> historico = await this.repositorio.ListarResultadosAsync(cancelamento);

        return historico.Count == 0 ? null : EstatisticasDezenas.Calcular(historico, ultimosConcursos);
    }
}
