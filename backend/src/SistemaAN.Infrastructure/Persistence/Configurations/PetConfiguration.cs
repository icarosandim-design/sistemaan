using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Pets;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.ToTable("pets", t =>
        {
            t.HasCheckConstraint("ck_pets_peso", "peso_kg > 0");
            t.HasCheckConstraint("ck_pets_gramas_ajustadas", "gramas_dia_ajustadas IS NULL OR gramas_dia_ajustadas >= 0");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).HasMaxLength(80).IsRequired();
        builder.Property(p => p.Raca).HasMaxLength(80);
        builder.Property(p => p.PesoKg).HasPrecision(6, 2).IsRequired();
        builder.Property(p => p.DataNascimento);
        builder.Property(p => p.IdadeAprox).HasMaxLength(40);
        builder.Property(p => p.Sexo).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.Ativo).IsRequired();
        builder.Property(p => p.ObservacoesGerais).HasMaxLength(1000);
        builder.Property(p => p.ObservacoesAlimentares).HasMaxLength(1000);
        builder.Property(p => p.GramasDiaAjustadas);

        builder.HasIndex(p => p.ClienteId);

        builder
            .HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
