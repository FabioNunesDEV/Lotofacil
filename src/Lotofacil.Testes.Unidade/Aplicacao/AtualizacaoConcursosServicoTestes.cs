// <copyright file="AtualizacaoConcursosServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio.Concursos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço de atualização dos concursos.
/// </summary>
public class AtualizacaoConcursosServicoTestes
{
    /// <summary>
    /// Fonte falsa dos concursos individuais.
    /// </summary>
    private readonly IFonteConcursos fonte = Substitute.For<IFonteConcursos>();

    /// <summary>
    /// Repositório falso dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Números gravados pelo repositório falso, na ordem das gravações.
    /// </summary>
    private readonly List<int> gravados = [];

    /// <summary>
    /// Faz o repositório falso anotar cada concurso gravado e a fonte devolver o concurso pedido.
    /// </summary>
    public AtualizacaoConcursosServicoTestes()
    {
        this.repositorio.AdicionarAsync(Arg.Any<IReadOnlyCollection<Concurso>>(), Arg.Any<CancellationToken>())
            .Returns(chamada =>
            {
                this.gravados.AddRange(chamada.ArgAt<IReadOnlyCollection<Concurso>>(0).Select(concurso => concurso.Numero));
                return Task.CompletedTask;
            });
        this.fonte.ObterConcursoAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(chamada => ConcursoFabrica.Criar(numero: chamada.ArgAt<int>(0)));
    }

    /// <summary>
    /// Com a base vazia, a fonte não é consultada.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BaseVaziaNaoConsultaAFonte()
    {
        this.repositorio.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns((int?)null);

        ResultadoAtualizacao resultado = await this.CriarServico().AtualizarAsync(null, TestContext.Current.CancellationToken);

        Assert.True(resultado.BaseVazia);
        Assert.Equal(0, resultado.ConcursosGravados);
        await this.fonte.DidNotReceiveWithAnyArgs().ObterNumeroUltimoConcursoAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Com o último divulgado igual ao último gravado, nada é buscado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BaseJaAtualizadaNaoBuscaConcursos()
    {
        this.DefinirSituacao(ultimoGravado: 3808, ultimoDivulgado: 3808);

        ResultadoAtualizacao resultado = await this.CriarServico().AtualizarAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoAtualizacao(3808, 0, null), resultado);
        Assert.Equal(3808, resultado.UltimoNumeroNaBase);
        await this.fonte.DidNotReceiveWithAnyArgs().ObterConcursoAsync(default, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Os concursos novos são buscados em ordem, gravados um a um e informados no andamento.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BuscaEGravaOsNovosEmOrdem()
    {
        this.DefinirSituacao(ultimoGravado: 3808, ultimoDivulgado: 3811);
        var andamento = new AndamentoAnotado();

        ResultadoAtualizacao resultado = await this.CriarServico().AtualizarAsync(andamento, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoAtualizacao(3808, 3, null), resultado);
        Assert.Equal(3809, resultado.PrimeiroGravado);
        Assert.Equal(3811, resultado.UltimoNumeroNaBase);
        Assert.Equal([3809, 3810, 3811], this.gravados);
        await this.repositorio.Received(3).AdicionarAsync(
            Arg.Is<IReadOnlyCollection<Concurso>>(lote => lote.Count == 1),
            Arg.Any<CancellationToken>());
        Assert.Equal(
            [new AndamentoAtualizacao(3809, 1, 3), new AndamentoAtualizacao(3810, 2, 3), new AndamentoAtualizacao(3811, 3, 3)],
            andamento.Recebidos);
    }

    /// <summary>
    /// Uma falha da fonte no meio encerra a atualização, mantém os gravados e volta no resultado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task FalhaNoMeioPreservaOsGravados()
    {
        this.DefinirSituacao(ultimoGravado: 3808, ultimoDivulgado: 3812);
        var falha = new FonteResultadosException("A Caixa respondeu com o código 503 ao buscar o concurso 3811.");
        this.fonte.ObterConcursoAsync(3811, Arg.Any<CancellationToken>()).ThrowsAsync(falha);

        ResultadoAtualizacao resultado = await this.CriarServico().AtualizarAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoAtualizacao(3808, 2, falha), resultado);
        Assert.Equal(3810, resultado.UltimoNumeroNaBase);
        Assert.Equal([3809, 3810], this.gravados);
        await this.fonte.DidNotReceive().ObterConcursoAsync(3812, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Uma falha da fonte ao consultar o último concurso volta no resultado, sem gravar nada.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task FalhaAoConsultarOUltimoNaoGrava()
    {
        this.repositorio.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(3808);
        var falha = new FonteResultadosException("A Caixa respondeu com o código 503 ao buscar o último concurso.");
        this.fonte.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).ThrowsAsync(falha);

        ResultadoAtualizacao resultado = await this.CriarServico().AtualizarAsync(null, TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoAtualizacao(3808, 0, falha), resultado);
        Assert.Empty(this.gravados);
    }

    /// <summary>
    /// Uma falha que não é da fonte (por exemplo, do banco) é lançada ao chamador.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task FalhaDoRepositorioELancada()
    {
        this.DefinirSituacao(ultimoGravado: 3808, ultimoDivulgado: 3809);
        this.repositorio.AdicionarAsync(Arg.Any<IReadOnlyCollection<Concurso>>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Banco bloqueado."));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.CriarServico().AtualizarAsync(null, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Define o último concurso gravado e o último divulgado pela fonte.
    /// </summary>
    /// <param name="ultimoGravado">Último número na base.</param>
    /// <param name="ultimoDivulgado">Último número divulgado pela fonte.</param>
    private void DefinirSituacao(int ultimoGravado, int ultimoDivulgado)
    {
        this.repositorio.ObterUltimoNumeroAsync(Arg.Any<CancellationToken>()).Returns(ultimoGravado);
        this.fonte.ObterNumeroUltimoConcursoAsync(Arg.Any<CancellationToken>()).Returns(ultimoDivulgado);
    }

    /// <summary>
    /// Cria o serviço com a fonte e o repositório falsos.
    /// </summary>
    /// <returns>Serviço testado.</returns>
    private AtualizacaoConcursosServico CriarServico()
    {
        return new AtualizacaoConcursosServico(this.fonte, this.repositorio);
    }

    /// <summary>
    /// Anota cada andamento informado, sem depender de contexto de sincronização.
    /// </summary>
    private sealed class AndamentoAnotado : IProgress<AndamentoAtualizacao>
    {
        /// <summary>
        /// Andamentos recebidos, na ordem.
        /// </summary>
        public List<AndamentoAtualizacao> Recebidos { get; } = [];

        /// <inheritdoc/>
        public void Report(AndamentoAtualizacao value)
        {
            this.Recebidos.Add(value);
        }
    }
}
