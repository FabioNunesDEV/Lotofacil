// <copyright file="DialogoConferencia.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Diálogo de conferência de um jogo: volante com a situação de cada dezena e os detalhes do prêmio.
/// </summary>
public partial class DialogoConferencia
{
    /// <summary>
    /// Jogo conferido.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public JogoConferido Jogo { get; set; } = null!;

    /// <summary>
    /// Concurso sorteado do jogo.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Concurso Concurso { get; set; } = null!;

    /// <summary>
    /// Instância do diálogo, usada para fechar.
    /// </summary>
    [CascadingParameter]
    private IMudDialogInstance Dialogo { get; set; } = null!;

    /// <summary>
    /// Conferência do jogo; o diálogo só é aberto para jogos conferidos.
    /// </summary>
    private ConferenciaJogo Conferencia => this.Jogo.Conferencia!;

    /// <summary>
    /// Data em que o prêmio prescreve.
    /// </summary>
    private DateOnly Prescricao => this.Concurso.Data.AddDays(RegrasLotofacil.DiasPrescricao);

    /// <summary>
    /// Fecha o diálogo.
    /// </summary>
    private void Fechar()
    {
        this.Dialogo.Close();
    }
}
