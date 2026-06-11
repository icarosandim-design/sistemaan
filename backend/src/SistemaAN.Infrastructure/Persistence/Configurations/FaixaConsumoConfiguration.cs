using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Consumo;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class FaixaConsumoConfiguration : IEntityTypeConfiguration<FaixaConsumo>
{
    public void Configure(EntityTypeBuilder<FaixaConsumo> builder)
    {
        builder.ToTable("faixas_consumo", t =>
        {
            t.HasCheckConstraint("ck_faixas_consumo_peso", "peso_inicial < peso_final");
            t.HasCheckConstraint("ck_faixas_consumo_gramas", "gramas_por_dia > 0");
        });

        builder.HasKey(f => f.Id);

        builder.Property(f => f.PesoInicial).HasPrecision(6, 2).IsRequired();
        builder.Property(f => f.PesoFinal).HasPrecision(6, 2).IsRequired();
        builder.Property(f => f.GramasPorDia).IsRequired();
        builder.Property(f => f.Ativo).IsRequired();

        builder.HasIndex(f => f.PesoInicial);
    }
}
