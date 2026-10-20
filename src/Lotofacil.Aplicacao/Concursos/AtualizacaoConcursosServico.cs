// <copyright file="AtualizacaoConcursosServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Atualiza a base com os concursos divulgados depois do último gravado, um a um e em ordem.
/// </summary>
public class AtualizacaoConcursosServico
{
    /// <summary>
    /// Fonte dos concursos individuais.
    /// </summary>
    private readonly IFonteConcursos fonte;

    /// <summary>
    /// Repositório dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio;

    /// <summary>
    /// Cria o serviço de atualização.
    /// </summary>
    /// <param name="fonte">Fonte dos concursos individuais.</param>
    /// <param name="repositorio">Repositório dos concursos.</param>
    public AtualizacaoConcursosServico(IFonteConcursos fonte, IConcursoRepositorio repositorio)
    {
        this.fonte = fonte;
        this.repositorio = repositorio;
    }

    /// <summary>
    /// Busca e grava os concursos novos. Cada concurso é gravado assim que obtido; uma falha da fonte encerra a
    /// atualização mantendo os já gravados e volta no resultado. Outras falhas são lançadas.
    /// </summary>
    /// <param name="andamento">Recebe o concurso sendo buscado, ou <c>null</c>.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Desfecho da atualização.</returns>
    public async Task<ResultadoAtualizacao> AtualizarAsync(
        IProgress<AndamentoAtualizacao>? andamento = null,
        CancellationToken cancelamento = default)
    {
        int? ultimoGravado = await this.repositorio.ObterUltimoNumeroAsync(cancelamento);
        if (ultimoGravado is not int ultimo)
        {
            return new ResultadoAtualizacao(null, 0, null);
        }

        int gravados = 0;
        try
        {
            int ultimoDivulgado = await this.fonte.ObterNumeroUltimoConcursoAsync(cancelamento);
            int total = ultimoDivulgado - ultimo;

            for (int posicao = 1; posicao <= total; posicao++)
            {
                int numero = ultimo + posicao;
                andamento?.Report(new AndamentoAtualizacao(numero, posicao, total));

                Concurso concurso = await this.fonte.ObterConcursoAsync(numero, cancelamento);
                await this.repositorio.AdicionarAsync([concurso], cancelamento);
                gravados++;
            }
        }
        catch (FonteResultadosException falha)
        {
            return new ResultadoAtualizacao(ultimo, gravados, falha);
        }

        return new ResultadoAtualizacao(ultimo, gravados, null);
    }
}
