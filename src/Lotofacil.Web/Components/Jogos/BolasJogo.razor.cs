// <copyright file="BolasJogo.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Mostra as dezenas de um jogo, destacando as acertadas quando o concurso já foi sorteado.
/// </summary>
public partial class BolasJogo
{
    /// <summary>
    /// Dezenas do jogo.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<int> Dezenas { get; set; } = [];

    /// <summary>
    /// Dezenas sorteadas, ou <c>null</c> enquanto o concurso não foi sorteado.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<int>? Sorteadas { get; set; }

    /// <summary>
    /// Classes da bola: acerto ou erro quando conferido.
    /// </summary>
    /// <param name="dezena">Dezena do jogo.</param>
    /// <returns>Classes CSS.</returns>
    private string Classe(int dezena)
    {
        if (this.Sorteadas is null)
        {
            return "bola";
        }

        return this.Sorteadas.Contains(dezena) ? "bola acerto" : "bola erro";
    }
}
