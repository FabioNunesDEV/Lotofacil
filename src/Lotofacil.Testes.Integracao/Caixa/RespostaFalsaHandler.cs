// <copyright file="RespostaFalsaHandler.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Net;

namespace Lotofacil.Testes.Integracao.Caixa;

/// <summary>
/// Substitui a rede nos testes: responde cada requisição com a função informada e guarda os endereços pedidos.
/// </summary>
internal sealed class RespostaFalsaHandler : HttpMessageHandler
{
    /// <summary>
    /// Função que produz a resposta de cada requisição.
    /// </summary>
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder;

    /// <summary>
    /// Cria o handler com a função de resposta.
    /// </summary>
    /// <param name="responder">Função que produz a resposta a partir da requisição.</param>
    public RespostaFalsaHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder)
    {
        this.responder = responder;
    }

    /// <summary>
    /// Endereços das requisições recebidas, na ordem.
    /// </summary>
    public List<Uri> Enderecos { get; } = [];

    /// <summary>
    /// Endereço da última requisição recebida.
    /// </summary>
    public Uri? UltimoEndereco => this.Enderecos.LastOrDefault();

    /// <summary>
    /// Cria um handler que responde sempre com o código e o conteúdo informados.
    /// </summary>
    /// <param name="codigo">Código HTTP da resposta.</param>
    /// <param name="conteudo">Corpo da resposta.</param>
    /// <returns>Handler criado.</returns>
    public static RespostaFalsaHandler Responder(HttpStatusCode codigo, byte[]? conteudo = null)
    {
        return new RespostaFalsaHandler((_, _) => Task.FromResult(CriarResposta(codigo, conteudo)));
    }

    /// <summary>
    /// Cria uma resposta com o código e o conteúdo informados.
    /// </summary>
    /// <param name="codigo">Código HTTP da resposta.</param>
    /// <param name="conteudo">Corpo da resposta.</param>
    /// <returns>Resposta criada.</returns>
    public static HttpResponseMessage CriarResposta(HttpStatusCode codigo, byte[]? conteudo = null)
    {
        return new HttpResponseMessage(codigo) { Content = new ByteArrayContent(conteudo ?? []) };
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        this.Enderecos.Add(request.RequestUri!);
        return this.responder(request, cancellationToken);
    }
}
