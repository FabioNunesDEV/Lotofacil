// <copyright file="MapaCalor.razor.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Estatisticas;
using Microsoft.AspNetCore.Components;

namespace Lotofacil.Web.Components.Estatisticas;

/// <summary>
/// Mostra as 25 dezenas no formato do volante, coloridas pela métrica escolhida.
/// </summary>
public partial class MapaCalor
{
    /// <summary>
    /// Maior intensidade de cor, em percentual da cor da métrica.
    /// </summary>
    private const int IntensidadeMaxima = 85;

    /// <summary>
    /// Intensidade mínima de frequência e atraso, para a célula de menor valor não ficar sem cor.
    /// </summary>
    private const int IntensidadeBase = 5;

    /// <summary>
    /// Intensidade a partir da qual o texto passa a ser branco.
    /// </summary>
    private const int IntensidadeTextoClaro = 50;

    /// <summary>
    /// Métrica exibida.
    /// </summary>
    private MetricaMapa metrica = MetricaMapa.Frequencia;

    /// <summary>
    /// Métricas que o mapa pode exibir.
    /// </summary>
    public enum MetricaMapa
    {
        /// <summary>
        /// Sorteios na janela.
        /// </summary>
        Frequencia,

        /// <summary>
        /// Concursos desde a última vez que a dezena saiu.
        /// </summary>
        Atraso,

        /// <summary>
        /// Distância do esperado em desvios-padrão.
        /// </summary>
        Desvio,
    }

    /// <summary>
    /// Estatísticas a exibir.
    /// </summary>
    [Parameter]
    [EditorRequired]
    public EstatisticasDezenas Estatisticas { get; set; } = null!;

    /// <summary>
    /// Explicação da métrica exibida.
    /// </summary>
    private string Legenda => this.metrica switch
    {
        MetricaMapa.Atraso => "Concursos desde a última vez que a dezena saiu. Mais escuro = mais atrasada.",
        MetricaMapa.Desvio => "Distância da frequência esperada em desvios-padrão. Verde acima, vermelho abaixo do esperado.",
        _ => "Quantidade de concursos em que a dezena saiu. Mais escuro = mais sorteada.",
    };

    /// <summary>
    /// Troca a métrica exibida.
    /// </summary>
    /// <param name="novaMetrica">Métrica escolhida.</param>
    private void TrocarMetrica(MetricaMapa novaMetrica)
    {
        this.metrica = novaMetrica;
    }

    /// <summary>
    /// Texto do valor na célula.
    /// </summary>
    /// <param name="dezena">Estatística da dezena.</param>
    /// <returns>Sorteios, atraso ou desvio formatados.</returns>
    private string Texto(EstatisticaDezena dezena)
    {
        return this.metrica switch
        {
            MetricaMapa.Atraso => FormatacaoEstatistica.AtrasoNoMapa(dezena.Atraso),
            MetricaMapa.Desvio => $"{FormatacaoEstatistica.Desvio(dezena.Desvio)}σ",
            _ => FormatacaoEstatistica.Quantidade(dezena.Sorteios),
        };
    }

    /// <summary>
    /// Classes da célula; no desvio, indica se a dezena está acima ou abaixo do esperado.
    /// </summary>
    /// <param name="dezena">Estatística da dezena.</param>
    /// <returns>Classes CSS.</returns>
    private string ClasseCelula(EstatisticaDezena dezena)
    {
        if (this.metrica != MetricaMapa.Desvio)
        {
            return "celula";
        }

        return $"celula {FormatacaoEstatistica.ClasseDiferenca(dezena.Diferenca)}".TrimEnd();
    }

    /// <summary>
    /// Cor de fundo e do texto da célula, proporcionais ao valor da métrica.
    /// </summary>
    /// <param name="dezena">Estatística da dezena.</param>
    /// <returns>Estilo CSS em linha.</returns>
    private string Estilo(EstatisticaDezena dezena)
    {
        string cor;
        int intensidade;
        if (this.metrica == MetricaMapa.Desvio)
        {
            double maiorDistancia = this.Estatisticas.Dezenas.Max(item => Math.Abs(item.Desvio));
            cor = dezena.Desvio >= 0 ? "var(--mud-palette-success)" : "var(--mud-palette-error)";
            intensidade = maiorDistancia == 0 ? 0 : (int)Math.Round(Math.Abs(dezena.Desvio) / maiorDistancia * IntensidadeMaxima);
        }
        else
        {
            List<int> valores = this.Estatisticas.Dezenas.Select(this.ValorProporcional).ToList();
            int minimo = valores.Min();
            int amplitude = Math.Max(valores.Max() - minimo, 1);
            cor = "var(--mud-palette-primary)";
            intensidade = IntensidadeBase
                + (int)Math.Round((double)(this.ValorProporcional(dezena) - minimo) / amplitude * IntensidadeMaxima);
        }

        string corTexto = intensidade > IntensidadeTextoClaro ? "#fff" : "var(--mud-palette-text-primary)";
        return $"background: color-mix(in srgb, {cor} {intensidade}%, var(--mud-palette-surface)); color: {corTexto};";
    }

    /// <summary>
    /// Valor da frequência ou do atraso usado na cor; "nunca saiu" recebe o maior atraso do mapa.
    /// </summary>
    /// <param name="dezena">Estatística da dezena.</param>
    /// <returns>Valor da métrica.</returns>
    private int ValorProporcional(EstatisticaDezena dezena)
    {
        if (this.metrica == MetricaMapa.Frequencia)
        {
            return dezena.Sorteios;
        }

        int maiorAtraso = this.Estatisticas.Dezenas.Max(item => item.Atraso) ?? 0;
        return dezena.Atraso ?? maiorAtraso;
    }
}
