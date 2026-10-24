// <copyright file="EstatisticasServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;
using Lotofacil.Dominio.Estatisticas.Perfis;

namespace Lotofacil.Aplicacao.Estatisticas;

/// <summary>
/// Calcula as estatísticas por dezena e por perfil do concurso a partir dos concursos gravados.
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
        ValidarJanela(ultimosConcursos);

        IReadOnlyList<ResultadoSorteio> historico = await this.repositorio.ListarResultadosAsync(cancelamento);

        return historico.Count == 0 ? null : EstatisticasDezenas.Calcular(historico, ultimosConcursos);
    }

    /// <summary>
    /// Calcula os perfis dos concursos na janela pedida.
    /// </summary>
    /// <param name="ultimosConcursos">Quantidade de concursos mais recentes, ou <c>null</c> para todo o histórico.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Os 7 perfis, ou <c>null</c> com a base vazia.</returns>
    public async Task<IReadOnlyList<DistribuicaoPerfil>?> CalcularPerfisAsync(
        int? ultimosConcursos,
        CancellationToken cancelamento = default)
    {
        ValidarJanela(ultimosConcursos);

        IReadOnlyList<ResultadoSorteio> historico = await this.repositorio.ListarResultadosAsync(cancelamento);

        return historico.Count == 0 ? null : EstatisticasPerfis.Calcular(historico, ultimosConcursos);
    }

    /// <summary>
    /// Rejeita uma janela menor que 1 concurso.
    /// </summary>
    /// <param name="ultimosConcursos">Quantidade de concursos mais recentes, ou <c>null</c> para todo o histórico.</param>
    private static void ValidarJanela(int? ultimosConcursos)
    {
        if (ultimosConcursos is int quantidade)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(quantidade, 1, nameof(ultimosConcursos));
        }
    }
}
