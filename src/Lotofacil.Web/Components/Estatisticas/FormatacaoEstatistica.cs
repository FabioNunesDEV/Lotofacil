// <copyright file="FormatacaoEstatistica.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Web.Components.Estatisticas;

/// <summary>
/// Textos exibidos para as estatísticas das dezenas, na cultura atual (pt-BR).
/// </summary>
public static class FormatacaoEstatistica
{
    /// <summary>
    /// Texto exibido quando a dezena nunca saiu.
    /// </summary>
    public const string Indefinido = "—";

    /// <summary>
    /// Formato com uma casa decimal e sinal "+" nos positivos; o zero fica sem sinal.
    /// </summary>
    private const string FormatoComSinal = "+0.0;-0.0;0.0";

    /// <summary>
    /// Formata uma quantidade de sorteios ou de concursos.
    /// </summary>
    /// <param name="quantidade">Quantidade.</param>
    /// <returns>Número com separador de milhar.</returns>
    public static string Quantidade(int quantidade)
    {
        return quantidade.ToString("N0");
    }

    /// <summary>
    /// Formata o percentual dos concursos.
    /// </summary>
    /// <param name="percentual">Percentual de 0 a 100.</param>
    /// <returns>Percentual com duas casas e "%" (ex.: "60,71%").</returns>
    public static string Percentual(decimal percentual)
    {
        return $"{percentual:N2}%";
    }

    /// <summary>
    /// Formata o valor esperado por dezena.
    /// </summary>
    /// <param name="esperado">Sorteios esperados.</param>
    /// <returns>Valor com uma casa decimal (ex.: "2.284,8").</returns>
    public static string Esperado(decimal esperado)
    {
        return esperado.ToString("N1");
    }

    /// <summary>
    /// Formata a diferença do esperado.
    /// </summary>
    /// <param name="diferenca">Sorteios menos o esperado.</param>
    /// <returns>Valor com uma casa e sinal (ex.: "+27,2", "-0,8", "0,0").</returns>
    public static string Diferenca(decimal diferenca)
    {
        // Arredonda antes, para um valor como -0,04 não aparecer como "-0,0".
        return Math.Round(diferenca, 1, MidpointRounding.AwayFromZero).ToString(FormatoComSinal);
    }

    /// <summary>
    /// Formata o desvio em desvios-padrão.
    /// </summary>
    /// <param name="desvio">Desvio.</param>
    /// <returns>Valor com uma casa e sinal (ex.: "+0,9").</returns>
    public static string Desvio(double desvio)
    {
        return Math.Round(desvio, 1, MidpointRounding.AwayFromZero).ToString(FormatoComSinal);
    }

    /// <summary>
    /// Classe CSS que destaca a dezena acima ou abaixo do esperado; o zero fica sem destaque.
    /// </summary>
    /// <param name="diferenca">Sorteios menos o esperado.</param>
    /// <returns>"acima", "abaixo" ou vazio.</returns>
    public static string ClasseDiferenca(decimal diferenca)
    {
        return Math.Sign(diferenca) switch
        {
            > 0 => "acima",
            < 0 => "abaixo",
            _ => string.Empty,
        };
    }

    /// <summary>
    /// Formata um número de concurso ou de atraso, ou "—" quando indefinido.
    /// </summary>
    /// <param name="numero">Número, ou <c>null</c>.</param>
    /// <returns>Número sem separador de milhar, ou "—".</returns>
    public static string NumeroOuIndefinido(int? numero)
    {
        return numero?.ToString() ?? Indefinido;
    }

    /// <summary>
    /// Formata o atraso no mapa de calor.
    /// </summary>
    /// <param name="atraso">Atraso, ou <c>null</c> se a dezena nunca saiu.</param>
    /// <returns>Atraso em concursos (ex.: "18 conc."), ou "—".</returns>
    public static string AtrasoNoMapa(int? atraso)
    {
        return atraso is int valor ? $"{valor} conc." : Indefinido;
    }

    /// <summary>
    /// Formata o atraso nos rankings.
    /// </summary>
    /// <param name="atraso">Atraso, ou <c>null</c> se a dezena nunca saiu.</param>
    /// <returns>"último concurso", "&lt;n&gt; conc." ou "nunca saiu".</returns>
    public static string AtrasoNoRanking(int? atraso)
    {
        return atraso switch
        {
            null => "nunca saiu",
            0 => "último concurso",
            _ => $"{atraso} conc.",
        };
    }
}
