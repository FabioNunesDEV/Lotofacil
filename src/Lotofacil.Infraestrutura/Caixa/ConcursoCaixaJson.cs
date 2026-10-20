// <copyright file="ConcursoCaixaJson.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Newtonsoft.Json;

namespace Lotofacil.Infraestrutura.Caixa;

/// <summary>
/// Campos usados da resposta da API da Caixa para um concurso; todos anuláveis para detectar ausência.
/// </summary>
internal sealed class ConcursoCaixaJson
{
    /// <summary>
    /// Número do concurso.
    /// </summary>
    [JsonProperty("numero")]
    public int? Numero { get; set; }

    /// <summary>
    /// Data do sorteio, no formato <c>dd/MM/yyyy</c>.
    /// </summary>
    [JsonProperty("dataApuracao")]
    public string? DataApuracao { get; set; }

    /// <summary>
    /// Dezenas sorteadas, como texto (<c>"01"</c> a <c>"25"</c>).
    /// </summary>
    [JsonProperty("listaDezenas")]
    public List<string>? ListaDezenas { get; set; }

    /// <summary>
    /// Dezenas na ordem do sorteio; vazia ou ausente quando não informada.
    /// </summary>
    [JsonProperty("dezenasSorteadasOrdemSorteio")]
    public List<string>? DezenasOrdemSorteio { get; set; }

    /// <summary>
    /// Rateio das faixas de premiação.
    /// </summary>
    [JsonProperty("listaRateioPremio")]
    public List<RateioCaixaJson>? ListaRateioPremio { get; set; }

    /// <summary>
    /// Arrecadação total.
    /// </summary>
    [JsonProperty("valorArrecadado")]
    public decimal? ValorArrecadado { get; set; }

    /// <summary>
    /// Valor acumulado para a faixa de 15 acertos do próximo concurso.
    /// </summary>
    [JsonProperty("valorAcumuladoProximoConcurso")]
    public decimal? ValorAcumuladoProximoConcurso { get; set; }

    /// <summary>
    /// Estimativa de prêmio do próximo concurso.
    /// </summary>
    [JsonProperty("valorEstimadoProximoConcurso")]
    public decimal? ValorEstimadoProximoConcurso { get; set; }

    /// <summary>
    /// Valor acumulado para a Lotofácil da Independência.
    /// </summary>
    [JsonProperty("valorAcumuladoConcursoEspecial")]
    public decimal? ValorAcumuladoConcursoEspecial { get; set; }
}
