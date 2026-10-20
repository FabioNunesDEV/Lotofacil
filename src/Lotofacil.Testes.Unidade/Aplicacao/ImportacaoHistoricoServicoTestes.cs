// <copyright file="ImportacaoHistoricoServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Aplicacao.Concursos;
using Lotofacil.Dominio.Concursos;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Lotofacil.Testes.Unidade.Aplicacao;

/// <summary>
/// Testes do serviço de importação do histórico.
/// </summary>
public class ImportacaoHistoricoServicoTestes
{
    /// <summary>
    /// Fonte falsa do histórico.
    /// </summary>
    private readonly IFonteHistoricoConcursos fonte = Substitute.For<IFonteHistoricoConcursos>();

    /// <summary>
    /// Repositório falso dos concursos.
    /// </summary>
    private readonly IConcursoRepositorio repositorio = Substitute.For<IConcursoRepositorio>();

    /// <summary>
    /// Com a base vazia, todos os concursos do histórico são gravados num único lote.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BaseVaziaGravaTodos()
    {
        this.DefinirHistorico(1, 2, 3);
        this.DefinirGravados();

        ResultadoImportacao resultado = await this.CriarServico().ImportarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoImportacao(3, 3), resultado);
        await this.repositorio.Received(1).AdicionarAsync(
            Arg.Is<IReadOnlyCollection<Concurso>>(lote => lote.Select(concurso => concurso.Numero).SequenceEqual(new[] { 1, 2, 3 })),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Com parte dos concursos na base, somente os novos são gravados.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task BaseParcialGravaSomenteOsNovos()
    {
        this.DefinirHistorico(1, 2, 3, 4, 5);
        this.DefinirGravados(1, 2, 4);

        ResultadoImportacao resultado = await this.CriarServico().ImportarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoImportacao(5, 2), resultado);
        await this.repositorio.Received(1).AdicionarAsync(
            Arg.Is<IReadOnlyCollection<Concurso>>(lote => lote.Select(concurso => concurso.Numero).SequenceEqual(new[] { 3, 5 })),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Quando todos os concursos já estão na base, nada é gravado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task NadaNovoNaoGrava()
    {
        this.DefinirHistorico(1, 2);
        this.DefinirGravados(1, 2);

        ResultadoImportacao resultado = await this.CriarServico().ImportarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new ResultadoImportacao(2, 0), resultado);
        await this.repositorio.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Uma falha da fonte chega ao chamador e nada é gravado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task FalhaDaFonteNaoGrava()
    {
        this.fonte.ObterHistoricoAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new FonteResultadosException("A Caixa respondeu com o código 503 ao baixar o histórico."));

        var erro = await Assert.ThrowsAsync<FonteResultadosException>(
            () => this.CriarServico().ImportarAsync(TestContext.Current.CancellationToken));

        Assert.Equal("A Caixa respondeu com o código 503 ao baixar o histórico.", erro.Message);
        await this.repositorio.DidNotReceiveWithAnyArgs().AdicionarAsync(default!, TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Faz a fonte devolver concursos com os números informados.
    /// </summary>
    /// <param name="numeros">Números dos concursos do histórico.</param>
    private void DefinirHistorico(params int[] numeros)
    {
        List<Concurso> concursos = numeros.Select(numero => ConcursoFabrica.Criar(numero)).ToList();
        this.fonte.ObterHistoricoAsync(Arg.Any<CancellationToken>()).Returns(concursos);
    }

    /// <summary>
    /// Faz o repositório informar os números já gravados.
    /// </summary>
    /// <param name="numeros">Números dos concursos na base.</param>
    private void DefinirGravados(params int[] numeros)
    {
        this.repositorio.ListarNumerosAsync(Arg.Any<CancellationToken>()).Returns(numeros.ToHashSet());
    }

    /// <summary>
    /// Cria o serviço com a fonte e o repositório falsos.
    /// </summary>
    /// <returns>Serviço testado.</returns>
    private ImportacaoHistoricoServico CriarServico()
    {
        return new ImportacaoHistoricoServico(this.fonte, this.repositorio);
    }
}
