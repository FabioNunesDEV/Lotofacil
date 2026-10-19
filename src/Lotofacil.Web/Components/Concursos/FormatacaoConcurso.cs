// <copyright file="FormatacaoConcurso.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;

namespace Lotofacil.Web.Components.Concursos;

/// <summary>
/// Textos exibidos para os valores de um concurso, na cultura atual (pt-BR).
/// </summary>
public static class FormatacaoConcurso
{
    /// <summary>
    /// Texto exibido no lugar de um prêmio quando a faixa não teve ganhador.
    /// </summary>
    public const string SemPremio = "—";

    /// <summary>
    /// Formata a data do sorteio.
    /// </summary>
    /// <param name="data">Data do sorteio.</param>
    /// <returns>Data no formato dd/MM/aaaa.</returns>
    public static string Data(DateOnly data)
    {
        return data.ToString("dd/MM/yyyy");
    }

    /// <summary>
    /// Formata o prêmio de uma faixa.
    /// </summary>
    /// <param name="rateio">Rateio da faixa.</param>
    /// <returns>Valor em reais, ou "—" quando não houve ganhador.</returns>
    public static string Premio(Rateio rateio)
    {
        return rateio.Ganhadores == 0 ? SemPremio : rateio.ValorPremio.ToString("C");
    }

    /// <summary>
    /// Formata a quantidade de ganhadores.
    /// </summary>
    /// <param name="rateio">Rateio da faixa.</param>
    /// <returns>Quantidade com separador de milhar.</returns>
    public static string Ganhadores(Rateio rateio)
    {
        return rateio.Ganhadores.ToString("N0");
    }

    /// <summary>
    /// Formata a arrecadação do concurso.
    /// </summary>
    /// <param name="arrecadacao">Arrecadação total.</param>
    /// <returns>Valor em reais, ou "Não informada" quando o valor gravado é zero.</returns>
    public static string Arrecadacao(decimal arrecadacao)
    {
        return arrecadacao == 0 ? "Não informada" : arrecadacao.ToString("C");
    }
}
