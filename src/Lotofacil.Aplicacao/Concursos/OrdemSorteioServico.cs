// <copyright file="OrdemSorteioServico.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Aplicacao.Concursos;

/// <summary>
/// Preenche, pela fonte de concursos, a ordem do sorteio dos concursos gravados sem ela, um a um e em ordem crescente.
/// </summary>
public class OrdemSorteioServico
{
    /// <summary>
    /// Falhas seguidas que encerram o preenchimento, para não esperar tempos limite com a Caixa fora do ar.
    /// </summary>
    public const int FalhasSeguidasParaEncerrar = 5;

    /// <summary>
    /// Esperas, em ordem, quando a Caixa limita as consultas.
    /// </summary>
    public static readonly IReadOnlyList<TimeSpan> EsperasLimitePadrao =
        [TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(8)];

    /// <summary>
    /// Fonte dos concursos individuais.
    /// </summary>
    private readonly IFonteConcursos fonte;

    /// <summary>
    /// Repositório dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio;

    /// <summary>
    /// Esperas usadas quando a Caixa limita as consultas.
    /// </summary>
    private readonly IReadOnlyList<TimeSpan> esperasLimite;

    /// <summary>
    /// Cria o serviço de preenchimento da ordem do sorteio.
    /// </summary>
    /// <param name="fonte">Fonte dos concursos individuais.</param>
    /// <param name="repositorio">Repositório dos concursos.</param>
    /// <param name="esperasLimite">Esperas quando a Caixa limita as consultas; <see cref="EsperasLimitePadrao"/> quando
    /// <c>null</c>.</param>
    public OrdemSorteioServico(
        IFonteConcursos fonte,
        IConcursoRepositorio repositorio,
        IReadOnlyList<TimeSpan>? esperasLimite = null)
    {
        this.fonte = fonte;
        this.repositorio = repositorio;
        this.esperasLimite = esperasLimite ?? EsperasLimitePadrao;
    }

    /// <summary>
    /// Conta os concursos gravados sem a ordem do sorteio.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Quantidade de concursos sem ordem.</returns>
    public Task<int> ContarSemOrdemAsync(CancellationToken cancelamento = default)
    {
        return this.repositorio.ContarSemOrdemAsync(cancelamento);
    }

    /// <summary>
    /// Busca e grava a ordem de cada concurso sem ela. Um concurso que falhar é pulado; falhas seguidas demais, ou o
    /// limite de consultas da Caixa depois de todas as esperas, encerram o preenchimento; o cancelamento interrompe
    /// sem lançar. Outras falhas são lançadas, mantendo as ordens já gravadas.
    /// </summary>
    /// <param name="andamento">Recebe o concurso sendo buscado, sua posição e as esperas, ou <c>null</c>.</param>
    /// <param name="cancelamento">Sinal de cancelamento, acionado ao interromper.</param>
    /// <returns>Desfecho do preenchimento.</returns>
    public async Task<ResultadoPreenchimentoOrdem> PreencherAsync(
        IProgress<AndamentoOrdemSorteio>? andamento = null,
        CancellationToken cancelamento = default)
    {
        IReadOnlyList<int> numeros = await this.repositorio.ListarNumerosSemOrdemAsync(cancelamento);
        List<FalhaOrdemSorteio> falhas = [];
        int preenchidos = 0;
        int falhasSeguidas = 0;

        try
        {
            for (int indice = 0; indice < numeros.Count; indice++)
            {
                var atual = new AndamentoOrdemSorteio(numeros[indice], indice + 1, numeros.Count, null);
                FalhaOrdemSorteio? falha = await this.PreencherConcursoAsync(atual, andamento, cancelamento);
                if (falha is null)
                {
                    preenchidos++;
                    falhasSeguidas = 0;
                    continue;
                }

                falhas.Add(falha);
                if (falha.Causa is FonteResultadosException { LimiteConsultas: true })
                {
                    return new ResultadoPreenchimentoOrdem(
                        numeros.Count, preenchidos, falhas, DesfechoPreenchimento.EncerradoPorLimite);
                }

                falhasSeguidas++;
                if (falhasSeguidas == FalhasSeguidasParaEncerrar)
                {
                    return new ResultadoPreenchimentoOrdem(
                        numeros.Count, preenchidos, falhas, DesfechoPreenchimento.EncerradoPorFalhas);
                }
            }
        }
        catch (OperationCanceledException) when (cancelamento.IsCancellationRequested)
        {
            return new ResultadoPreenchimentoOrdem(numeros.Count, preenchidos, falhas, DesfechoPreenchimento.Interrompido);
        }

        return new ResultadoPreenchimentoOrdem(numeros.Count, preenchidos, falhas, DesfechoPreenchimento.Concluido);
    }

    /// <summary>
    /// Busca e grava a ordem de um concurso.
    /// </summary>
    /// <param name="atual">Concurso e sua posição.</param>
    /// <param name="andamento">Recebe o andamento e as esperas, ou <c>null</c>.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>A falha, ou <c>null</c> se a ordem foi gravada.</returns>
    private async Task<FalhaOrdemSorteio?> PreencherConcursoAsync(
        AndamentoOrdemSorteio atual,
        IProgress<AndamentoOrdemSorteio>? andamento,
        CancellationToken cancelamento)
    {
        int numero = atual.Numero;
        IReadOnlyList<int>? ordem;
        try
        {
            ordem = await this.ObterOrdemAsync(atual, andamento, cancelamento);
        }
        catch (FonteResultadosException erro)
        {
            return new FalhaOrdemSorteio(numero, erro.Message, erro);
        }

        if (ordem is null)
        {
            return new FalhaOrdemSorteio(numero, $"A Caixa não informou a ordem do sorteio do concurso {numero}.", null);
        }

        try
        {
            await this.repositorio.GravarOrdemSorteioAsync(numero, ordem, cancelamento);
        }
        catch (ArgumentException erro)
        {
            return new FalhaOrdemSorteio(
                numero,
                $"A ordem do sorteio informada pela Caixa para o concurso {numero} não corresponde às dezenas gravadas.",
                erro);
        }

        return null;
    }

    /// <summary>
    /// Busca a ordem na fonte; quando a Caixa limita as consultas, espera e tenta de novo até acabarem as esperas.
    /// </summary>
    /// <param name="atual">Concurso e sua posição.</param>
    /// <param name="andamento">Recebe o andamento e as esperas, ou <c>null</c>.</param>
    /// <param name="cancelamento">Sinal de cancelamento, que também corta a espera.</param>
    /// <returns>A ordem, ou <c>null</c> se a resposta não a trouxer.</returns>
    private async Task<IReadOnlyList<int>?> ObterOrdemAsync(
        AndamentoOrdemSorteio atual,
        IProgress<AndamentoOrdemSorteio>? andamento,
        CancellationToken cancelamento)
    {
        int esperasFeitas = 0;
        while (true)
        {
            andamento?.Report(atual);
            try
            {
                return await this.fonte.ObterOrdemSorteioAsync(atual.Numero, cancelamento);
            }
            catch (FonteResultadosException erro) when (erro.LimiteConsultas && esperasFeitas < this.esperasLimite.Count)
            {
                TimeSpan espera = this.esperasLimite[esperasFeitas];
                andamento?.Report(atual with { Espera = espera });
                await Task.Delay(espera, cancelamento);
                esperasFeitas++;
            }
        }
    }
}
