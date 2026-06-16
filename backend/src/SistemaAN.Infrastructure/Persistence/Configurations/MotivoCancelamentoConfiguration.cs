using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class MotivoCancelamentoConfiguration : IEntityTypeConfiguration<MotivoCancelamento>
{
    public void Configure(EntityTypeBuilder<MotivoCancelamento> builder)
    {
        builder.ToTable("motivos_cancelamento");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nome).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Ordem).IsRequired();
        builder.Property(m => m.Ativo).IsRequired();
        builder.Property(m => m.Observacoes).HasMaxLength(500);

        builder.HasIndex(m => m.Nome).IsUnique();
    }
}
