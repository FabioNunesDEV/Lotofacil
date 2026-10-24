// <copyright file="ConcursosCaixaFonteTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Net;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura.Caixa;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Testes da fonte de concursos da API JSON da Caixa, com a rede substituída por um handler falso.
/// </summary>
public class ConcursosCaixaFonteTestes
{
    /// <summary>
    /// O último concurso é pedido no caminho da Lotofácil sem número.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ConsultaOUltimoConcurso()
    {
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.OK, RespostaConcursoCaixa.ComNumero(3811));

        int numero = await CriarFonte(handler).ObterNumeroUltimoConcursoAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3811, numero);
        Assert.Equal("https://exemplo.local/portaldeloterias/api/lotofacil/", handler.UltimoEndereco?.OriginalString);
    }

    /// <summary>
    /// Um concurso é pedido pelo número e convertido com a ordem do sorteio.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BuscaUmConcurso()
    {
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.OK, RespostaConcursoCaixa.ComNumero(3809));

        Concurso concurso = await CriarFonte(handler).ObterConcursoAsync(3809, TestContext.Current.CancellationToken);

        Assert.Equal(3809, concurso.Numero);
        Assert.NotNull(concurso.OrdemSorteio);
        Assert.Equal("https://exemplo.local/portaldeloterias/api/lotofacil/3809", handler.UltimoEndereco?.OriginalString);
    }

    /// <summary>
    /// Código de erro na consulta do último concurso tem a operação na mensagem.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ErroAoConsultarOUltimo()
    {
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.ServiceUnavailable);

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => CriarFonte(handler).ObterNumeroUltimoConcursoAsync(TestContext.Current.CancellationToken));

        Assert.Equal("A Caixa respondeu com o código 503 ao buscar o último concurso.", erro.Message);
    }

    /// <summary>
    /// Tempo limite na busca de um concurso tem o número na mensagem.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task TempoLimiteAoBuscarUmConcurso()
    {
        var handler = new RespostaFalsaHandler(async (_, cancelamento) =>
        {
            await Task.Delay(Timeout.Infinite, cancelamento);
            return RespostaFalsaHandler.CriarResposta(HttpStatusCode.OK);
        });
        using var http = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(50) };
        var fonte = new ConcursosCaixaFonte(new ClienteCaixa(http, new OpcoesFonteResultados("https://exemplo.local", 60, 0)));

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => fonte.ObterConcursoAsync(3810, TestContext.Current.CancellationToken));

        Assert.Equal("A Caixa não respondeu em 60 segundos ao buscar o concurso 3810.", erro.Message);
    }

    /// <summary>
    /// A ordem do sorteio é pedida no mesmo caminho do concurso.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BuscaAOrdemDoSorteio()
    {
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.OK, RespostaConcursoCaixa.ComNumero(1500));

        IReadOnlyList<int>? ordem = await CriarFonte(handler).ObterOrdemSorteioAsync(1500, TestContext.Current.CancellationToken);

        Assert.Equal([13, 10, 22, 19, 12, 1, 7, 14, 4, 24, 21, 15, 18, 20, 17], ordem);
        Assert.Equal("https://exemplo.local/portaldeloterias/api/lotofacil/1500", handler.UltimoEndereco?.OriginalString);
    }

    /// <summary>
    /// Código de erro na busca da ordem tem a operação própria na mensagem.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ErroAoBuscarAOrdem()
    {
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.ServiceUnavailable);

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => CriarFonte(handler).ObterOrdemSorteioAsync(1500, TestContext.Current.CancellationToken));

        Assert.Equal("A Caixa respondeu com o código 503 ao buscar a ordem do concurso 1500.", erro.Message);
    }

    /// <summary>
    /// Cria a fonte com o handler falso.
    /// </summary>
    /// <param name="handler">Handler que substitui a rede.</param>
    /// <returns>Fonte testada.</returns>
    private static ConcursosCaixaFonte CriarFonte(RespostaFalsaHandler handler)
    {
        var opcoes = new OpcoesFonteResultados("https://exemplo.local/portaldeloterias", 60, 0);

        return new ConcursosCaixaFonte(new ClienteCaixa(new HttpClient(handler), opcoes));
    }
}
