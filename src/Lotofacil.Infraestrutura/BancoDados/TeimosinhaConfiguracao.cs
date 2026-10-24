// <copyright file="TeimosinhaConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Teimosinhas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Mapeia a Teimosinha para a tabela <c>Teimosinha</c> e os jogos dela para a tabela <c>TeimosinhaJogo</c>.
/// </summary>
internal sealed class TeimosinhaConfiguracao : IEntityTypeConfiguration<Teimosinha>
{
    /// <summary>
    /// Configura o mapeamento da Teimosinha.
    /// </summary>
    /// <param name="builder">Construtor do mapeamento da entidade.</param>
    public void Configure(EntityTypeBuilder<Teimosinha> builder)
    {
        builder.ToTable("Teimosinha");
        builder.HasKey(teimosinha => teimosinha.Id);
        builder.Property(teimosinha => teimosinha.Nome).IsRequired().HasMaxLength(Teimosinha.TamanhoMaximoNome);

        builder.OwnsMany(teimosinha => teimosinha.Jogos, jogo =>
        {
            jogo.ToTable("TeimosinhaJogo");
            jogo.WithOwner().HasForeignKey("TeimosinhaId");
            jogo.HasKey(item => item.Id);
            jogo.PrimitiveCollection(item => item.Dezenas).IsRequired();
        });
        builder.Navigation(teimosinha => teimosinha.Jogos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
