// <copyright file="ConcursoConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) Fábio Nunes. Todos os direitos reservados.
// </copyright>

using Lotofacil.Dominio.Concursos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lotofacil.Infraestrutura.BancoDados;

/// <summary>
/// Mapeia o concurso para a tabela <c>Concurso</c> e o rateio para a tabela <c>ConcursoRateio</c>.
/// </summary>
internal sealed class ConcursoConfiguracao : IEntityTypeConfiguration<Concurso>
{
    /// <summary>
    /// Nome da coluna que liga o rateio ao concurso.
    /// </summary>
    private const string ColunaNumeroConcurso = "NumeroConcurso";

    /// <summary>
    /// Configura o mapeamento do concurso.
    /// </summary>
    /// <param name="builder">Construtor do mapeamento da entidade.</param>
    public void Configure(EntityTypeBuilder<Concurso> builder)
    {
        builder.ToTable("Concurso");
        builder.HasKey(concurso => concurso.Numero);
        builder.Property(concurso => concurso.Numero).ValueGeneratedNever();

        // Dezenas sempre lidas junto com o concurso: uma coluna JSON em vez de uma linha por dezena.
        builder.PrimitiveCollection(concurso => concurso.Dezenas).IsRequired();
        builder.PrimitiveCollection(concurso => concurso.OrdemSorteio);

        builder.OwnsMany(concurso => concurso.Rateios, rateio =>
        {
            rateio.ToTable("ConcursoRateio");
            rateio.WithOwner().HasForeignKey(ColunaNumeroConcurso);
            rateio.HasKey(ColunaNumeroConcurso, nameof(Rateio.Faixa));
            rateio.Property(item => item.Faixa).ValueGeneratedNever();
        });
        builder.Navigation(concurso => concurso.Rateios).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
