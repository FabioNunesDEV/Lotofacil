// <copyright file="RankingDezenas.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas;
using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Estatisticas;

/// <summary>
/// Lista as primeiras dezenas de um ranking, com o valor de cada uma e a indicação de empate.
/// </summary>
public partial class RankingDezenas
{
    /// <summary>
    /// Título do ranking (ex.: "Mais sorteadas").
    /// </summary>
    [Parameter]
    [EditorRequired]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Ranking a exibir.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public Ranking Ranking { get; set; } = null!;

    /// <summary>
    /// Indica que o ranking é por atraso; caso contrário, é por sorteios.
    /// </summary>
    [Parameter]
    public bool PorAtraso { get; set; }

    /// <summary>
    /// Classe CSS adicional do ranking.
    /// </summary>
    [Parameter]
    public string Classe { get; set; } = string.Empty;

    /// <summary>
    /// Texto da indicação de empate com a última dezena exibida.
    /// </summary>
    private string TextoEmpate
    {
        get
        {
            EstatisticaDezena ultimo = this.Ranking.Itens[^1];
            string valor = this.PorAtraso
                ? $"atraso {FormatacaoEstatistica.NumeroOuIndefinido(ultimo.Atraso)}"
                : $"{FormatacaoEstatistica.Quantidade(ultimo.Sorteios)} sorteios";

            return $"+{this.Ranking.Empatadas} empatadas com {valor}";
        }
    }

    /// <summary>
    /// Valor exibido ao lado da dezena.
    /// </summary>
    /// <param name="item">Estatística da dezena.</param>
    /// <returns>Sorteios ou atraso formatados.</returns>
    private string Valor(EstatisticaDezena item)
    {
        return this.PorAtraso
            ? FormatacaoEstatistica.AtrasoNoRanking(item.Atraso)
            : FormatacaoEstatistica.Quantidade(item.Sorteios);
    }
}
