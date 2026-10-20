// <copyright file="FormatacaoJogoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Web.Components.Jogos;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes dos formatos de exibição dos jogos, na cultura pt-BR.
/// </summary>
public class FormatacaoJogoTestes
{
    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public FormatacaoJogoTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Quantidade de jogos no singular e no plural.
    /// </summary>
    [Fact]
    public void QuantidadeDeJogos()
    {
        Assert.Equal("1 jogo", FormatacaoJogo.QuantidadeJogos(1));
        Assert.Equal("3 jogos", FormatacaoJogo.QuantidadeJogos(3));
        Assert.Equal("1.200 jogos", FormatacaoJogo.QuantidadeJogos(1200));
    }

    /// <summary>
    /// Quantidade de concursos e de apostas no singular e no plural.
    /// </summary>
    [Fact]
    public void QuantidadeDeConcursosEApostas()
    {
        Assert.Equal("1 concurso", FormatacaoJogo.QuantidadeConcursos(1));
        Assert.Equal("2 concursos", FormatacaoJogo.QuantidadeConcursos(2));
        Assert.Equal("1 aposta de 15 dezenas", FormatacaoJogo.QuantidadeApostas(1));
        Assert.Equal("5 apostas de 15 dezenas", FormatacaoJogo.QuantidadeApostas(5));
    }

    /// <summary>
    /// Número do jogo, acertos, reais e prêmio.
    /// </summary>
    [Fact]
    public void NumeroAcertosEValores()
    {
        Assert.Equal("02", FormatacaoJogo.NumeroJogo(2));
        Assert.Equal("13 acertos", FormatacaoJogo.Acertos(13));
        Assert.Equal("R$ 10,50", FormatacaoJogo.Reais(10.5m).Replace(' ', ' '));
        Assert.Equal("R$ 1.920,13", FormatacaoJogo.Premio(1920.13m).Replace(' ', ' '));
        Assert.Equal("—", FormatacaoJogo.Premio(0m));
    }
}
