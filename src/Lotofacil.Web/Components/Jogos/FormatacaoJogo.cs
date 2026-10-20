// <copyright file="FormatacaoJogo.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Textos exibidos para os jogos do usuário, na cultura atual (pt-BR).
/// </summary>
public static class FormatacaoJogo
{
    /// <summary>
    /// Texto exibido quando o jogo não tem prêmio.
    /// </summary>
    public const string SemPremio = "—";

    /// <summary>
    /// Formata a quantidade de jogos, no singular ou no plural.
    /// </summary>
    /// <param name="quantidade">Quantidade de jogos.</param>
    /// <returns>"1 jogo" ou "&lt;n&gt; jogos".</returns>
    public static string QuantidadeJogos(int quantidade)
    {
        return quantidade == 1 ? "1 jogo" : $"{quantidade:N0} jogos";
    }

    /// <summary>
    /// Formata a quantidade de concursos, no singular ou no plural.
    /// </summary>
    /// <param name="quantidade">Quantidade de concursos.</param>
    /// <returns>"1 concurso" ou "&lt;n&gt; concursos".</returns>
    public static string QuantidadeConcursos(int quantidade)
    {
        return quantidade == 1 ? "1 concurso" : $"{quantidade:N0} concursos";
    }

    /// <summary>
    /// Formata a quantidade de apostas de 15 dezenas, no singular ou no plural.
    /// </summary>
    /// <param name="quantidade">Quantidade de apostas.</param>
    /// <returns>"1 aposta de 15 dezenas" ou "&lt;n&gt; apostas de 15 dezenas".</returns>
    public static string QuantidadeApostas(int quantidade)
    {
        return quantidade == 1 ? "1 aposta de 15 dezenas" : $"{quantidade:N0} apostas de 15 dezenas";
    }

    /// <summary>
    /// Formata a posição do jogo no concurso com dois dígitos.
    /// </summary>
    /// <param name="numero">Posição do jogo, começando em 1.</param>
    /// <returns>Número com dois dígitos (ex.: "01").</returns>
    public static string NumeroJogo(int numero)
    {
        return numero.ToString("00");
    }

    /// <summary>
    /// Formata a quantidade de acertos.
    /// </summary>
    /// <param name="acertos">Acertos do jogo.</param>
    /// <returns>Texto como "13 acertos".</returns>
    public static string Acertos(int acertos)
    {
        return $"{acertos} acertos";
    }

    /// <summary>
    /// Formata um valor em reais.
    /// </summary>
    /// <param name="valor">Valor.</param>
    /// <returns>Valor como "R$ 1.234,56".</returns>
    public static string Reais(decimal valor)
    {
        return valor.ToString("C");
    }

    /// <summary>
    /// Formata o prêmio de um jogo, ou "—" sem prêmio.
    /// </summary>
    /// <param name="premio">Prêmio do jogo.</param>
    /// <returns>Valor em reais ou "—".</returns>
    public static string Premio(decimal premio)
    {
        return premio > 0 ? Reais(premio) : SemPremio;
    }
}
