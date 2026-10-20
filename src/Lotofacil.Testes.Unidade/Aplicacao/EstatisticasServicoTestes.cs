// <copyright file="EstatisticasServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Estatisticas;
using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Estatisticas;
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
    /// Cria o serviço com o repositório falso.
    /// </summary>
    /// <returns>Serviço testado.</returns>
    private EstatisticasServico CriarServico()
    {
        return new EstatisticasServico(this.repositorio);
    }
}
