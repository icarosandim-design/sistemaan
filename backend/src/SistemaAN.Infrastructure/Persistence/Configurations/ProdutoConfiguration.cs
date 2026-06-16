using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Pacotes;
using SistemaAN.Domain.Produtos;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produtos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nome).HasMaxLength(160).IsRequired();
        builder.Property(p => p.Codigo).HasMaxLength(40);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.ReceitaCasaId);
        builder.Property(p => p.TamanhoPacoteId);
        builder.Property(p => p.UnidadeMedida).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(p => p.PesoGramas);
        builder.Property(p => p.PrecoVendaAvulsaPF).HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.PrecoVendaPJ).HasPrecision(12, 2).IsRequired();
        builder.Property(p => p.ControlaEstoque).IsRequired();
        builder.Property(p => p.ProduzidoInternamente).IsRequired();
        builder.Property(p => p.Ativo).IsRequired();
        builder.Property(p => p.Observacoes).HasMaxLength(1000);

        // Unicidade de produto derivado de Receita+Tamanho (quando aplicável).
        builder.HasIndex(p => new { p.ReceitaCasaId, p.TamanhoPacoteId })
            .IsUnique()
            .HasFilter("receita_casa_id IS NOT NULL AND tamanho_pacote_id IS NOT NULL")
            .HasDatabaseName("ix_produtos_receita_tamanho");
        builder.HasIndex(p => p.Codigo)
            .IsUnique()
            .HasFilter("codigo IS NOT NULL")
            .HasDatabaseName("ix_produtos_codigo");
        builder.HasIndex(p => p.Nome).HasDatabaseName("ix_produtos_nome");
        builder.HasIndex(p => p.Tipo).HasDatabaseName("ix_produtos_tipo");
        builder.HasIndex(p => p.ReceitaCasaId).HasDatabaseName("ix_produtos_receita");

        builder.HasOne<Receita>()
            .WithMany()
            .HasForeignKey(p => p.ReceitaCasaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_produtos_receita");

        builder.HasOne<TamanhoPacote>()
            .WithMany()
            .HasForeignKey(p => p.TamanhoPacoteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_produtos_tamanho");
    }
}
