// <copyright file="RespostaFalsaHandler.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Net;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Substitui a rede nos testes: responde cada requisição com a função informada e guarda a última requisição.
/// </summary>
internal sealed class RespostaFalsaHandler : HttpMessageHandler
{
    /// <summary>
    /// Função que produz a resposta de cada requisição.
    /// </summary>
    private readonly Func<CancellationToken, Task<HttpResponseMessage>> responder;

    /// <summary>
    /// Cria o handler com a função de resposta.
    /// </summary>
    /// <param name="responder">Função que produz a resposta.</param>
    public RespostaFalsaHandler(Func<CancellationToken, Task<HttpResponseMessage>> responder)
    {
        this.responder = responder;
    }

    /// <summary>
    /// Endereço da última requisição recebida.
    /// </summary>
    public Uri? UltimoEndereco { get; private set; }

    /// <summary>
    /// Cria um handler que responde sempre com o código e o conteúdo informados.
    /// </summary>
    /// <param name="codigo">Código HTTP da resposta.</param>
    /// <param name="conteudo">Corpo da resposta.</param>
    /// <returns>Handler criado.</returns>
    public static RespostaFalsaHandler Responder(HttpStatusCode codigo, byte[]? conteudo = null)
    {
        return new RespostaFalsaHandler(_ => Task.FromResult(new HttpResponseMessage(codigo)
        {
            Content = new ByteArrayContent(conteudo ?? []),
        }));
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        this.UltimoEndereco = request.RequestUri;
        return this.responder(cancellationToken);
    }
}
