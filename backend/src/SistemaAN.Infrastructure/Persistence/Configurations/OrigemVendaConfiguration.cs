using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class OrigemVendaConfiguration : IEntityTypeConfiguration<OrigemVenda>
{
    public void Configure(EntityTypeBuilder<OrigemVenda> builder)
    {
        builder.ToTable("origens_venda");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Nome).HasMaxLength(80).IsRequired();
        builder.Property(o => o.Ordem).IsRequired();
        builder.Property(o => o.Ativo).IsRequired();

        builder.HasIndex(o => o.Nome).IsUnique();
    }
}
