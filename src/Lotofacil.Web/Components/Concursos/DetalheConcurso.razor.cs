// <copyright file="DetalheConcurso.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Concursos;

/// <summary>
/// Detalhe de um concurso: rateio das faixas, arrecadação e ordem do sorteio.
/// </summary>
public partial class DetalheConcurso
{
    /// <summary>
    /// Concurso exibido.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Concurso Concurso { get; set; } = null!;

    /// <summary>
    /// Rateio da faixa de 15 acertos até a de 11.
    /// </summary>
    private IEnumerable<Rateio> RateiosDaMaiorFaixa => this.Concurso.Rateios.OrderByDescending(rateio => rateio.Faixa);
}
