// <copyright file="JogoConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Jogos;
using Lotofacil.Dominio.Teimosinhas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Mapeia o jogo do usuário para a tabela <c>Jogo</c>.
/// </summary>
internal sealed class JogoConfiguracao : IEntityTypeConfiguration<Jogo>
{
    /// <summary>
    /// Configura o mapeamento do jogo.
    /// </summary>
    /// <param name="builder">Construtor do mapeamento da entidade.</param>
    public void Configure(EntityTypeBuilder<Jogo> builder)
    {
        builder.ToTable("Jogo");
        builder.HasKey(jogo => jogo.Id);
        builder.PrimitiveCollection(jogo => jogo.Dezenas).IsRequired();

        // As dezenas são gravadas em ordem crescente, então o mesmo jogo gera sempre o mesmo texto.
        builder.HasIndex(jogo => new { jogo.NumeroConcurso, jogo.Dezenas }).IsUnique();

        // A Teimosinha nunca é apagada, só fica inativa; a restrição protege o nome nos jogos já criados.
        builder.HasOne<Teimosinha>()
            .WithMany()
            .HasForeignKey(jogo => jogo.TeimosinhaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
