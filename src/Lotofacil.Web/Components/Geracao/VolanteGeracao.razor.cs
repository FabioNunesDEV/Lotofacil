// <copyright file="VolanteGeracao.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Geracao;

/// <summary>
/// Volante de 25 dezenas em que cada clique alterna a dezena entre livre, fixa e excluída.
/// </summary>
public partial class VolanteGeracao
{
    /// <summary>
    /// Dezenas fixas.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<int> Fixas { get; set; } = [];

    /// <summary>
    /// Avisa a nova coleção de fixas depois de um clique.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<int>> FixasChanged { get; set; }

    /// <summary>
    /// Dezenas excluídas.
    /// </summary>
    [Parameter]
    public IReadOnlyCollection<int> Excluidas { get; set; } = [];

    /// <summary>
    /// Avisa a nova coleção de excluídas depois de um clique.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<int>> ExcluidasChanged { get; set; }

    /// <summary>
    /// Classe CSS do botão conforme o estado da dezena.
    /// </summary>
    /// <param name="dezena">Dezena do botão.</param>
    /// <returns>Classes do botão.</returns>
    private string Classe(int dezena)
    {
        if (this.Fixas.Contains(dezena))
        {
            return "dezena fixa";
        }

        return this.Excluidas.Contains(dezena) ? "dezena excluida" : "dezena";
    }

    /// <summary>
    /// Texto acessível do botão, com o estado da dezena.
    /// </summary>
    /// <param name="dezena">Dezena do botão.</param>
    /// <returns>Texto como "07 fixa".</returns>
    private string Rotulo(int dezena)
    {
        string estado = this.Fixas.Contains(dezena) ? "fixa" : this.Excluidas.Contains(dezena) ? "excluída" : "livre";

        return $"{dezena:00} {estado}";
    }

    /// <summary>
    /// Passa a dezena ao estado seguinte: livre → fixa → excluída → livre.
    /// </summary>
    /// <param name="dezena">Dezena clicada.</param>
    /// <returns>Tarefa dos avisos.</returns>
    private async Task AlternarAsync(int dezena)
    {
        var fixas = new HashSet<int>(this.Fixas);
        var excluidas = new HashSet<int>(this.Excluidas);
        if (fixas.Remove(dezena))
        {
            excluidas.Add(dezena);
        }
        else if (!excluidas.Remove(dezena))
        {
            fixas.Add(dezena);
        }

        await this.FixasChanged.InvokeAsync(fixas);
        await this.ExcluidasChanged.InvokeAsync(excluidas);
    }
}
