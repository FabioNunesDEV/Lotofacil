// <copyright file="ComponenteTesteBase.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Bunit;
using MudBlazor.Services;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Base dos testes de componentes: registra os serviços do MudBlazor e aceita as chamadas de JavaScript dele.
/// </summary>
public abstract class ComponenteTesteBase : BunitContext
{
    /// <summary>
    /// Prepara o contexto de renderização com os serviços do MudBlazor.
    /// </summary>
    protected ComponenteTesteBase()
    {
        // O MudBlazor chama JavaScript que o bUnit não executa; o modo Loose devolve valores padrão.
        this.JSInterop.Mode = JSRuntimeMode.Loose;
        this.Services.AddMudServices();
    }
}
