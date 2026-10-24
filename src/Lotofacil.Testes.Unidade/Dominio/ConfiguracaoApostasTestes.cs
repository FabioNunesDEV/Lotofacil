// <copyright file="ConfiguracaoApostasTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Configuracoes;

namespace Lotofacil.Testes.Unidade.Dominio;

/// <summary>
/// Testes das regras do preço da aposta mínima.
/// </summary>
public class ConfiguracaoApostasTestes
{
    /// <summary>
    /// Preços nos limites da faixa, com até dois decimais, são aceitos.
    /// </summary>
    /// <param name="valor">Preço informado.</param>
    [Theory]
    [InlineData(0.01)]
    [InlineData(3.5)]
    [InlineData(100.00)]
    public void AceitaPrecoValido(double valor)
    {
        decimal preco = (decimal)valor;

        var configuracao = new ConfiguracaoApostas(preco);

        Assert.True(ConfiguracaoApostas.PrecoValido(preco));
        Assert.Equal(preco, configuracao.PrecoApostaMinima);
        Assert.Equal(ConfiguracaoApostas.IdUnico, configuracao.Id);
    }

    /// <summary>
    /// Preços zerados, negativos, acima do máximo ou com mais de dois decimais são recusados.
    /// </summary>
    /// <param name="valor">Preço informado.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.01)]
    [InlineData(3.555)]
    public void RecusaPrecoInvalido(double valor)
    {
        decimal preco = (decimal)valor;

        Assert.False(ConfiguracaoApostas.PrecoValido(preco));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConfiguracaoApostas(preco));
    }
}
