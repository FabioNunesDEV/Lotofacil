// <copyright file="FormatacaoTeimosinhaTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using System.Globalization;
using Lotofacil.Aplicacao.Jogos;
using Lotofacil.Aplicacao.Teimosinhas;
using Lotofacil.Web.Components.Geracao;
using Lotofacil.Web.Components.Teimosinhas;

namespace Lotofacil.Testes.Unidade.Web;

/// <summary>
/// Testes dos textos das Teimosinhas.
/// </summary>
public class FormatacaoTeimosinhaTestes
{
    /// <summary>
    /// Usa a cultura da aplicação.
    /// </summary>
    public FormatacaoTeimosinhaTestes()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
    }

    /// <summary>
    /// Aviso dos jogos criados, para um e para vários concursos, no singular e no plural.
    /// </summary>
    /// <param name="quantidade">Jogos criados.</param>
    /// <param name="primeiro">Primeiro concurso.</param>
    /// <param name="ultimo">Último concurso.</param>
    /// <param name="esperado">Texto esperado.</param>
    [Theory]
    [InlineData(2, 3814, 3814, "Teimosinhas: 2 jogos criados para o concurso 3814.")]
    [InlineData(3, 3814, 3816, "Teimosinhas: 3 jogos criados para os concursos 3814 a 3816.")]
    [InlineData(1, 3814, 3814, "Teimosinhas: 1 jogo criado para o concurso 3814.")]
    public void Criados(int quantidade, int primeiro, int ultimo, string esperado)
    {
        Assert.Equal(esperado, FormatacaoTeimosinha.Criados(new JogosCriadosTeimosinha(quantidade, primeiro, ultimo)));
    }

    /// <summary>
    /// Linhas da página e marca de Meus jogos.
    /// </summary>
    [Fact]
    public void TextosDaPaginaEDaMarca()
    {
        Assert.Equal("Desde o concurso 3813", FormatacaoTeimosinha.Desde(3813));
        Assert.Equal("2 jogos · R$ 59,50 por concurso", FormatacaoTeimosinha.JogosECusto(2, 59.50m).Replace(' ', ' '));
        Assert.Equal("1 jogo · R$ 3,50 por concurso", FormatacaoTeimosinha.JogosECusto(1, 3.50m).Replace(' ', ' '));
        Assert.Equal("Teimosinha · Minhas fixas", FormatacaoTeimosinha.Marca("Minhas fixas"));
        Assert.Equal(
            "Jogo salvo para o concurso 3813 na Teimosinha Minhas fixas.",
            FormatacaoTeimosinha.JogoSalvo(3813, "Minhas fixas"));
    }

    /// <summary>
    /// O resumo de Gerar jogos traz a Teimosinha logo depois do concurso.
    /// </summary>
    /// <param name="duplicados">Jogos recusados.</param>
    /// <param name="esperado">Texto esperado.</param>
    [Theory]
    [InlineData(0, "3 jogos salvos para o concurso 3813 na Teimosinha Geradas.")]
    [InlineData(1, "3 jogos salvos para o concurso 3813 na Teimosinha Geradas; 1 já estava cadastrado.")]
    public void SalvamentoNaTeimosinha(int duplicados, string esperado)
    {
        Assert.Equal(
            esperado,
            FormatacaoGeracao.Salvamento(new ResultadoCadastroVarios(3, duplicados, null), 3813, "Geradas"));
    }
}
