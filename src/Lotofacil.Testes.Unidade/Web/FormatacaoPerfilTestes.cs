// <copyright file="FormatacaoPerfilTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Dominio.Estatisticas.Perfis;
using Lotofacil.Web.Components.Estatisticas;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes dos textos dos cartões de perfil, na cultura pt-BR.
/// </summary>
public class FormatacaoPerfilTestes
{
    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public FormatacaoPerfilTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Títulos e descrições da spec.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="titulo">Título esperado.</param>
    /// <param name="descricao">Descrição esperada.</param>
    [Theory]
    [InlineData(TipoPerfil.Pares, "Pares e ímpares", "12 pares e 13 ímpares no volante")]
    [InlineData(TipoPerfil.Primos, "Números primos", "02 03 05 07 11 13 17 19 23")]
    [InlineData(TipoPerfil.MultiplosDe3, "Múltiplos de 3", "03 06 09 12 15 18 21 24")]
    [InlineData(TipoPerfil.Fibonacci, "Fibonacci", "01 02 03 05 08 13 21")]
    [InlineData(TipoPerfil.Soma, "Soma das dezenas", "Soma das 15 dezenas, em faixas de 15")]
    [InlineData(
        TipoPerfil.Repetidas,
        "Repetidas do concurso anterior",
        "Dezenas que também saíram no concurso anterior")]
    [InlineData(
        TipoPerfil.Moldura,
        "Moldura e miolo",
        "Moldura: as 16 dezenas da borda; miolo: 07 08 09 12 13 14 17 18 19")]
    public void TituloEDescricao(TipoPerfil tipo, string titulo, string descricao)
    {
        Assert.Equal(titulo, FormatacaoPerfil.Titulo(tipo));
        Assert.Equal(descricao, FormatacaoPerfil.Descricao(tipo));
    }

    /// <summary>
    /// Rótulos simples de quantidade e de faixa de soma.
    /// </summary>
    /// <param name="tipo">Perfil.</param>
    /// <param name="minimo">Menor valor da linha.</param>
    /// <param name="maximo">Maior valor da linha.</param>
    /// <param name="esperado">Rótulo esperado.</param>
    [Theory]
    [InlineData(TipoPerfil.Pares, 7, 7, "7 pares")]
    [InlineData(TipoPerfil.Primos, 5, 5, "5 primos")]
    [InlineData(TipoPerfil.MultiplosDe3, 5, 5, "5 múltiplos")]
    [InlineData(TipoPerfil.Fibonacci, 7, 7, "7 de Fibonacci")]
    [InlineData(TipoPerfil.Soma, 165, 179, "165–179")]
    [InlineData(TipoPerfil.Repetidas, 9, 9, "9 repetidas")]
    [InlineData(TipoPerfil.Moldura, 10, 10, "10 na moldura")]
    public void RotuloSimples(TipoPerfil tipo, int minimo, int maximo, string esperado)
    {
        Assert.Equal(esperado, FormatacaoPerfil.Rotulo(tipo, Faixa(minimo, maximo, abaixo: false, acima: false)));
    }

    /// <summary>
    /// Rótulos com as pontas agrupadas.
    /// </summary>
    [Fact]
    public void RotuloAgrupado()
    {
        Assert.Equal("≤ 4 pares", FormatacaoPerfil.Rotulo(TipoPerfil.Pares, Faixa(2, 4, abaixo: true, acima: false)));
        Assert.Equal("≥ 10 pares", FormatacaoPerfil.Rotulo(TipoPerfil.Pares, Faixa(10, 12, abaixo: false, acima: true)));
        Assert.Equal("≤ 164", FormatacaoPerfil.Rotulo(TipoPerfil.Soma, Faixa(120, 164, abaixo: true, acima: false)));
        Assert.Equal("≥ 225", FormatacaoPerfil.Rotulo(TipoPerfil.Soma, Faixa(225, 270, abaixo: false, acima: true)));
        Assert.Equal(
            "≤ 2 de Fibonacci",
            FormatacaoPerfil.Rotulo(TipoPerfil.Fibonacci, Faixa(0, 2, abaixo: true, acima: false)));
    }

    /// <summary>
    /// Atraso: "último" no zero, o número nos demais e "—" quando nunca saiu.
    /// </summary>
    [Fact]
    public void Atraso()
    {
        Assert.Equal("último", FormatacaoPerfil.Atraso(0));
        Assert.Equal("42", FormatacaoPerfil.Atraso(42));
        Assert.Equal("1234", FormatacaoPerfil.Atraso(1234));
        Assert.Equal("—", FormatacaoPerfil.Atraso(null));
    }

    /// <summary>
    /// A posição na barra usa ponto decimal, independente da cultura.
    /// </summary>
    [Fact]
    public void PosicaoNaBarraUsaPontoDecimal()
    {
        Assert.Equal("50%", FormatacaoPerfil.PosicaoNaBarra(15m, 30m));
        Assert.Equal("33.33%", FormatacaoPerfil.PosicaoNaBarra(10m, 30m));
        Assert.Equal("0%", FormatacaoPerfil.PosicaoNaBarra(0m, 30m));
    }

    /// <summary>
    /// Cria uma linha só com os limites e as pontas.
    /// </summary>
    /// <param name="minimo">Menor valor.</param>
    /// <param name="maximo">Maior valor.</param>
    /// <param name="abaixo">Se agrupou a ponta de baixo.</param>
    /// <param name="acima">Se agrupou a ponta de cima.</param>
    /// <returns>Linha do perfil.</returns>
    private static FaixaPerfil Faixa(int minimo, int maximo, bool abaixo, bool acima)
    {
        return new FaixaPerfil(minimo, maximo, abaixo, acima, Observado: 0, Teorico: 0, Atraso: null);
    }
}
