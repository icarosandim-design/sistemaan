using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Pets;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class DoencaConfiguration : IEntityTypeConfiguration<Doenca>
{
    public void Configure(EntityTypeBuilder<Doenca> builder)
    {
        builder.ToTable("doencas");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Nome).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Ordem).IsRequired();
        builder.Property(d => d.Ativo).IsRequired();

        builder.HasIndex(d => d.Nome).IsUnique();
    }
}
