// <copyright file="FormatacaoPerfil.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Dominio.Estatisticas.Perfis;

namespace Lotofacil.Web.Components.Estatisticas;

/// <summary>
/// Textos exibidos nos cartões de perfil do concurso.
/// </summary>
public static class FormatacaoPerfil
{
    /// <summary>
    /// Texto do atraso quando a linha saiu no último concurso.
    /// </summary>
    public const string Ultimo = "último";

    /// <summary>
    /// Título do cartão do perfil.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Título exibido.</returns>
    public static string Titulo(TipoPerfil tipo)
    {
        return tipo switch
        {
            TipoPerfil.Pares => "Pares e ímpares",
            TipoPerfil.Primos => "Números primos",
            TipoPerfil.MultiplosDe3 => "Múltiplos de 3",
            TipoPerfil.Fibonacci => "Fibonacci",
            TipoPerfil.Soma => "Soma das dezenas",
            TipoPerfil.Repetidas => "Repetidas do concurso anterior",
            TipoPerfil.Moldura => "Moldura e miolo",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };
    }

    /// <summary>
    /// Descrição do cartão do perfil.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <returns>Descrição exibida abaixo do título.</returns>
    public static string Descricao(TipoPerfil tipo)
    {
        return tipo switch
        {
            TipoPerfil.Pares => "12 pares e 13 ímpares no volante",
            TipoPerfil.Primos => "02 03 05 07 11 13 17 19 23",
            TipoPerfil.MultiplosDe3 => "03 06 09 12 15 18 21 24",
            TipoPerfil.Fibonacci => "01 02 03 05 08 13 21",
            TipoPerfil.Soma => "Soma das 15 dezenas, em faixas de 15",
            TipoPerfil.Repetidas => "Dezenas que também saíram no concurso anterior",
            TipoPerfil.Moldura => "Moldura: as 16 dezenas da borda; miolo: 07 08 09 12 13 14 17 18 19",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };
    }

    /// <summary>
    /// Rótulo da linha: simples ("7 pares", "165–179") ou com a ponta agrupada ("≤ 4 pares", "≥ 225").
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="faixa">Linha do perfil.</param>
    /// <returns>Rótulo exibido na coluna "Faixa".</returns>
    public static string Rotulo(TipoPerfil tipo, FaixaPerfil faixa)
    {
        string sufixo = tipo == TipoPerfil.Soma ? string.Empty : $" {Unidade(tipo)}";

        if (faixa.AbertaAbaixo)
        {
            return $"≤ {faixa.Maximo}{sufixo}";
        }

        if (faixa.AbertaAcima)
        {
            return $"≥ {faixa.Minimo}{sufixo}";
        }

        return tipo == TipoPerfil.Soma ? $"{faixa.Minimo}–{faixa.Maximo}" : $"{faixa.Minimo}{sufixo}";
    }

    /// <summary>
    /// Formata o atraso da linha.
    /// </summary>
    /// <param name="atraso">Atraso, ou <c>null</c> se a linha nunca saiu.</param>
    /// <returns>"último", o atraso em concursos ou "—".</returns>
    public static string Atraso(int? atraso)
    {
        return atraso == 0 ? Ultimo : FormatacaoEstatistica.NumeroOuIndefinido(atraso);
    }

    /// <summary>
    /// Posição de um percentual na barra do cartão, para usar em CSS.
    /// </summary>
    /// <param name="percentual">Percentual observado ou teórico.</param>
    /// <param name="escala">Percentual que corresponde à barra inteira.</param>
    /// <returns>Percentual da barra com ponto decimal (ex.: "41.67%").</returns>
    public static string PosicaoNaBarra(decimal percentual, decimal escala)
    {
        decimal posicao = percentual / escala * 100;

        return posicao.ToString("0.##", CultureInfo.InvariantCulture) + "%";
    }

    /// <summary>
    /// Palavra que acompanha a quantidade nos rótulos.
    /// </summary>
    /// <param name="tipo">Perfil de quantidade.</param>
    /// <returns>Palavra do rótulo (ex.: "pares", "na moldura").</returns>
    private static string Unidade(TipoPerfil tipo)
    {
        return tipo switch
        {
            TipoPerfil.Pares => "pares",
            TipoPerfil.Primos => "primos",
            TipoPerfil.MultiplosDe3 => "múltiplos",
            TipoPerfil.Fibonacci => "de Fibonacci",
            TipoPerfil.Repetidas => "repetidas",
            TipoPerfil.Moldura => "na moldura",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };
    }
}
