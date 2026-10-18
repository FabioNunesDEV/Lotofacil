// <copyright file="MainLayout.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Web.Components.Layout;

/// <summary>
/// Layout principal: barra superior com o título e menu lateral de navegação.
/// </summary>
public partial class MainLayout
{
    /// <summary>
    /// Indica se o menu lateral está aberto.
    /// </summary>
    private bool menuAberto = true;

    /// <summary>
    /// Abre o menu lateral se estiver fechado e fecha se estiver aberto.
    /// </summary>
    private void AlternarMenu()
    {
        this.menuAberto = !this.menuAberto;
    }
}
