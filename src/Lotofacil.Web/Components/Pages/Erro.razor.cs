// <copyright file="Erro.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Diagnostics;
using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Pages;

/// <summary>
/// Página exibida quando ocorre uma exceção não tratada fora do ambiente de desenvolvimento.
/// </summary>
public partial class Erro
{
    /// <summary>
    /// Contexto HTTP da requisição que falhou, quando a página é renderizada no servidor.
    /// </summary>
    [CascadingParameter]
    private HttpContext? ContextoHttp { get; set; }

    /// <summary>
    /// Identificador da requisição, para localizar o erro no log.
    /// </summary>
    private string? IdentificadorRequisicao { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        this.IdentificadorRequisicao = Activity.Current?.Id ?? this.ContextoHttp?.TraceIdentifier;
    }
}
