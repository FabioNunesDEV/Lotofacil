// <copyright file="EstatisticasServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;
using Lotofacil.Dominio.Estatisticas.Perfis;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço de estatísticas.
/// </summary>
public class EstatisticasServicoTestes
{
    /// <summary>
    /// Repositório falso dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Com a base vazia, não há estatísticas.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BaseVaziaDevolveNulo()
    {
        this.repositorio.ListarResultadosAsync(Arg.Any<CancellationToken>()).Returns([]);

        EstatisticasDezenas? estatisticas = await this.CriarServico().CalcularAsync(null, TestContext.Current.CancellationToken);

        Assert.Null(estatisticas);
    }

    /// <summary>
    /// A janela pedida é usada no cálculo.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task UsaAJanelaPedida()
    {
        List<ResultadoSorteio> historico = Enumerable.Range(1, 30)
            .Select(numero => new ResultadoSorteio(numero, ConcursoFabrica.DezenasPadrao))
            .ToList();
        this.repositorio.ListarResultadosAsync(Arg.Any<CancellationToken>()).Returns(historico);

        EstatisticasDezenas? estatisticas = await this.CriarServico().CalcularAsync(10, TestContext.Current.CancellationToken);

        Assert.NotNull(estatisticas);
        Assert.Equal(10, estatisticas.ConcursosNaJanela);
        Assert.Equal(21, estatisticas.PrimeiroConcurso);
    }

    /// <summary>
    /// Janela menor que 1 é rejeitada sem consultar a base.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task JanelaInvalidaERejeitada()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => this.CriarServico().CalcularAsync(0, TestContext.Current.CancellationToken));

        await this.repositorio.DidNotReceiveWithAnyArgs().ListarResultadosAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Com a base vazia, não há perfis.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task PerfisComBaseVaziaDevolveNulo()
    {
        this.repositorio.ListarResultadosAsync(Arg.Any<CancellationToken>()).Returns([]);

        IReadOnlyList<DistribuicaoPerfil>? perfis =
            await this.CriarServico().CalcularPerfisAsync(null, TestContext.Current.CancellationToken);

        Assert.Null(perfis);
    }

    /// <summary>
    /// Os perfis usam a janela pedida: com 10 concursos, o observado de cada perfil soma 100% sobre eles.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task PerfisUsamAJanelaPedida()
    {
        // Os concursos 1 a 20 têm as dezenas 01 a 15 (7 pares); os 10 últimos, as 10 a 24 (8 pares).
        List<ResultadoSorteio> historico = Enumerable.Range(1, 30)
            .Select(numero => new ResultadoSorteio(
                numero,
                numero <= 20 ? Enumerable.Range(1, 15).ToArray() : Enumerable.Range(10, 15).ToArray()))
            .ToList();
        this.repositorio.ListarResultadosAsync(Arg.Any<CancellationToken>()).Returns(historico);

        IReadOnlyList<DistribuicaoPerfil>? perfis =
            await this.CriarServico().CalcularPerfisAsync(10, TestContext.Current.CancellationToken);

        Assert.NotNull(perfis);
        Assert.Equal(7, perfis.Count);
        IReadOnlyList<FaixaPerfil> pares = perfis[0].Faixas;
        Assert.Equal(100m, pares.Single(faixa => faixa.Contem(8)).Observado);
        Assert.Equal(0m, pares.Single(faixa => faixa.Contem(7)).Observado);
    }

    /// <summary>
    /// Janela menor que 1 é rejeitada nos perfis sem consultar a base.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task PerfisComJanelaInvalidaSaoRejeitados()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => this.CriarServico().CalcularPerfisAsync(0, TestContext.Current.CancellationToken));

        await this.repositorio.DidNotReceiveWithAnyArgs().ListarResultadosAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Cria o serviço com o repositório falso.
    /// </summary>
    /// <returns>Serviço testado.</returns>
    private EstatisticasServico CriarServico()
    {
        return new EstatisticasServico(this.repositorio);
    }
}
