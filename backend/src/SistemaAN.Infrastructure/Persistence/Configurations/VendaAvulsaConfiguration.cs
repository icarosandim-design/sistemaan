using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Pets;
using SistemaAN.Domain.Produtos;
using SistemaAN.Domain.Vendas;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class VendaAvulsaConfiguration : IEntityTypeConfiguration<VendaAvulsa>
{
    public void Configure(EntityTypeBuilder<VendaAvulsa> builder)
    {
        builder.ToTable("vendas_avulsas");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.ClienteId).IsRequired();
        builder.Property(v => v.PetId);
        builder.Property(v => v.EntregaId);
        builder.Property(v => v.DataVenda).IsRequired();
        builder.Property(v => v.ValorTotal).HasPrecision(12, 2).IsRequired();
        builder.Property(v => v.FormaPagamento).HasMaxLength(30);
        builder.Property(v => v.Status).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(v => v.Observacoes).HasMaxLength(1000);

        builder.HasIndex(v => v.ClienteId).HasDatabaseName("ix_vendas_avulsas_cliente");
        builder.HasIndex(v => v.DataVenda).HasDatabaseName("ix_vendas_avulsas_data");
        builder.HasIndex(v => v.EntregaId).HasDatabaseName("ix_vendas_avulsas_entrega");
        builder.HasIndex(v => v.Status).HasDatabaseName("ix_vendas_avulsas_status");

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_vendas_avulsas_cliente");

        builder.HasOne<Pet>()
            .WithMany()
            .HasForeignKey(v => v.PetId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_vendas_avulsas_pet");

        builder.HasMany(v => v.Itens)
            .WithOne()
            .HasForeignKey(i => i.VendaAvulsaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_vendas_avulsas_itens_venda");

        builder.Metadata.FindNavigation(nameof(VendaAvulsa.Itens))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class VendaAvulsaItemConfiguration : IEntityTypeConfiguration<VendaAvulsaItem>
{
    public void Configure(EntityTypeBuilder<VendaAvulsaItem> builder)
    {
        builder.ToTable("vendas_avulsas_itens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.VendaAvulsaId).IsRequired();
        builder.Property(i => i.ProdutoId).IsRequired();
        builder.Property(i => i.Quantidade).IsRequired();
        builder.Property(i => i.PrecoUnitario).HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.ValorTotal).HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.Observacao).HasMaxLength(500);

        builder.HasIndex(i => i.VendaAvulsaId).HasDatabaseName("ix_vendas_avulsas_itens_venda");
        builder.HasIndex(i => i.ProdutoId).HasDatabaseName("ix_vendas_avulsas_itens_produto");

        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_vendas_avulsas_itens_produto");
    }
}
