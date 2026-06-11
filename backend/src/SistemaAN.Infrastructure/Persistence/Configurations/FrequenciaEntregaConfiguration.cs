using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Entregas;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class FrequenciaEntregaConfiguration : IEntityTypeConfiguration<FrequenciaEntrega>
{
    public void Configure(EntityTypeBuilder<FrequenciaEntrega> builder)
    {
        builder.ToTable("frequencias_entrega", t =>
            t.HasCheckConstraint(
                "ck_frequencias_entrega_dias",
                "personalizada = true OR (dias_ciclo IS NOT NULL AND dias_ciclo > 0)"));

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Nome).HasMaxLength(60).IsRequired();
        builder.Property(f => f.DiasCiclo);
        builder.Property(f => f.Descricao).HasMaxLength(255);
        builder.Property(f => f.Personalizada).IsRequired();
        builder.Property(f => f.Ativo).IsRequired();

        builder.HasIndex(f => f.Nome).IsUnique();
    }
}
