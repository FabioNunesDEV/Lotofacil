// <copyright file="ImportacaoHistoricoServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Importa o histórico completo de concursos, gravando somente os que ainda não estão na base.
/// </summary>
public class ImportacaoHistoricoServico
{
    /// <summary>
    /// Fonte do histórico.
    /// </summary>
    private readonly IFonteHistoricoConcursos fonte;

    /// <summary>
    /// Repositório dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio;

    /// <summary>
    /// Cria o serviço de importação.
    /// </summary>
    /// <param name="fonte">Fonte do histórico.</param>
    /// <param name="repositorio">Repositório dos concursos.</param>
    public ImportacaoHistoricoServico(IFonteHistoricoConcursos fonte, IConcursoRepositorio repositorio)
    {
        this.fonte = fonte;
        this.repositorio = repositorio;
    }

    /// <summary>
    /// Obtém o histórico e grava, num único lote, os concursos que ainda não estão na base. Falhas da fonte
    /// chegam como <see cref="FonteResultadosException"/>, sem nada gravado.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Quantidades de concursos lidos e gravados.</returns>
    public async Task<ResultadoImportacao> ImportarAsync(CancellationToken cancelamento = default)
    {
        IReadOnlyList<Concurso> historico = await this.fonte.ObterHistoricoAsync(cancelamento);
        IReadOnlySet<int> jaGravados = await this.repositorio.ListarNumerosAsync(cancelamento);

        List<Concurso> novos = historico
            .Where(concurso => !jaGravados.Contains(concurso.Numero))
            .ToList();
        if (novos.Count > 0)
        {
            await this.repositorio.AdicionarAsync(novos, cancelamento);
        }

        return new ResultadoImportacao(historico.Count, novos.Count);
    }
}
