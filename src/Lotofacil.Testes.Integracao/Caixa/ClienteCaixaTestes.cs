// <copyright file="ClienteCaixaTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Diagnostics;
using System.Net;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura.Caixa;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Testes do cliente da API da Caixa: pausa entre requisições e mensagens de falha com a operação.
/// </summary>
public class ClienteCaixaTestes
{
    /// <summary>
    /// Operação usada nas mensagens dos testes.
    /// </summary>
    private const string Operacao = "buscar o concurso 3810";

    /// <summary>
    /// Duas requisições seguidas começam com pelo menos a pausa configurada de diferença.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RespeitaAPausaEntreRequisicoes()
    {
        var relogio = Stopwatch.StartNew();
        List<TimeSpan> inicios = [];
        var handler = new RespostaFalsaHandler((_, _) =>
        {
            inicios.Add(relogio.Elapsed);
            return Task.FromResult(RespostaFalsaHandler.CriarResposta(HttpStatusCode.OK));
        });
        ClienteCaixa cliente = CriarCliente(handler, pausaMs: 200);

        await cliente.ObterAsync("api/lotofacil/1", Operacao, TestContext.Current.CancellationToken);
        await cliente.ObterAsync("api/lotofacil/2", Operacao, TestContext.Current.CancellationToken);

        Assert.True(inicios[1] - inicios[0] >= TimeSpan.FromMilliseconds(190), $"Intervalo: {inicios[1] - inicios[0]}");
    }

    /// <summary>
    /// Com pausa zero, as requisições não esperam.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task PausaZeroNaoEspera()
    {
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.OK);
        ClienteCaixa cliente = CriarCliente(handler, pausaMs: 0);
        var relogio = Stopwatch.StartNew();

        for (int numero = 1; numero <= 5; numero++)
        {
            await cliente.ObterAsync($"api/lotofacil/{numero}", Operacao, TestContext.Current.CancellationToken);
        }

        Assert.Equal(5, handler.Enderecos.Count);
        Assert.True(relogio.Elapsed < TimeSpan.FromMilliseconds(500), $"Tempo: {relogio.Elapsed}");
    }

    /// <summary>
    /// O caminho é somado à URL base, com ou sem barra final.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task MontaOEnderecoAPartirDaUrlBase()
    {
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.OK);
        ClienteCaixa cliente = CriarCliente(handler, urlBase: "https://exemplo.local/portaldeloterias/");

        await cliente.ObterAsync("api/lotofacil/3810", Operacao, TestContext.Current.CancellationToken);

        Assert.Equal("https://exemplo.local/portaldeloterias/api/lotofacil/3810", handler.UltimoEndereco?.OriginalString);
    }

    /// <summary>
    /// Código de erro aparece na mensagem junto com a operação.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CodigoDeErroComAOperacao()
    {
        ClienteCaixa cliente = CriarCliente(RespostaFalsaHandler.Responder(HttpStatusCode.ServiceUnavailable));

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => cliente.ObterAsync("api/lotofacil/3810", Operacao, TestContext.Current.CancellationToken));

        Assert.Equal("A Caixa respondeu com o código 503 ao buscar o concurso 3810.", erro.Message);
        Assert.False(erro.LimiteConsultas);
    }

    /// <summary>
    /// O código 429 marca o limite de consultas, com a mesma mensagem dos demais códigos.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CodigoDeLimiteDeConsultas()
    {
        ClienteCaixa cliente = CriarCliente(RespostaFalsaHandler.Responder(HttpStatusCode.TooManyRequests));

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => cliente.ObterAsync("api/lotofacil/3810", Operacao, TestContext.Current.CancellationToken));

        Assert.Equal("A Caixa respondeu com o código 429 ao buscar o concurso 3810.", erro.Message);
        Assert.True(erro.LimiteConsultas);
    }

    /// <summary>
    /// Erro de rede vira mensagem de falta de conexão com a operação.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SemConexaoComAOperacao()
    {
        var handler = new RespostaFalsaHandler((_, _) => throw new HttpRequestException("Host desconhecido."));

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => CriarCliente(handler).ObterAsync("api/lotofacil/3810", Operacao, TestContext.Current.CancellationToken));

        Assert.Equal("Não foi possível conectar à Caixa para buscar o concurso 3810.", erro.Message);
    }

    /// <summary>
    /// Tempo limite aparece na mensagem com os segundos configurados e a operação.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task TempoLimiteComAOperacao()
    {
        var handler = new RespostaFalsaHandler(async (_, cancelamento) =>
        {
            await Task.Delay(Timeout.Infinite, cancelamento);
            return RespostaFalsaHandler.CriarResposta(HttpStatusCode.OK);
        });
        using var http = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(50) };
        var cliente = new ClienteCaixa(http, new OpcoesFonteResultados("https://exemplo.local", 60, 0));

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => cliente.ObterAsync("api/lotofacil/3810", Operacao, TestContext.Current.CancellationToken));

        Assert.Equal("A Caixa não respondeu em 60 segundos ao buscar o concurso 3810.", erro.Message);
    }

    /// <summary>
    /// Cria o cliente com o handler falso.
    /// </summary>
    /// <param name="handler">Handler que substitui a rede.</param>
    /// <param name="pausaMs">Pausa entre requisições.</param>
    /// <param name="urlBase">URL base configurada.</param>
    /// <returns>Cliente testado.</returns>
    private static ClienteCaixa CriarCliente(
        RespostaFalsaHandler handler,
        int pausaMs = 0,
        string urlBase = "https://exemplo.local")
    {
        return new ClienteCaixa(new HttpClient(handler), new OpcoesFonteResultados(urlBase, 60, pausaMs));
    }
}
