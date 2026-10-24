// <copyright file="TeimosinhaRepositorioTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;
using Lotofacil.Infraestrutura.BancoDados;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Lotofacil.Testes.Integracao.BancoDados;

/// <summary>
/// Testes do repositório das Teimosinhas sobre SQLite em memória, com a estrutura criada pelas migrations.
/// </summary>
public sealed class TeimosinhaRepositorioTestes : IAsyncLifetime
{
    /// <summary>
    /// Migration anterior à Teimosinha, usada para simular uma base já existente.
    /// </summary>
    private const string MigrationAnterior = "20261024204005_CriacaoConfiguracaoApostas";

    /// <summary>
    /// Dezenas 01 a 15.
    /// </summary>
    private static readonly int[] De01a15 = [.. Enumerable.Range(1, 15)];

    /// <summary>
    /// Dezenas 11 a 25.
    /// </summary>
    private static readonly int[] De11a25 = [.. Enumerable.Range(11, 15)];

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
    private readonly TeimosinhaRepositorio repositorio;

    /// <summary>
    /// Monta o repositório sobre a conexão em memória.
    /// </summary>
    public TeimosinhaRepositorioTestes()
    {
        this.provedor = new ServiceCollection()
            .AddDbContextFactory<LotofacilContexto>(opcoes => opcoes.UseSqlite(this.conexao))
            .BuildServiceProvider();
        this.repositorio = new TeimosinhaRepositorio(this.Fabrica);
    }

    /// <summary>
    /// Fábrica de contextos do provedor.
    /// </summary>
    private IDbContextFactory<LotofacilContexto> Fabrica =>
        this.provedor.GetRequiredService<IDbContextFactory<LotofacilContexto>>();

    /// <summary>
    /// Abre a conexão; cada teste escolhe até qual migration aplicar.
    /// </summary>
    /// <returns>Tarefa da preparação.</returns>
    public async ValueTask InitializeAsync()
    {
        await this.conexao.OpenAsync(TestContext.Current.CancellationToken);
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
    /// A Teimosinha volta com os jogos, e um jogo acrescentado depois é inserido na atualização.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SalvaELeComOsJogos()
    {
        await this.MigrarAsync();
        var teimosinha = new Teimosinha("Minhas fixas", 3813);
        teimosinha.Acrescentar(De11a25.Reverse(), 3813);
        await this.repositorio.SalvarAsync(teimosinha, TestContext.Current.CancellationToken);

        Teimosinha? lida = await this.repositorio.ObterAsync(teimosinha.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(lida);
        lida.Acrescentar(De01a15, 3815);
        lida.RegistrarCriacao(3814);
        await this.repositorio.SalvarAsync(lida, TestContext.Current.CancellationToken);

        Teimosinha? relida = await this.repositorio.ObterAsync(teimosinha.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(relida);
        Assert.Equal("Minhas fixas", relida.Nome);
        Assert.Equal(3814, relida.UltimoConcursoCriado);
        Assert.Equal(
            [(3813, De11a25), (3815, De01a15)],
            relida.Jogos.Select(jogo => (jogo.ConcursoInicial, jogo.Dezenas.ToArray())));
    }

    /// <summary>
    /// Só as ativas são listadas, em ordem de nome sem diferenciar maiúsculas; os nomes incluem as removidas.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ListaAtivasENomes()
    {
        await this.MigrarAsync();
        var zebra = new Teimosinha("zebra", 3813);
        var alfa = new Teimosinha("Alfa", 3813);
        var removida = new Teimosinha("Antiga", 3800);
        removida.Remover();
        foreach (Teimosinha teimosinha in new[] { zebra, alfa, removida })
        {
            teimosinha.Acrescentar(De01a15, 3813);
            await this.repositorio.SalvarAsync(teimosinha, TestContext.Current.CancellationToken);
        }

        IReadOnlyList<Teimosinha> ativas = await this.repositorio.ListarAtivasAsync(TestContext.Current.CancellationToken);
        IReadOnlyDictionary<int, string> nomes = await this.repositorio.ListarNomesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["Alfa", "zebra"], ativas.Select(teimosinha => teimosinha.Nome));
        Assert.Single(ativas[0].Jogos);
        Assert.Equal("Antiga", nomes[removida.Id]);
        Assert.Equal(3, nomes.Count);
    }

    /// <summary>
    /// O jogo comum guarda a Teimosinha que o criou.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task JogoGuardaATeimosinha()
    {
        await this.MigrarAsync();
        var teimosinha = new Teimosinha("Minhas fixas", 3813);
        teimosinha.Acrescentar(De01a15, 3813);
        await this.repositorio.SalvarAsync(teimosinha, TestContext.Current.CancellationToken);
        var jogos = new JogoRepositorio(this.Fabrica);

        await jogos.AdicionarAsync(new Jogo(3813, De01a15, DateTime.Now, teimosinha.Id), TestContext.Current.CancellationToken);
        await jogos.AdicionarAsync(new Jogo(3813, De11a25, DateTime.Now), TestContext.Current.CancellationToken);

        IReadOnlyList<Jogo> lidos = await jogos.ListarAsync(TestContext.Current.CancellationToken);
        Assert.Equal([teimosinha.Id, null], lidos.Select(jogo => jogo.TeimosinhaId));
    }

    /// <summary>
    /// Numa base criada antes da Teimosinha, a migration mantém concursos e jogos, que ficam sem Teimosinha.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task MigrationPreservaBaseExistente()
    {
        await this.MigrarAsync(MigrationAnterior);
        var concursos = new ConcursoRepositorio(this.Fabrica);
        Rateio[] rateios = [new(15, 1, 10m), new(14, 1, 10m), new(13, 1, 35m), new(12, 1, 14m), new(11, 1, 7m)];
        await concursos.AdicionarAsync(
            [new Concurso(3812, new DateOnly(2026, 10, 23), De11a25, null, rateios, 0m, 0m, 0m, 0m)],
            TestContext.Current.CancellationToken);
        await using (LotofacilContexto contexto = await this.Fabrica.CreateDbContextAsync(TestContext.Current.CancellationToken))
        {
            // Antes da migration a tabela Jogo não tem a coluna TeimosinhaId, então o jogo é gravado por SQL.
            await contexto.Database.ExecuteSqlRawAsync(
                "INSERT INTO Jogo (NumeroConcurso, Dezenas, CadastradoEm) VALUES (3813, '[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15]', '2026-10-24 10:00:00')",
                TestContext.Current.CancellationToken);
        }

        await this.MigrarAsync();

        IReadOnlyList<Jogo> jogos = await new JogoRepositorio(this.Fabrica).ListarAsync(TestContext.Current.CancellationToken);
        Assert.Equal(De01a15, Assert.Single(jogos).Dezenas);
        Assert.Null(jogos[0].TeimosinhaId);
        Assert.NotNull(await concursos.ObterAsync(3812, TestContext.Current.CancellationToken));
        Assert.Empty(await this.repositorio.ListarAtivasAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Aplica as migrations até a informada, ou todas.
    /// </summary>
    /// <param name="migration">Última migration a aplicar; <c>null</c> aplica todas.</param>
    /// <returns>Tarefa da migração.</returns>
    private async Task MigrarAsync(string? migration = null)
    {
        await using LotofacilContexto contexto = await this.Fabrica.CreateDbContextAsync(TestContext.Current.CancellationToken);

        await contexto.GetService<IMigrator>().MigrateAsync(migration, TestContext.Current.CancellationToken);
    }
}
