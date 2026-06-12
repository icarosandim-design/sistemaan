using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Pacotes;
using SistemaAN.Domain.Planos;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class PlanoItemPacoteConfiguration : IEntityTypeConfiguration<PlanoItemPacote>
{
    public void Configure(EntityTypeBuilder<PlanoItemPacote> builder)
    {
        builder.ToTable("plano_item_pacotes", t =>
            t.HasCheckConstraint("ck_plano_item_pacotes_qtd", "quantidade > 0"));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PlanoItemReceitaId).IsRequired();
        builder.Property(p => p.TamanhoPacoteId).IsRequired();
        builder.Property(p => p.Quantidade).IsRequired();

        builder.HasIndex(p => p.PlanoItemReceitaId).HasDatabaseName("ix_plano_item_pacotes_item");
        builder.HasIndex(p => p.TamanhoPacoteId).HasDatabaseName("ix_plano_item_pacotes_tamanho");

        builder
            .HasOne<TamanhoPacote>()
            .WithMany()
            .HasForeignKey(p => p.TamanhoPacoteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_plano_item_pacotes_tamanho");
    }
}
