// <copyright file="ConfiguracoesServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Configuracoes;
using Lotofacil.Dominio.Configuracoes;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço das configurações.
/// </summary>
public class ConfiguracoesServicoTestes
{
    /// <summary>
    /// Preço padrão da configuração da aplicação.
    /// </summary>
    private const decimal Padrao = 3.50m;

    /// <summary>
    /// Repositório falso da configuração das apostas.
    /// </summary>
    private readonly IConfiguracaoApostasRepositorio repositorio = Substitute.For<IConfiguracaoApostasRepositorio>();

    /// <summary>
    /// Serviço testado.
    /// </summary>
    private readonly ConfiguracoesServico servico;

    /// <summary>
    /// Monta o serviço com o preço padrão.
    /// </summary>
    public ConfiguracoesServicoTestes()
    {
        this.servico = new ConfiguracoesServico(this.repositorio, Padrao);
    }

    /// <summary>
    /// Sem nada gravado, vale o preço padrão, antes e depois de carregar.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SemLinhaGravadaUsaOPadrao()
    {
        Assert.Equal(Padrao, this.servico.PrecoApostaMinima);

        await this.servico.CarregarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(Padrao, this.servico.PrecoApostaMinima);
    }

    /// <summary>
    /// Depois de carregar, vale o preço gravado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CarregarUsaOPrecoGravado()
    {
        this.repositorio.ObterAsync(Arg.Any<CancellationToken>()).Returns(new ConfiguracaoApostas(4.00m));

        await this.servico.CarregarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(4.00m, this.servico.PrecoApostaMinima);
    }

    /// <summary>
    /// Salvar um preço válido grava e muda o preço em uso na hora.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SalvarPrecoValidoGravaEMudaNaHora()
    {
        bool salvo = await this.servico.SalvarPrecoApostaMinimaAsync(4.00m, TestContext.Current.CancellationToken);

        Assert.True(salvo);
        Assert.Equal(4.00m, this.servico.PrecoApostaMinima);
        await this.repositorio.Received(1).SalvarAsync(
            Arg.Is<ConfiguracaoApostas>(configuracao => configuracao.PrecoApostaMinima == 4.00m),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Salvar um preço inválido não grava e mantém o preço em uso.
    /// </summary>
    /// <param name="valor">Preço informado.</param>
    /// <returns>Tarefa do teste.</returns>
    [Theory]
    [InlineData(0)]
    [InlineData(100.01)]
    [InlineData(3.555)]
    public async Task SalvarPrecoInvalidoNaoGrava(double valor)
    {
        bool salvo = await this.servico.SalvarPrecoApostaMinimaAsync((decimal)valor, TestContext.Current.CancellationToken);

        Assert.False(salvo);
        Assert.Equal(Padrao, this.servico.PrecoApostaMinima);
        await this.repositorio.DidNotReceiveWithAnyArgs().SalvarAsync(default!, TestContext.Current.CancellationToken);
    }
}
