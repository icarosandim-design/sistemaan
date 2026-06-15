using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Rotas;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class RotaConfiguration : IEntityTypeConfiguration<Rota>
{
    public void Configure(EntityTypeBuilder<Rota> builder)
    {
        builder.ToTable("rotas");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Data).IsRequired();
        builder.Property(r => r.Nome).HasMaxLength(120).IsRequired();
        builder.Property(r => r.Periodo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Entregador).HasMaxLength(120);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(r => r.Observacoes).HasMaxLength(1000);

        builder.HasIndex(r => r.Data).HasDatabaseName("ix_rotas_data");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_rotas_status");

        builder
            .HasMany(r => r.Paradas)
            .WithOne()
            .HasForeignKey(p => p.RotaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_rota_paradas_rota");

        builder.Metadata.FindNavigation(nameof(Rota.Paradas))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class RotaParadaConfiguration : IEntityTypeConfiguration<RotaParada>
{
    public void Configure(EntityTypeBuilder<RotaParada> builder)
    {
        builder.ToTable("rota_paradas");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.RotaId).IsRequired();
        builder.Property(p => p.EntregaId).IsRequired();
        builder.Property(p => p.Ordem).IsRequired();

        builder.HasIndex(p => p.RotaId).HasDatabaseName("ix_rota_paradas_rota");
        builder.HasIndex(p => p.EntregaId).HasDatabaseName("ix_rota_paradas_entrega");
    }
}
