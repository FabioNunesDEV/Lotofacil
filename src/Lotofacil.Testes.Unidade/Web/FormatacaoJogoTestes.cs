// <copyright file="FormatacaoJogoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Dominio.Jogos;
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

    /// <summary>
    /// Contagem do volante no cadastro, com o custo da aposta.
    /// </summary>
    [Fact]
    public void ContagemComCusto()
    {
        Assert.Equal("14 de 15 dezenas · R$ 3,50", FormatacaoJogo.Contagem(14, 15, 3.5m).Replace(' ', ' '));
        Assert.Equal("12 de 17 dezenas · R$ 476,00", FormatacaoJogo.Contagem(12, 17, 476m).Replace(' ', ' '));
    }

    /// <summary>
    /// Quantidade de dezenas e tamanho da aposta, com as apostas equivalentes só acima de 15 dezenas.
    /// </summary>
    [Fact]
    public void DezenasEAposta()
    {
        Assert.Equal("16 dezenas", FormatacaoJogo.QuantidadeDezenas(16));
        Assert.Equal("15 dezenas", FormatacaoJogo.Aposta(15, 1));
        Assert.Equal("16 dezenas · 16 apostas", FormatacaoJogo.Aposta(16, 16));
        Assert.Equal("19 dezenas · 3.876 apostas", FormatacaoJogo.Aposta(19, 3_876));
    }

    /// <summary>
    /// Prêmios de uma faixa: quantidade, faixa e valor de cada prêmio.
    /// </summary>
    [Fact]
    public void PremiosDaFaixa()
    {
        Assert.Equal("3 × 13 acertos · R$ 35,00", FormatacaoJogo.PremiosDaFaixa(new PremioFaixa(13, 3, 35m)).Replace(' ', ' '));
        Assert.Equal("1.365 × 12 acertos · R$ 14,00", FormatacaoJogo.PremiosDaFaixa(new PremioFaixa(12, 1_365, 14m)).Replace(' ', ' '));
    }
}
