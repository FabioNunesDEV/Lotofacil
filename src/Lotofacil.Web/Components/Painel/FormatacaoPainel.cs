// <copyright file="FormatacaoPainel.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Web.Components.Concursos;
using Lotofacil.Web.Components.Jogos;

namespace Lotofacil.Web.Components.Painel;

/// <summary>
/// Formatos dos textos dos indicadores e cartões do Painel, na cultura atual.
/// </summary>
public static class FormatacaoPainel
{
    /// <summary>
    /// Detalhe do indicador de concursos: data do último sorteio gravado.
    /// </summary>
    /// <param name="data">Data do último concurso gravado.</param>
    /// <returns>Texto "Último sorteio em dd/mm/aaaa".</returns>
    public static string UltimoSorteio(DateOnly data)
    {
        return $"Último sorteio em {FormatacaoConcurso.Data(data)}";
    }

    /// <summary>
    /// Detalhe do indicador do próximo concurso: estimativa de prêmio gravada no último concurso.
    /// </summary>
    /// <param name="estimativa">Estimativa gravada; zero quando não informada.</param>
    /// <returns>Texto da estimativa.</returns>
    public static string Estimativa(decimal estimativa)
    {
        return estimativa == 0 ? "Estimativa não informada" : $"Estimativa {FormatacaoJogo.Reais(estimativa)}";
    }

    /// <summary>
    /// Detalhe do indicador de jogos aguardando: o concurso, a quantidade de concursos ou nenhum.
    /// </summary>
    /// <param name="concursos">Números dos concursos com jogos aguardando sorteio.</param>
    /// <returns>Texto do detalhe.</returns>
    public static string ConcursosAguardando(IReadOnlyList<int> concursos)
    {
        return concursos.Count switch
        {
            0 => "Nenhum jogo aguardando sorteio",
            1 => $"Concurso {concursos[0]}",
            _ => FormatacaoJogo.QuantidadeConcursos(concursos.Count),
        };
    }

    /// <summary>
    /// Ganhadores da faixa de 15 acertos, ou "Acumulou" quando não houve ganhador.
    /// </summary>
    /// <param name="rateio">Rateio da faixa de 15 acertos.</param>
    /// <returns>Texto dos ganhadores.</returns>
    public static string Ganhadores15(Rateio rateio)
    {
        return rateio.Ganhadores switch
        {
            0 => "Acumulou",
            1 => "1 ganhador",
            _ => $"{rateio.Ganhadores:N0} ganhadores",
        };
    }
}
