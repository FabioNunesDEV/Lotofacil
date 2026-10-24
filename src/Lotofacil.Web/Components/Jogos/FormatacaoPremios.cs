// <copyright file="FormatacaoPremios.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Dominio;
using Lotofacil.Web.Components.Concursos;

namespace Lotofacil.Web.Components.Jogos;

/// <summary>
/// Formatos dos textos do aviso de jogos premiados, na cultura atual.
/// </summary>
public static class FormatacaoPremios
{
    /// <summary>
    /// Resumo de quantos jogos conferidos foram premiados.
    /// </summary>
    /// <param name="premiados">Jogos premiados.</param>
    /// <param name="conferidos">Jogos conferidos.</param>
    /// <returns>Texto como "3 de 5 jogos conferidos foram premiados.".</returns>
    public static string Resumo(int premiados, int conferidos)
    {
        if (conferidos == 1)
        {
            return "1 de 1 jogo conferido foi premiado.";
        }

        return premiados == 1
            ? $"1 de {conferidos} jogos conferidos foi premiado."
            : $"{premiados} de {conferidos} jogos conferidos foram premiados.";
    }

    /// <summary>
    /// Cabeçalho do concurso no diálogo.
    /// </summary>
    /// <param name="grupo">Jogos premiados de um concurso sorteado.</param>
    /// <returns>Texto como "Concurso 3809 · 20/10/2026".</returns>
    public static string Concurso(GrupoJogos grupo)
    {
        return grupo.Concurso is null
            ? $"Concurso {grupo.NumeroConcurso}"
            : $"Concurso {grupo.NumeroConcurso} · {FormatacaoConcurso.Data(grupo.Concurso.Data)}";
    }

    /// <summary>
    /// Prazo para receber os prêmios do concurso.
    /// </summary>
    /// <param name="grupo">Jogos premiados de um concurso sorteado.</param>
    /// <param name="hoje">Data de referência.</param>
    /// <returns>"Receber até 18/01/2027" ou "Prazo encerrado em 18/01/2027".</returns>
    public static string Prazo(GrupoJogos grupo, DateOnly hoje)
    {
        if (grupo.Prescricao is not DateOnly prescricao)
        {
            return string.Empty;
        }

        string data = FormatacaoConcurso.Data(prescricao);
        return grupo.DentroDoPrazo(hoje) ? $"Receber até {data}" : $"Prazo encerrado em {data}";
    }

    /// <summary>
    /// Linha de um jogo premiado.
    /// </summary>
    /// <param name="jogo">Jogo conferido e premiado.</param>
    /// <returns>Texto como "Jogo 02 · 13 acertos · R$ 35,00" ou "Jogo 01 · 16 dezenas · 13 acertos · R$ 287,00".</returns>
    public static string Jogo(JogoConferido jogo)
    {
        List<string> partes = [$"Jogo {FormatacaoJogo.NumeroJogo(jogo.Numero)}"];
        int dezenas = jogo.Jogo.Dezenas.Count;
        if (dezenas > RegrasLotofacil.MinimoDezenasPorJogo)
        {
            partes.Add(FormatacaoJogo.QuantidadeDezenas(dezenas));
        }

        if (jogo.Conferencia is not null)
        {
            partes.Add(FormatacaoJogo.Acertos(jogo.Conferencia.Acertos));
            partes.Add(FormatacaoJogo.Reais(jogo.Conferencia.Premio));
        }

        return string.Join(" · ", partes);
    }

    /// <summary>
    /// Total dos jogos premiados.
    /// </summary>
    /// <param name="premiados">Jogos premiados.</param>
    /// <param name="total">Soma dos prêmios.</param>
    /// <returns>Texto como "Total: 3 jogos · R$ 49,00".</returns>
    public static string Total(int premiados, decimal total)
    {
        return $"Total: {FormatacaoJogo.QuantidadeJogos(premiados)} · {FormatacaoJogo.Reais(total)}";
    }

    /// <summary>
    /// Aviso de que os jogos dos concursos novos foram conferidos sem prêmio.
    /// </summary>
    /// <param name="conferidos">Jogos conferidos.</param>
    /// <returns>Texto do aviso.</returns>
    public static string NenhumPremio(int conferidos)
    {
        return conferidos == 1
            ? "Seu jogo dos concursos novos foi conferido e não foi premiado."
            : $"Seus {conferidos} jogos dos concursos novos foram conferidos; nenhum foi premiado.";
    }
}
