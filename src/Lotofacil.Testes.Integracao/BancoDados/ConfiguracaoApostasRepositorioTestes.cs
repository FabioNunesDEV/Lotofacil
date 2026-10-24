// <copyright file="ConfiguracaoApostasRepositorioTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Configuracoes;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Infraestrutura.BancoDados;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Lotofacil.Testes.Integracao.BancoDados;

/// <summary>
/// Testes do repositório da configuração das apostas sobre SQLite em memória, com a estrutura criada pelas
/// migrations.
/// </summary>
public sealed class ConfiguracaoApostasRepositorioTestes : IAsyncLifetime
{
    /// <summary>
    /// Migration anterior à tabela da configuração, usada para simular uma base já existente.
    /// </summary>
    private const string MigrationAnterior = "20261020200835_CriacaoJogos";

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
    private readonly ConfiguracaoApostasRepositorio repositorio;

    /// <summary>
    /// Monta o repositório sobre a conexão em memória.
    /// </summary>
    public ConfiguracaoApostasRepositorioTestes()
    {
        this.provedor = new ServiceCollection()
            .AddDbContextFactory<LotofacilContexto>(opcoes => opcoes.UseSqlite(this.conexao))
            .BuildServiceProvider();
        this.repositorio = new ConfiguracaoApostasRepositorio(this.Fabrica);
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
    /// Sem nada gravado, a leitura devolve <c>null</c>.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ObterSemLinhaDevolveNulo()
    {
        await this.MigrarAsync();

        Assert.Null(await this.repositorio.ObterAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// O primeiro salvar insere a linha; o seguinte substitui o preço sem criar outra linha.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SalvarInsereEDepoisAtualiza()
    {
        await this.MigrarAsync();

        await this.repositorio.SalvarAsync(new ConfiguracaoApostas(4.00m), TestContext.Current.CancellationToken);
        Assert.Equal(4.00m, (await this.repositorio.ObterAsync(TestContext.Current.CancellationToken))?.PrecoApostaMinima);

        await this.repositorio.SalvarAsync(new ConfiguracaoApostas(3.75m), TestContext.Current.CancellationToken);
        ConfiguracaoApostas? gravada = await this.repositorio.ObterAsync(TestContext.Current.CancellationToken);

        Assert.Equal(3.75m, gravada?.PrecoApostaMinima);
        Assert.Equal(ConfiguracaoApostas.IdUnico, gravada?.Id);
        await using LotofacilContexto contexto = await this.Fabrica.CreateDbContextAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, await contexto.ConfiguracoesApostas.CountAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Numa base criada antes da tabela da configuração, a migration nova mantém concursos e jogos e deixa a
    /// configuração vazia.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task MigrationPreservaBaseExistente()
    {
        await this.MigrarAsync(MigrationAnterior);
        var jogos = new JogoRepositorio(this.Fabrica);
        await using (LotofacilContexto contexto = await this.Fabrica.CreateDbContextAsync(TestContext.Current.CancellationToken))
        {
            // O modelo atual tem colunas que a base antiga ainda não tem, então o jogo é gravado por SQL.
            await contexto.Database.ExecuteSqlRawAsync(
                "INSERT INTO Jogo (NumeroConcurso, Dezenas, CadastradoEm) VALUES (3810, '[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15]', '2026-10-24 10:00:00')",
                TestContext.Current.CancellationToken);
        }

        var concursos = new ConcursoRepositorio(this.Fabrica);
        Rateio[] rateios = [new(15, 1, 10m), new(14, 1, 10m), new(13, 1, 35m), new(12, 1, 14m), new(11, 1, 7m)];
        int[] dezenas = [1, 2, 4, 5, 7, 10, 11, 13, 14, 18, 20, 21, 22, 24, 25];
        var concurso = new Concurso(3809, new DateOnly(2026, 10, 23), dezenas, null, rateios, 0m, 0m, 0m, 0m);
        await concursos.AdicionarAsync([concurso], TestContext.Current.CancellationToken);

        await this.MigrarAsync();

        Assert.Single(await jogos.ListarAsync(TestContext.Current.CancellationToken));
        Assert.NotNull(await concursos.ObterAsync(3809, TestContext.Current.CancellationToken));
        Assert.Null(await this.repositorio.ObterAsync(TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Aplica as migrations até a informada, ou todas.
    /// </summary>
    /// <param name="migration">Última migration a aplicar; <c>null</c> aplica todas.</param>
    /// <returns>Tarefa da migração.</returns>
    private async Task MigrarAsync(string? migration = null)
    {
        await using LotofacilContexto contexto = await this.Fabrica.CreateDbContextAsync(TestContext.Current.CancellationToken);
        IMigrator migrador = contexto.GetService<IMigrator>();

        await migrador.MigrateAsync(migration, TestContext.Current.CancellationToken);
    }
}
