using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class ItemReceitaConfiguration : IEntityTypeConfiguration<ItemReceita>
{
    public void Configure(EntityTypeBuilder<ItemReceita> builder)
    {
        builder.ToTable("itens_receita", t =>
            t.HasCheckConstraint("ck_itens_receita_gramas", "gramas > 0"));

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Gramas).IsRequired();

        builder
            .HasOne<Ingrediente>()
            .WithMany()
            .HasForeignKey(i => i.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.ReceitaId);
    }
}
