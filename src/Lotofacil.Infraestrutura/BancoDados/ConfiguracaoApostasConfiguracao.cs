// <copyright file="ConfiguracaoApostasConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Configuracoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Mapeia a configuração das apostas para a tabela <c>ConfiguracaoApostas</c>, de linha única.
/// </summary>
internal sealed class ConfiguracaoApostasConfiguracao : IEntityTypeConfiguration<ConfiguracaoApostas>
{
    /// <summary>
    /// Configura o mapeamento da configuração das apostas.
    /// </summary>
    /// <param name="builder">Construtor do mapeamento da entidade.</param>
    public void Configure(EntityTypeBuilder<ConfiguracaoApostas> builder)
    {
        builder.ToTable("ConfiguracaoApostas");
        builder.HasKey(configuracao => configuracao.Id);
        builder.Property(configuracao => configuracao.Id).ValueGeneratedNever();
    }
}
