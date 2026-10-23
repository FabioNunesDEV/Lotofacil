// <copyright file="PainelServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;

namespace Lotofacil.Aplicacao.Painel;

/// <summary>
/// Monta o resumo do Painel a partir dos concursos, das estatísticas e dos jogos.
/// </summary>
public class PainelServico
{
    /// <summary>
    /// Repositório dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio concursos;

    /// <summary>
    /// Serviço das estatísticas das dezenas.
    /// </summary>
    private readonly EstatisticasServico estatisticas;

    /// <summary>
    /// Serviço dos jogos, que já devolve os jogos conferidos.
    /// </summary>
    private readonly JogosServico jogos;

    /// <summary>
    /// Cria o serviço do Painel.
    /// </summary>
    /// <param name="concursos">Repositório dos concursos.</param>
    /// <param name="estatisticas">Serviço das estatísticas das dezenas.</param>
    /// <param name="jogos">Serviço dos jogos.</param>
    public PainelServico(IConcursoRepositorio concursos, EstatisticasServico estatisticas, JogosServico jogos)
    {
        this.concursos = concursos;
        this.estatisticas = estatisticas;
        this.jogos = jogos;
    }

    /// <summary>
    /// Monta o resumo do Painel.
    /// </summary>
    /// <param name="hoje">Data de referência para o prazo dos prêmios.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>O resumo, ou <c>null</c> com a base vazia.</returns>
    public async Task<ResumoPainel?> ObterResumoAsync(DateOnly hoje, CancellationToken cancelamento = default)
    {
        int? ultimoNumero = await this.concursos.ObterUltimoNumeroAsync(cancelamento);
        if (ultimoNumero is not int numero)
        {
            return null;
        }

        Concurso? ultimo = await this.concursos.ObterAsync(numero, cancelamento);
        EstatisticasDezenas? estatisticasDezenas = await this.estatisticas.CalcularAsync(null, cancelamento);
        if (ultimo is null || estatisticasDezenas is null)
        {
            return null;
        }

        int totalConcursos = await this.concursos.ContarAsync(cancelamento);
        IReadOnlyList<GrupoJogos> grupos = await this.jogos.ListarAsync(cancelamento);
        List<GrupoJogos> aguardando = grupos.Where(grupo => !grupo.Conferido).ToList();

        return new ResumoPainel(
            totalConcursos,
            ultimo,
            estatisticasDezenas,
            grupos.FirstOrDefault(grupo => grupo.NumeroConcurso == numero),
            aguardando.Sum(grupo => grupo.Jogos.Count),
            aguardando.Select(grupo => grupo.NumeroConcurso).Order().ToList(),
            grupos.Where(grupo => grupo.DentroDoPrazo(hoje)).Sum(grupo => grupo.PremioTotal));
    }
}
