// <copyright file="ConcursoRepositorioTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura.BancoDados;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lotofacil.Testes.Integracao.BancoDados;

/// <summary>
/// Testes do repositório de concursos sobre SQLite em memória, com a estrutura criada pelas migrations.
/// </summary>
public sealed class ConcursoRepositorioTestes : IAsyncLifetime
{
    /// <summary>
    /// Conexão mantida aberta durante o teste; o banco em memória deixa de existir quando ela fecha.
    /// </summary>
    private readonly SqliteConnection conexao = new("Data Source=:memory:");

    /// <summary>
    /// Provedor com a fábrica de contextos ligada à conexão em memória.
    /// </summary>
    private readonly ServiceProvider provedor;

    /// <summary>
    /// Repositório testado.
    /// </summary>
    private readonly ConcursoRepositorio repositorio;

    /// <summary>
    /// Monta o repositório sobre a conexão em memória.
    /// </summary>
    public ConcursoRepositorioTestes()
    {
        this.provedor = new ServiceCollection()
            .AddDbContextFactory<LotofacilContexto>(opcoes => opcoes.UseSqlite(this.conexao))
            .BuildServiceProvider();
        this.repositorio = new ConcursoRepositorio(this.Fabrica);
    }

    /// <summary>
    /// Fábrica de contextos usada pelo repositório.
    /// </summary>
    private IDbContextFactory<LotofacilContexto> Fabrica =>
        this.provedor.GetRequiredService<IDbContextFactory<LotofacilContexto>>();

    /// <summary>
    /// Abre a conexão e cria a estrutura do banco pelas migrations.
    /// </summary>
    /// <returns>Tarefa da preparação.</returns>
    public async ValueTask InitializeAsync()
    {
        await this.conexao.OpenAsync(TestContext.Current.CancellationToken);
        await using LotofacilContexto contexto = await this.Fabrica.CreateDbContextAsync(TestContext.Current.CancellationToken);
        await contexto.Database.MigrateAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Fecha a conexão, descartando o banco em memória.
    /// </summary>
    /// <returns>Tarefa da limpeza.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.provedor.DisposeAsync();
        await this.conexao.DisposeAsync();
    }

    /// <summary>
    /// O concurso recuperado tem exatamente os dados gravados, com centavos preservados.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task GravaERecuperaComOsMesmosDados()
    {
        int[] ordem = [13, 2, 24, 7, 20, 11, 1, 25, 18, 5, 22, 14, 10, 21, 4];
        Concurso gravado = CriarConcurso(3807, ordemSorteio: ordem);

        await this.repositorio.AdicionarAsync([gravado, CriarConcurso(3806)], TestContext.Current.CancellationToken);
        Concurso? lido = await this.repositorio.ObterAsync(3807, TestContext.Current.CancellationToken);

        Assert.NotNull(lido);
        Assert.Equal(gravado.Numero, lido.Numero);
        Assert.Equal(gravado.Data, lido.Data);
        Assert.Equal(gravado.Dezenas, lido.Dezenas);
        Assert.Equal(ordem, lido.OrdemSorteio);
        Assert.Equal(
            gravado.Rateios.Select(rateio => (rateio.Faixa, rateio.Ganhadores, rateio.ValorPremio)),
            lido.Rateios.Select(rateio => (rateio.Faixa, rateio.Ganhadores, rateio.ValorPremio)));
        Assert.Equal(24_873_112.57m, lido.Arrecadacao);
        Assert.Equal(0.01m, lido.Acumulado15);
        Assert.Equal(1_800_000.99m, lido.EstimativaProximo);
        Assert.Equal(120_500_300.25m, lido.AcumuladoIndependencia);
    }

    /// <summary>
    /// Concurso gravado sem ordem do sorteio continua sem ela ao ser lido.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ConcursoSemOrdemDoSorteioContinuaSemOrdem()
    {
        await this.repositorio.AdicionarAsync([CriarConcurso(1)], TestContext.Current.CancellationToken);

        Concurso? lido = await this.repositorio.ObterAsync(1, TestContext.Current.CancellationToken);

        Assert.NotNull(lido);
        Assert.Null(lido.OrdemSorteio);
    }

    /// <summary>
    /// Buscar um número que não está na base devolve <c>null</c>, sem erro.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ObterConcursoInexistenteDevolveNulo()
    {
        Concurso? lido = await this.repositorio.ObterAsync(9999, TestContext.Current.CancellationToken);

        Assert.Null(lido);
    }

    /// <summary>
    /// Um lote com concurso já gravado falha e nenhum concurso do lote é gravado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task LoteComConcursoJaGravadoNaoGravaNada()
    {
        await this.repositorio.AdicionarAsync([CriarConcurso(1)], TestContext.Current.CancellationToken);

        var erro = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.repositorio.AdicionarAsync([CriarConcurso(2), CriarConcurso(1)], TestContext.Current.CancellationToken));

        Assert.Contains("Concursos já gravados na base local: 1.", erro.Message);
        Assert.Equal(1, await this.repositorio.ContarAsync(TestContext.Current.CancellationToken));
        Assert.Null(await this.repositorio.ObterAsync(2, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Um lote com o mesmo número duas vezes falha e nada é gravado.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task LoteComNumeroRepetidoNaoGravaNada()
    {
        var erro = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.repositorio.AdicionarAsync(
                [CriarConcurso(1), CriarConcurso(2), CriarConcurso(1)],
                TestContext.Current.CancellationToken));

        Assert.Contains("O lote tem concursos repetidos: 1.", erro.Message);
        Assert.Equal(0, await this.repositorio.ContarAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// A listagem devolve a fatia pedida do mais recente ao mais antigo, e a contagem traz o total.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ListaDoMaisRecenteAoMaisAntigo()
    {
        List<Concurso> concursos = Enumerable.Range(1, 23).Select(numero => CriarConcurso(numero)).ToList();
        await this.repositorio.AdicionarAsync(concursos, TestContext.Current.CancellationToken);

        IReadOnlyList<Concurso> primeiraPagina = await this.repositorio.ListarAsync(0, 10, TestContext.Current.CancellationToken);
        IReadOnlyList<Concurso> segundaPagina = await this.repositorio.ListarAsync(10, 10, TestContext.Current.CancellationToken);
        IReadOnlyList<Concurso> ultimaPagina = await this.repositorio.ListarAsync(20, 10, TestContext.Current.CancellationToken);

        Assert.Equal(Enumerable.Range(14, 10).Reverse(), primeiraPagina.Select(concurso => concurso.Numero));
        Assert.Equal(Enumerable.Range(4, 10).Reverse(), segundaPagina.Select(concurso => concurso.Numero));
        Assert.Equal([3, 2, 1], ultimaPagina.Select(concurso => concurso.Numero));
        Assert.All(primeiraPagina, concurso => Assert.Equal(5, concurso.Rateios.Count));
        Assert.Equal(23, await this.repositorio.ContarAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Com a base vazia, a lista de números gravados vem vazia.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ListarNumerosComBaseVaziaDevolveVazio()
    {
        IReadOnlySet<int> numeros = await this.repositorio.ListarNumerosAsync(TestContext.Current.CancellationToken);

        Assert.Empty(numeros);
    }

    /// <summary>
    /// A lista de números traz todos os concursos gravados, inclusive com lacunas na sequência.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ListarNumerosDevolveTodosOsGravados()
    {
        await this.repositorio.AdicionarAsync(
            [CriarConcurso(1), CriarConcurso(2), CriarConcurso(5)],
            TestContext.Current.CancellationToken);

        IReadOnlySet<int> numeros = await this.repositorio.ListarNumerosAsync(TestContext.Current.CancellationToken);

        Assert.True(numeros.SetEquals([1, 2, 5]));
    }

    /// <summary>
    /// Com a base vazia, não há último número.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task UltimoNumeroComBaseVaziaENulo()
    {
        int? ultimo = await this.repositorio.ObterUltimoNumeroAsync(TestContext.Current.CancellationToken);

        Assert.Null(ultimo);
    }

    /// <summary>
    /// O último número é o maior gravado, mesmo com lacunas e gravação fora de ordem.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task UltimoNumeroEOMaiorGravado()
    {
        await this.repositorio.AdicionarAsync(
            [CriarConcurso(3808), CriarConcurso(1), CriarConcurso(102)],
            TestContext.Current.CancellationToken);

        int? ultimo = await this.repositorio.ObterUltimoNumeroAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3808, ultimo);
    }

    /// <summary>
    /// O modelo do EF Core e as migrations estão em dia: falha se o mapeamento mudar sem nova migration.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task MigrationsEstaoEmDiaComOModelo()
    {
        await using LotofacilContexto contexto = await this.Fabrica.CreateDbContextAsync(TestContext.Current.CancellationToken);

        Assert.False(contexto.Database.HasPendingModelChanges());
    }

    /// <summary>
    /// Cria um concurso válido com valores que têm centavos.
    /// </summary>
    /// <param name="numero">Número do concurso.</param>
    /// <param name="ordemSorteio">Ordem do sorteio, ou <c>null</c>.</param>
    /// <returns>Concurso criado.</returns>
    private static Concurso CriarConcurso(int numero, int[]? ordemSorteio = null)
    {
        Rateio[] rateios =
        [
            new Rateio(15, 1, 1_712_408.63m),
            new Rateio(14, 287, 1_502.34m),
            new Rateio(13, 9_874, 35m),
            new Rateio(12, 121_045, 14m),
            new Rateio(11, 653_211, 7m),
        ];

        return new Concurso(
            numero,
            new DateOnly(2026, 10, 17),
            [1, 2, 4, 5, 7, 10, 11, 13, 14, 18, 20, 21, 22, 24, 25],
            ordemSorteio,
            rateios,
            24_873_112.57m,
            0.01m,
            1_800_000.99m,
            120_500_300.25m);
    }
}
