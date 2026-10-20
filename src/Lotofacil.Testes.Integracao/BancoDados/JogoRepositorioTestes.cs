// <copyright file="JogoRepositorioTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Jogos;
using Lotofacil.Infraestrutura.BancoDados;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lotofacil.Testes.Integracao.BancoDados;

/// <summary>
/// Testes do repositório de jogos sobre SQLite em memória, com a estrutura criada pelas migrations.
/// </summary>
public sealed class JogoRepositorioTestes : IAsyncLifetime
{
    /// <summary>
    /// Dezenas 01 a 15.
    /// </summary>
    private static readonly int[] De01a15 = Enumerable.Range(1, 15).ToArray();

    /// <summary>
    /// Dezenas 11 a 25.
    /// </summary>
    private static readonly int[] De11a25 = Enumerable.Range(11, 15).ToArray();

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
    private readonly JogoRepositorio repositorio;

    /// <summary>
    /// Monta o repositório sobre a conexão em memória.
    /// </summary>
    public JogoRepositorioTestes()
    {
        this.provedor = new ServiceCollection()
            .AddDbContextFactory<LotofacilContexto>(opcoes => opcoes.UseSqlite(this.conexao))
            .BuildServiceProvider();
        this.repositorio = new JogoRepositorio(this.provedor.GetRequiredService<IDbContextFactory<LotofacilContexto>>());
    }

    /// <summary>
    /// Abre a conexão e cria a estrutura do banco pelas migrations.
    /// </summary>
    /// <returns>Tarefa da preparação.</returns>
    public async ValueTask InitializeAsync()
    {
        await this.conexao.OpenAsync(TestContext.Current.CancellationToken);
        IDbContextFactory<LotofacilContexto> fabrica = this.provedor.GetRequiredService<IDbContextFactory<LotofacilContexto>>();
        await using LotofacilContexto contexto = await fabrica.CreateDbContextAsync(TestContext.Current.CancellationToken);
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
    /// Os jogos voltam na ordem de cadastro, com os mesmos dados e um identificador.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task GravaEListaNaOrdemDeCadastro()
    {
        var cadastro = new DateTime(2026, 10, 20, 17, 30, 15);
        await this.repositorio.AdicionarAsync(new Jogo(3810, De11a25.Reverse(), cadastro), TestContext.Current.CancellationToken);
        await this.repositorio.AdicionarAsync(new Jogo(3809, De01a15, cadastro), TestContext.Current.CancellationToken);
        await this.repositorio.AdicionarAsync(new Jogo(3810, De01a15, cadastro), TestContext.Current.CancellationToken);

        IReadOnlyList<Jogo> jogos = await this.repositorio.ListarAsync(TestContext.Current.CancellationToken);

        Assert.Equal([3810, 3809, 3810], jogos.Select(jogo => jogo.NumeroConcurso));
        Assert.Equal(De11a25, jogos[0].Dezenas);
        Assert.Equal(cadastro, jogos[0].CadastradoEm);
        Assert.All(jogos, jogo => Assert.True(jogo.Id > 0));
        Assert.Equal(3, jogos.Select(jogo => jogo.Id).Distinct().Count());
    }

    /// <summary>
    /// A verificação de existência compara o concurso e as dezenas, em qualquer ordem.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ExisteComparaConcursoEDezenas()
    {
        await this.repositorio.AdicionarAsync(new Jogo(3809, De01a15, DateTime.Now), TestContext.Current.CancellationToken);

        Assert.True(await this.repositorio.ExisteAsync(3809, De01a15.Reverse(), TestContext.Current.CancellationToken));
        Assert.False(await this.repositorio.ExisteAsync(3810, De01a15, TestContext.Current.CancellationToken));
        Assert.False(await this.repositorio.ExisteAsync(3809, De11a25, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Excluir remove só o jogo pedido; um identificador inexistente não causa erro.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ExcluiPeloIdentificador()
    {
        await this.repositorio.AdicionarAsync(new Jogo(3809, De01a15, DateTime.Now), TestContext.Current.CancellationToken);
        await this.repositorio.AdicionarAsync(new Jogo(3809, De11a25, DateTime.Now), TestContext.Current.CancellationToken);
        IReadOnlyList<Jogo> antes = await this.repositorio.ListarAsync(TestContext.Current.CancellationToken);

        await this.repositorio.ExcluirAsync(antes[0].Id, TestContext.Current.CancellationToken);
        await this.repositorio.ExcluirAsync(99_999, TestContext.Current.CancellationToken);

        IReadOnlyList<Jogo> depois = await this.repositorio.ListarAsync(TestContext.Current.CancellationToken);
        Assert.Equal([antes[1].Id], depois.Select(jogo => jogo.Id));
    }

    /// <summary>
    /// O banco recusa o mesmo jogo duas vezes no mesmo concurso, mesmo se a verificação do serviço falhar.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task IndiceUnicoRecusaJogoDuplicado()
    {
        await this.repositorio.AdicionarAsync(new Jogo(3809, De01a15, DateTime.Now), TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => this.repositorio.AdicionarAsync(new Jogo(3809, De01a15.Reverse(), DateTime.Now), TestContext.Current.CancellationToken));
    }
}
