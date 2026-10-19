// <copyright file="InicializacaoBancoDadosTestes.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Lotofacil.Testes.Integracao.BancoDados;

/// <summary>
/// Verifica a criação e a atualização do arquivo da base local na inicialização da aplicação.
/// </summary>
public sealed class InicializacaoBancoDadosTestes : IDisposable
{
    /// <summary>
    /// Pasta temporária do teste.
    /// </summary>
    private readonly string pasta = Path.Combine(Path.GetTempPath(), "lotofacil-testes", Guid.NewGuid().ToString("N"));

    /// <summary>
    /// Caminho do arquivo da base, ainda inexistente no início do teste.
    /// </summary>
    private string CaminhoBanco => Path.Combine(this.pasta, "lotofacil.db");

    /// <summary>
    /// Na primeira execução o arquivo é criado vazio; na seguinte, os concursos gravados continuam lá.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriaArquivoNaPrimeiraExecucaoEPreservaDadosNaSeguinte()
    {
        Directory.CreateDirectory(this.pasta);
        Assert.False(File.Exists(this.CaminhoBanco));

        await using (ServiceProvider primeiraExecucao = await this.IniciarAsync())
        {
            Assert.True(File.Exists(this.CaminhoBanco));
            IConcursoRepositorio repositorio = primeiraExecucao.GetRequiredService<IConcursoRepositorio>();
            Assert.Equal(0, await repositorio.ContarAsync(TestContext.Current.CancellationToken));

            await repositorio.AdicionarAsync([CriarConcurso()], TestContext.Current.CancellationToken);
        }

        await using (ServiceProvider segundaExecucao = await this.IniciarAsync())
        {
            IConcursoRepositorio repositorio = segundaExecucao.GetRequiredService<IConcursoRepositorio>();
            Assert.NotNull(await repositorio.ObterAsync(3807, TestContext.Current.CancellationToken));
        }
    }

    /// <summary>
    /// Remove a pasta temporária, liberando antes o arquivo mantido aberto pelo pool de conexões do SQLite.
    /// </summary>
    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(this.pasta))
        {
            Directory.Delete(this.pasta, recursive: true);
        }
    }

    /// <summary>
    /// Cria um concurso válido.
    /// </summary>
    /// <returns>Concurso 3807.</returns>
    private static Concurso CriarConcurso()
    {
        Rateio[] rateios = [new(15, 1, 10m), new(14, 1, 10m), new(13, 1, 35m), new(12, 1, 14m), new(11, 1, 7m)];

        return new Concurso(
            3807,
            new DateOnly(2026, 10, 17),
            [1, 2, 4, 5, 7, 10, 11, 13, 14, 18, 20, 21, 22, 24, 25],
            null,
            rateios,
            0m,
            0m,
            0m,
            0m);
    }

    /// <summary>
    /// Simula uma inicialização da aplicação: registra a infraestrutura e prepara a base.
    /// </summary>
    /// <returns>Provedor de serviços pronto.</returns>
    private async Task<ServiceProvider> IniciarAsync()
    {
        ServiceProvider provedor = new ServiceCollection()
            .AddInfraestrutura(this.CaminhoBanco)
            .BuildServiceProvider();
        await provedor.InicializarBancoDadosAsync(TestContext.Current.CancellationToken);

        return provedor;
    }
}
