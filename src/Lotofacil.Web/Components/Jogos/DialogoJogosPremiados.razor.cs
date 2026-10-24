// <copyright file="DialogoJogosPremiados.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Diálogo com os jogos premiados dos concursos gravados por uma atualização.
/// </summary>
public partial class DialogoJogosPremiados
{
    /// <summary>
    /// Endereço da página Meus jogos.
    /// </summary>
    private const string EnderecoMeusJogos = "/meus-jogos";

    /// <summary>
    /// Jogos conferidos dos concursos novos.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public ConferenciaConcursos Conferencia { get; set; } = null!;

    /// <summary>
    /// Data de referência do prazo para receber.
    /// </summary>
    [Parameter]
    public DateOnly Hoje { get; set; }

    /// <summary>
    /// Instância do diálogo, usada para fechar.
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance Dialogo { get; set; } = null!;

    /// <summary>
    /// Navegação para Meus jogos.
    /// </summary>
    [Inject]
    private NavigationManager Navegacao { get; set; } = null!;

    /// <summary>
    /// Fecha o diálogo.
    /// </summary>
    private void Fechar()
    {
        this.Dialogo.Close();
    }

    /// <summary>
    /// Fecha o diálogo e abre Meus jogos.
    /// </summary>
    private void VerMeusJogos()
    {
        this.Dialogo.Close();
        this.Navegacao.NavigateTo(EnderecoMeusJogos);
    }
}
