using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Estoque;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class NotaCompraConfiguration : IEntityTypeConfiguration<NotaCompra>
{
    public void Configure(EntityTypeBuilder<NotaCompra> builder)
    {
        builder.ToTable("notas_compra");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.FornecedorId);
        builder.Property(n => n.DataCompra).IsRequired();
        builder.Property(n => n.DataEntrada).IsRequired();

        builder.Property(n => n.NumeroNotaFiscal).HasMaxLength(40);
        builder.Property(n => n.SerieNotaFiscal).HasMaxLength(10);
        builder.Property(n => n.ChaveAcessoNotaFiscal).HasMaxLength(60);
        builder.Property(n => n.DataEmissaoNotaFiscal);

        builder.Property(n => n.DataVencimentoPagamento);
        builder.Property(n => n.FormaPagamento).HasMaxLength(30);
        builder.Property(n => n.CondicaoPagamento).HasMaxLength(60);
        builder.Property(n => n.LinhaDigitavelBoleto).HasMaxLength(80);
        builder.Property(n => n.CodigoBarrasBoleto).HasMaxLength(60);
        builder.Property(n => n.BancoEmissorBoleto).HasMaxLength(60);
        builder.Property(n => n.NumeroDocumento).HasMaxLength(40);

        builder.Property(n => n.ValorProdutos).HasPrecision(18, 2).IsRequired();
        builder.Property(n => n.Frete).HasPrecision(18, 2).IsRequired();
        builder.Property(n => n.Desconto).HasPrecision(18, 2).IsRequired();
        builder.Property(n => n.Acrescimo).HasPrecision(18, 2).IsRequired();
        builder.Property(n => n.ValorTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(n => n.Observacoes).HasMaxLength(1000);

        builder.HasIndex(n => n.FornecedorId).HasDatabaseName("ix_notas_compra_fornecedor");
        builder.HasIndex(n => n.DataCompra).HasDatabaseName("ix_notas_compra_data");
        builder.HasIndex(n => n.DataVencimentoPagamento).HasDatabaseName("ix_notas_compra_vencimento");

        builder.HasOne<Fornecedor>()
            .WithMany()
            .HasForeignKey(n => n.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_notas_compra_fornecedor");
    }
}
