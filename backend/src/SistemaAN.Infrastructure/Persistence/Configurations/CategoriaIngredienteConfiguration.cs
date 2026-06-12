using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Catalog;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class CategoriaIngredienteConfiguration : IEntityTypeConfiguration<CategoriaIngrediente>
{
    public void Configure(EntityTypeBuilder<CategoriaIngrediente> builder)
    {
        builder.ToTable("categorias_ingredientes");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).HasMaxLength(60).IsRequired();
        builder.Property(c => c.Descricao).HasMaxLength(255);
        builder.Property(c => c.Ordem).IsRequired();
        builder.Property(c => c.Ativo).IsRequired();

        builder.HasIndex(c => c.Nome).IsUnique();
    }
}
