using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Pets;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class RacaConfiguration : IEntityTypeConfiguration<Raca>
{
    public void Configure(EntityTypeBuilder<Raca> builder)
    {
        builder.ToTable("racas");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Nome).HasMaxLength(80).IsRequired();
        builder.Property(r => r.Ordem).IsRequired();
        builder.Property(r => r.Ativo).IsRequired();

        builder.HasIndex(r => r.Nome).IsUnique();
    }
}
