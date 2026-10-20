// <copyright file="HistoricoCaixaFonteTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Net;
using System.Text;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura.Caixa;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Testes do download do histórico da Caixa, com a rede substituída por um handler falso.
/// </summary>
public class HistoricoCaixaFonteTestes
{
    /// <summary>
    /// Tempo limite informado nas opções dos testes.
    /// </summary>
    private const int TempoLimiteSegundos = 60;

    /// <summary>
    /// O arquivo é pedido no caminho de download a partir da URL base, com ou sem barra final.
    /// </summary>
    /// <param name="urlBase">URL base configurada.</param>
    /// <returns>Tarefa do teste.</returns>
    [Theory]
    [InlineData("https://exemplo.local/portaldeloterias")]
    [InlineData("https://exemplo.local/portaldeloterias/")]
    public async Task PedeOArquivoNoEnderecoDaConfiguracao(string urlBase)
    {
        byte[] amostra = PlanilhaCaixa.AbrirAmostra().ToArray();
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.OK, amostra);

        IReadOnlyList<Concurso> concursos = await CriarFonte(handler, urlBase).ObterHistoricoAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            "https://exemplo.local/portaldeloterias/api/resultados/download?modalidade=Lotof%C3%A1cil",
            handler.UltimoEndereco?.OriginalString);
        Assert.Equal([1, 2, 3808], concursos.Select(concurso => concurso.Numero));
    }

    /// <summary>
    /// Resposta com código de erro interrompe o download informando o código.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CaixaRespondeComErro()
    {
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.ServiceUnavailable);

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => CriarFonte(handler).ObterHistoricoAsync(TestContext.Current.CancellationToken));

        Assert.Equal("A Caixa respondeu com o código 503 ao baixar o histórico.", erro.Message);
    }

    /// <summary>
    /// Erro de rede vira mensagem de falta de conexão, com a causa preservada.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SemConexao()
    {
        var handler = new RespostaFalsaHandler(_ => throw new HttpRequestException("Host desconhecido."));

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => CriarFonte(handler).ObterHistoricoAsync(TestContext.Current.CancellationToken));

        Assert.Equal("Não foi possível conectar à Caixa para baixar o histórico.", erro.Message);
        Assert.IsType<HttpRequestException>(erro.InnerException);
    }

    /// <summary>
    /// Sem resposta dentro do tempo limite do cliente, a mensagem informa os segundos configurados.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task TempoLimiteExcedido()
    {
        var handler = new RespostaFalsaHandler(async cancelamento =>
        {
            await Task.Delay(Timeout.Infinite, cancelamento);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var cliente = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(50) };
        var fonte = new HistoricoCaixaFonte(cliente, new OpcoesFonteResultados("https://exemplo.local", TempoLimiteSegundos));

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => fonte.ObterHistoricoAsync(TestContext.Current.CancellationToken));

        Assert.Equal("A Caixa não respondeu em 60 segundos ao baixar o histórico.", erro.Message);
    }

    /// <summary>
    /// Cancelamento pedido pelo chamador continua como cancelamento, não como falha da fonte.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CancelamentoDoChamadorNaoViraFalha()
    {
        var handler = new RespostaFalsaHandler(async cancelamento =>
        {
            await Task.Delay(Timeout.Infinite, cancelamento);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var cancelamento = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CriarFonte(handler).ObterHistoricoAsync(cancelamento.Token));
    }

    /// <summary>
    /// Resposta de sucesso com conteúdo que não é XLSX é rejeitada.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ConteudoHtml()
    {
        byte[] html = Encoding.UTF8.GetBytes("<html><body>Manutenção</body></html>");
        RespostaFalsaHandler handler = RespostaFalsaHandler.Responder(HttpStatusCode.OK, html);

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => CriarFonte(handler).ObterHistoricoAsync(TestContext.Current.CancellationToken));

        Assert.Equal("O arquivo recebido da Caixa não é uma planilha XLSX válida.", erro.Message);
    }

    /// <summary>
    /// Cria a fonte com o handler falso.
    /// </summary>
    /// <param name="handler">Handler que substitui a rede.</param>
    /// <param name="urlBase">URL base configurada.</param>
    /// <returns>Fonte testada.</returns>
    private static HistoricoCaixaFonte CriarFonte(RespostaFalsaHandler handler, string urlBase = "https://exemplo.local")
    {
        return new HistoricoCaixaFonte(new HttpClient(handler), new OpcoesFonteResultados(urlBase, TempoLimiteSegundos));
    }
}
