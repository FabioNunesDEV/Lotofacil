// <copyright file="ClienteCaixa.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Diagnostics;
using System.Net;
using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Infraestrutura.Caixa;

/// <summary>
/// Faz as requisições à API da Caixa, uma de cada vez, com pausa mínima entre elas, e traduz as falhas de
/// comunicação em <see cref="FonteResultadosException"/>.
/// </summary>
internal sealed class ClienteCaixa
{
    /// <summary>
    /// Cliente HTTP com o tempo limite já configurado.
    /// </summary>
    private readonly HttpClient cliente;

    /// <summary>
    /// Configuração do acesso à Caixa.
    /// </summary>
    private readonly OpcoesFonteResultados opcoes;

    /// <summary>
    /// Garante uma requisição por vez, para a pausa valer entre todas elas.
    /// </summary>
    private readonly SemaphoreSlim vez = new(1, 1);

    /// <summary>
    /// Mede o tempo desde o início da requisição anterior; parado até a primeira requisição.
    /// </summary>
    private readonly Stopwatch desdeUltimaRequisicao = new();

    /// <summary>
    /// Cria o cliente da Caixa.
    /// </summary>
    /// <param name="cliente">Cliente HTTP com o tempo limite já configurado.</param>
    /// <param name="opcoes">Configuração do acesso à Caixa.</param>
    public ClienteCaixa(HttpClient cliente, OpcoesFonteResultados opcoes)
    {
        this.cliente = cliente;
        this.opcoes = opcoes;
    }

    /// <summary>
    /// Faz um GET na API da Caixa e devolve o conteúdo da resposta.
    /// </summary>
    /// <param name="caminho">Caminho relativo à URL base (ex.: <c>api/lotofacil/3808</c>).</param>
    /// <param name="operacao">Descrição da operação usada nas mensagens (ex.: "buscar o concurso 3808").</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Conteúdo da resposta.</returns>
    public async Task<byte[]> ObterAsync(string caminho, string operacao, CancellationToken cancelamento)
    {
        var endereco = new Uri($"{this.opcoes.UrlBase.TrimEnd('/')}/{caminho}");

        await this.vez.WaitAsync(cancelamento);
        try
        {
            await this.AguardarPausaAsync(cancelamento);
            return await this.BaixarAsync(endereco, operacao, cancelamento);
        }
        finally
        {
            this.vez.Release();
        }
    }

    /// <summary>
    /// Espera o que faltar para completar a pausa desde o início da requisição anterior.
    /// </summary>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da espera.</returns>
    private async Task AguardarPausaAsync(CancellationToken cancelamento)
    {
        TimeSpan pausa = TimeSpan.FromMilliseconds(this.opcoes.PausaEntreRequisicoesMs);
        if (this.desdeUltimaRequisicao.IsRunning)
        {
            TimeSpan restante = pausa - this.desdeUltimaRequisicao.Elapsed;
            if (restante > TimeSpan.Zero)
            {
                await Task.Delay(restante, cancelamento);
            }
        }

        this.desdeUltimaRequisicao.Restart();
    }

    /// <summary>
    /// Faz o GET e traduz as falhas de comunicação.
    /// </summary>
    /// <param name="endereco">Endereço completo.</param>
    /// <param name="operacao">Descrição da operação usada nas mensagens.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Conteúdo da resposta.</returns>
    private async Task<byte[]> BaixarAsync(Uri endereco, string operacao, CancellationToken cancelamento)
    {
        try
        {
            using HttpResponseMessage resposta = await this.cliente.GetAsync(endereco, cancelamento);
            if (!resposta.IsSuccessStatusCode)
            {
                throw new FonteResultadosException(
                    $"A Caixa respondeu com o código {(int)resposta.StatusCode} ao {operacao}.",
                    resposta.StatusCode == HttpStatusCode.TooManyRequests);
            }

            return await resposta.Content.ReadAsByteArrayAsync(cancelamento);
        }
        catch (HttpRequestException erro)
        {
            throw new FonteResultadosException($"Não foi possível conectar à Caixa para {operacao}.", erro);
        }
        catch (TaskCanceledException erro) when (!cancelamento.IsCancellationRequested)
        {
            // Sem cancelamento pedido pelo chamador, o cancelamento veio do tempo limite do HttpClient.
            throw new FonteResultadosException(
                $"A Caixa não respondeu em {this.opcoes.TempoLimiteSegundos} segundos ao {operacao}.",
                erro);
        }
    }
}
