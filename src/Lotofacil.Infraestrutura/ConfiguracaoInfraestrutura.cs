// <copyright file="ConfiguracaoInfraestrutura.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Infraestrutura.BancoDados;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lotofacil.Infraestrutura;

/// <summary>
/// Registra os serviços da infraestrutura e prepara a base local.
/// </summary>
public static class ConfiguracaoInfraestrutura
{
    /// <summary>
    /// Registra o acesso à base SQLite e os repositórios.
    /// </summary>
    /// <param name="servicos">Coleção de serviços da aplicação.</param>
    /// <param name="caminhoBanco">Caminho do arquivo SQLite.</param>
    /// <returns>A mesma coleção de serviços.</returns>
    public static IServiceCollection AddInfraestrutura(this IServiceCollection servicos, string caminhoBanco)
    {
        servicos.AddDbContextFactory<LotofacilContexto>(opcoes => opcoes.UseSqlite($"Data Source={caminhoBanco}"));
        servicos.AddSingleton<IConcursoRepositorio, ConcursoRepositorio>();

        return servicos;
    }

    /// <summary>
    /// Cria o arquivo da base, se ainda não existir, e aplica as migrations pendentes.
    /// </summary>
    /// <param name="provedor">Provedor de serviços já montado.</param>
    /// <param name="cancelamento">Sinal de cancelamento.</param>
    /// <returns>Tarefa da inicialização.</returns>
    public static async Task InicializarBancoDadosAsync(
        this IServiceProvider provedor,
        CancellationToken cancelamento = default)
    {
        IDbContextFactory<LotofacilContexto> fabrica = provedor.GetRequiredService<IDbContextFactory<LotofacilContexto>>();
        await using LotofacilContexto contexto = await fabrica.CreateDbContextAsync(cancelamento);

        await contexto.Database.MigrateAsync(cancelamento);
    }
}
