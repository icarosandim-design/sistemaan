using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Pets;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class ReceitaConfiguration : IEntityTypeConfiguration<Receita>
{
    public void Configure(EntityTypeBuilder<Receita> builder)
    {
        builder.ToTable("receitas");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Codigo).HasMaxLength(30).IsRequired();
        builder.Property(r => r.Nome).HasMaxLength(120).IsRequired();
        builder.Property(r => r.Observacoes).HasMaxLength(1000);
        builder.Property(r => r.Ativo).IsRequired();
        builder.Property(r => r.PetId);

        builder.Property(r => r.Tipo).HasConversion<string>().HasMaxLength(15).IsRequired();

        // Código único por tipo (na prática, Casa).
        builder.HasIndex(r => new { r.Tipo, r.Codigo }).IsUnique();

        // Dono da receita personalizada (PetId vira FK real; nulo para Casa).
        builder.HasIndex(r => r.PetId).HasDatabaseName("ix_receitas_pet");
        builder
            .HasOne<Pet>()
            .WithMany()
            .HasForeignKey(r => r.PetId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_receitas_pet");

        builder
            .HasMany(r => r.Itens)
            .WithOne()
            .HasForeignKey(i => i.ReceitaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Receita.Itens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
