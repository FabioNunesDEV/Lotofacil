// <copyright file="LotofacilContexto.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Lotofacil.Dominio.Configuracoes;
using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;
using Microsoft.EntityFrameworkCore;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Acesso à base local SQLite pelo EF Core.
/// </summary>
public class LotofacilContexto : DbContext
{
    /// <summary>
    /// Cria o contexto com as opções informadas.
    /// </summary>
    /// <param name="opcoes">Opções do EF Core, incluindo a conexão com o banco.</param>
    public LotofacilContexto(DbContextOptions<LotofacilContexto> opcoes)
        : base(opcoes)
    {
    }

    /// <summary>
    /// Concursos gravados.
    /// </summary>
    public DbSet<Concurso> Concursos => this.Set<Concurso>();

    /// <summary>
    /// Jogos do usuário.
    /// </summary>
    public DbSet<Jogo> Jogos => this.Set<Jogo>();

    /// <summary>
    /// Configuração das apostas, em linha única.
    /// </summary>
    public DbSet<ConfiguracaoApostas> ConfiguracoesApostas => this.Set<ConfiguracaoApostas>();

    /// <summary>
    /// Teimosinhas, ativas e removidas.
    /// </summary>
    public DbSet<Teimosinha> Teimosinhas => this.Set<Teimosinha>();

    /// <summary>
    /// Aplica os mapeamentos das entidades.
    /// </summary>
    /// <param name="modelBuilder">Construtor do modelo do EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LotofacilContexto).Assembly);
    }
}
