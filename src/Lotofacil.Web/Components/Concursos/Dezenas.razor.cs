// <copyright file="Dezenas.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Concursos;

/// <summary>
/// Exibe dezenas como bolinhas numeradas com dois dígitos, na ordem recebida.
/// </summary>
public partial class Dezenas
{
    /// <summary>
    /// Dezenas a exibir.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<int> Valores { get; set; } = [];
}
