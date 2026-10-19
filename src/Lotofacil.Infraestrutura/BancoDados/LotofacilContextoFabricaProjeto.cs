// <copyright file="LotofacilContextoFabricaProjeto.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Cria o contexto para o <c>dotnet ef</c> gerar migrations sem depender do projeto Web.
/// </summary>
internal sealed class LotofacilContextoFabricaProjeto : IDesignTimeDbContextFactory<LotofacilContexto>
{
    /// <summary>
    /// Cria o contexto apontando para um banco qualquer; as migrations só precisam do modelo.
    /// </summary>
    /// <param name="args">Argumentos da linha de comando do <c>dotnet ef</c>.</param>
    /// <returns>Contexto configurado para SQLite.</returns>
    public LotofacilContexto CreateDbContext(string[] args)
    {
        DbContextOptions<LotofacilContexto> opcoes = new DbContextOptionsBuilder<LotofacilContexto>()
            .UseSqlite("Data Source=lotofacil.db")
            .Options;

        return new LotofacilContexto(opcoes);
    }
}
