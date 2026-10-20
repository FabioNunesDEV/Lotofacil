// <copyright file="RateioCaixaJson.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Newtonsoft.Json;

namespace Lotofacil.Infraestrutura.Caixa;

/// <summary>
/// Uma faixa do rateio na resposta da API da Caixa.
/// </summary>
internal sealed class RateioCaixaJson
{
    /// <summary>
    /// Posição da faixa: 1 para 15 acertos até 5 para 11 acertos.
    /// </summary>
    [JsonProperty("faixa")]
    public int? Faixa { get; set; }

    /// <summary>
    /// Descrição da faixa (ex.: <c>"15 acertos"</c>).
    /// </summary>
    [JsonProperty("descricaoFaixa")]
    public string? DescricaoFaixa { get; set; }

    /// <summary>
    /// Quantidade de apostas ganhadoras.
    /// </summary>
    [JsonProperty("numeroDeGanhadores")]
    public int? NumeroDeGanhadores { get; set; }

    /// <summary>
    /// Valor do prêmio de cada aposta ganhadora.
    /// </summary>
    [JsonProperty("valorPremio")]
    public decimal? ValorPremio { get; set; }
}
