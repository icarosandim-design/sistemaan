using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Pets;
using SistemaAN.Domain.Planos;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class PlanoAlimentarConfiguration : IEntityTypeConfiguration<PlanoAlimentar>
{
    public void Configure(EntityTypeBuilder<PlanoAlimentar> builder)
    {
        builder.ToTable("planos_alimentares", t =>
            t.HasCheckConstraint(
                "ck_planos_alimentares_gramas",
                "(gramas_dia_sugeridas IS NULL OR gramas_dia_sugeridas >= 0) AND "
                + "(gramas_dia_ajustadas IS NULL OR gramas_dia_ajustadas >= 0)"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PetId).IsRequired();
        builder.Property(p => p.FrequenciaEntregaId).IsRequired();
        builder.Property(p => p.PrimeiraEntrega).IsRequired();
        builder.Property(p => p.GramasDiaSugeridas);
        builder.Property(p => p.GramasDiaAjustadas);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(p => p.Ativo).IsRequired();

        // 1 plano vigente por pet.
        builder.HasIndex(p => p.PetId).IsUnique().HasDatabaseName("ix_planos_alimentares_pet");
        builder.HasIndex(p => p.FrequenciaEntregaId).HasDatabaseName("ix_planos_alimentares_frequencia");

        builder
            .HasOne<Pet>()
            .WithMany()
            .HasForeignKey(p => p.PetId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_planos_alimentares_pet");

        builder
            .HasOne<FrequenciaEntrega>()
            .WithMany()
            .HasForeignKey(p => p.FrequenciaEntregaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_planos_alimentares_frequencia");

        builder
            .HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.PlanoAlimentarId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_plano_itens_receita_plano");

        builder.Metadata.FindNavigation(nameof(PlanoAlimentar.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
