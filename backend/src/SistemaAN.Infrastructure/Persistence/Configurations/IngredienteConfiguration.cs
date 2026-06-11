using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Catalog;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class IngredienteConfiguration : IEntityTypeConfiguration<Ingrediente>
{
    public void Configure(EntityTypeBuilder<Ingrediente> builder)
    {
        builder.ToTable("ingredientes", t =>
        {
            t.HasCheckConstraint("ck_ingredientes_coeficiente", "coeficiente_conversao > 0");
            t.HasCheckConstraint("ck_ingredientes_custo", "custo_atual_kg >= 0");
        });

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Nome).HasMaxLength(120).IsRequired();
        builder.Property(i => i.Ativo).IsRequired();

        builder.Property(i => i.TipoConversao)
            .HasConversion(v => v.ToDbValue(), v => TipoConversaoMap.FromDbValue(v))
            .HasMaxLength(15)
            .IsRequired();

        builder.Property(i => i.CoeficienteConversao).HasPrecision(8, 4).IsRequired();
        builder.Property(i => i.CustoAtualKg).HasPrecision(12, 2).IsRequired();

        builder.HasIndex(i => i.Nome).IsUnique();

        builder
            .HasOne(i => i.Categoria)
            .WithMany()
            .HasForeignKey(i => i.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
