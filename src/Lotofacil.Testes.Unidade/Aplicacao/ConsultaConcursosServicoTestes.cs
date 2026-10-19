// <copyright file="ConsultaConcursosServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio.Concursos;
using NSubstitute;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço de consulta de concursos.
/// </summary>
public class ConsultaConcursosServicoTestes
{
    /// <summary>
    /// Repositório falso usado pelo serviço.
    /// </summary>
    private readonly IConcursoRepositorio repositorio = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// A página é convertida em "pular/pegar" e o total vem da contagem da base.
    /// </summary>
    /// <param name="pagina">Índice da página.</param>
    /// <param name="tamanhoPagina">Tamanho da página.</param>
    /// <param name="ignorarEsperado">Quantidade a pular esperada.</param>
    /// <returns>Tarefa do teste.</returns>
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 10)]
    [InlineData(2, 25, 50)]
    public async Task ListarPaginaConverteEmPularEPegar(int pagina, int tamanhoPagina, int ignorarEsperado)
    {
        Concurso concurso = ConcursoFabrica.Criar();
        this.repositorio.ListarAsync(ignorarEsperado, tamanhoPagina, Arg.Any<CancellationToken>()).Returns([concurso]);
        this.repositorio.ContarAsync(Arg.Any<CancellationToken>()).Returns(23);

        PaginaConcursos resultado = await this.CriarServico().ListarPaginaAsync(pagina, tamanhoPagina, TestContext.Current.CancellationToken);

        Assert.Equal([concurso], resultado.Itens);
        Assert.Equal(23, resultado.Total);
    }

    /// <summary>
    /// Página negativa ou tamanho fora de 1 a 100 é rejeitado sem consultar a base.
    /// </summary>
    /// <param name="pagina">Índice da página.</param>
    /// <param name="tamanhoPagina">Tamanho da página.</param>
    /// <returns>Tarefa do teste.</returns>
    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    [InlineData(0, 101)]
    public async Task ListarPaginaRejeitaArgumentosInvalidos(int pagina, int tamanhoPagina)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => this.CriarServico().ListarPaginaAsync(pagina, tamanhoPagina, TestContext.Current.CancellationToken));

        await this.repositorio.DidNotReceiveWithAnyArgs().ListarAsync(default, default, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// A busca por número devolve o concurso da base.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ObterDevolveConcursoDaBase()
    {
        Concurso concurso = ConcursoFabrica.Criar(numero: 3805);
        this.repositorio.ObterAsync(3805, Arg.Any<CancellationToken>()).Returns(concurso);

        Concurso? resultado = await this.CriarServico().ObterAsync(3805, TestContext.Current.CancellationToken);

        Assert.Same(concurso, resultado);
    }

    /// <summary>
    /// A busca por número inexistente devolve <c>null</c>.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ObterDevolveNuloQuandoNaoEncontra()
    {
        Concurso? resultado = await this.CriarServico().ObterAsync(9999, TestContext.Current.CancellationToken);

        Assert.Null(resultado);
    }

    /// <summary>
    /// Cria o serviço com o repositório falso.
    /// </summary>
    /// <returns>Serviço de consulta.</returns>
    private ConsultaConcursosServico CriarServico()
    {
        return new ConsultaConcursosServico(this.repositorio);
    }
}
