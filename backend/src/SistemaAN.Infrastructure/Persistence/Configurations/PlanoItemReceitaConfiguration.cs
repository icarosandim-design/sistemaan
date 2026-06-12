using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Planos;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class PlanoItemReceitaConfiguration : IEntityTypeConfiguration<PlanoItemReceita>
{
    public void Configure(EntityTypeBuilder<PlanoItemReceita> builder)
    {
        builder.ToTable("plano_itens_receita", t =>
        {
            t.HasCheckConstraint(
                "ck_plano_itens_receita_ciclo",
                "quantidade_ciclo_gramas IS NULL OR quantidade_ciclo_gramas > 0");
            t.HasCheckConstraint(
                "ck_plano_itens_receita_pacotes",
                "quantidade_pacotes IS NULL OR quantidade_pacotes > 0");
        });

        builder.HasKey(i => i.Id);

        builder.Property(i => i.PlanoAlimentarId).IsRequired();
        builder.Property(i => i.ReceitaId).IsRequired();
        builder.Property(i => i.QuantidadeCicloGramas);
        builder.Property(i => i.QuantidadePacotes);

        builder.HasIndex(i => i.PlanoAlimentarId).HasDatabaseName("ix_plano_itens_receita_plano");
        builder.HasIndex(i => i.ReceitaId).HasDatabaseName("ix_plano_itens_receita_receita");

        builder
            .HasOne<Receita>()
            .WithMany()
            .HasForeignKey(i => i.ReceitaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_plano_itens_receita_receita");

        builder
            .HasMany(i => i.Pacotes)
            .WithOne()
            .HasForeignKey(p => p.PlanoItemReceitaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_plano_item_pacotes_item");

        builder.Metadata.FindNavigation(nameof(PlanoItemReceita.Pacotes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
