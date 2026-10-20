// <copyright file="Volante.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio;
using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Volante de 01 a 25: marca dezenas até o limite ou, na conferência, mostra a situação de cada dezena.
/// </summary>
public partial class Volante
{
    /// <summary>
    /// Dezenas marcadas.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<int> Marcadas { get; set; } = [];

    /// <summary>
    /// Recebe as dezenas marcadas após cada clique; quando informado, o volante permite marcar.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<int>> MarcadasChanged { get; set; }

    /// <summary>
    /// Dezenas sorteadas, para mostrar a conferência; <c>null</c> fora da conferência.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<int>? Sorteadas { get; set; }

    /// <summary>
    /// Quantidade máxima de dezenas marcadas.
    /// </summary>
    [Parameter]
    public int Limite { get; set; } = RegrasLotofacil.DezenasPorJogo;

    /// <summary>
    /// Indica se o usuário pode marcar e desmarcar dezenas.
    /// </summary>
    private bool Interativo => this.MarcadasChanged.HasDelegate;

    /// <summary>
    /// Classes da dezena conforme ela está marcada, sorteada ou bloqueada.
    /// </summary>
    /// <param name="dezena">Dezena.</param>
    /// <returns>Classes CSS.</returns>
    private string Classe(int dezena)
    {
        bool marcada = this.Marcadas.Contains(dezena);
        if (this.Sorteadas is not null)
        {
            bool sorteada = this.Sorteadas.Contains(dezena);
            return (marcada, sorteada) switch
            {
                (true, true) => "dezena acerto",
                (true, false) => "dezena erro",
                (false, true) => "dezena nao-marcada",
                _ => "dezena",
            };
        }

        return marcada ? "dezena marcada" : "dezena";
    }

    /// <summary>
    /// Indica se a dezena não pode ser marcada porque o limite foi atingido.
    /// </summary>
    /// <param name="dezena">Dezena.</param>
    /// <returns><c>true</c> se a dezena estiver bloqueada.</returns>
    private bool Bloqueada(int dezena)
    {
        return !this.Marcadas.Contains(dezena) && this.Marcadas.Count >= this.Limite;
    }

    /// <summary>
    /// Marca ou desmarca a dezena e informa as novas dezenas marcadas.
    /// </summary>
    /// <param name="dezena">Dezena clicada.</param>
    /// <returns>Tarefa da notificação.</returns>
    private Task AlternarAsync(int dezena)
    {
        var marcadas = new HashSet<int>(this.Marcadas);
        if (!marcadas.Remove(dezena) && marcadas.Count < this.Limite)
        {
            marcadas.Add(dezena);
        }

        return this.MarcadasChanged.InvokeAsync(marcadas);
    }
}
