// <copyright file="CartaoPerfil.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas.Perfis;
using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Estatisticas;

/// <summary>
/// Cartão de um perfil do concurso: uma linha por faixa, com barra de observado × teórico e atraso.
/// </summary>
public partial class CartaoPerfil
{
    /// <summary>
    /// Folga da escala sobre o maior percentual, para a barra cheia não encostar na borda.
    /// </summary>
    private const decimal FolgaDaEscala = 1.1m;

    /// <summary>
    /// Perfil exibido.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public DistribuicaoPerfil Distribuicao { get; set; } = null!;

    /// <summary>
    /// Percentual que corresponde à barra inteira: o maior observado ou teórico do cartão, com folga.
    /// </summary>
    private decimal Escala => this.Distribuicao.Faixas.Max(faixa => Math.Max(faixa.Observado, faixa.Teorico)) * FolgaDaEscala;

    /// <summary>
    /// Posição de um percentual na barra.
    /// </summary>
    /// <param name="percentual">Percentual observado ou teórico.</param>
    /// <returns>Valor CSS em percentual.</returns>
    private string Posicao(decimal percentual)
    {
        return FormatacaoPerfil.PosicaoNaBarra(percentual, this.Escala);
    }
}
