using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class PapelConfiguration : IEntityTypeConfiguration<Papel>
{
    public void Configure(EntityTypeBuilder<Papel> builder)
    {
        builder.ToTable("papeis");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).HasMaxLength(40).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(160);

        builder.HasIndex(p => p.Nome).IsUnique();
    }
}
