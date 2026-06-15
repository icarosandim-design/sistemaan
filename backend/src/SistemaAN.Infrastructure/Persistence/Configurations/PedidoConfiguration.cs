using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Pedidos;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("pedidos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClienteId).IsRequired();
        builder.Property(p => p.ClienteNome).HasMaxLength(180).IsRequired();
        builder.Property(p => p.DataPedido).IsRequired();
        builder.Property(p => p.DataEntrega).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(p => p.Observacoes).HasMaxLength(1000);
        builder.Property(p => p.ValorTotal).HasPrecision(12, 2);
        builder.Property(p => p.EntregaId); // id-only (vínculo com a Entrega)

        builder.HasIndex(p => p.ClienteId).HasDatabaseName("ix_pedidos_cliente");
        builder.HasIndex(p => p.DataEntrega).HasDatabaseName("ix_pedidos_data_entrega");
        builder.HasIndex(p => p.Status).HasDatabaseName("ix_pedidos_status");

        builder
            .HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_pedidos_cliente");

        builder
            .HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_pedido_itens_pedido");

        builder.Metadata.FindNavigation(nameof(Pedido.Itens))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PedidoItemConfiguration : IEntityTypeConfiguration<PedidoItem>
{
    public void Configure(EntityTypeBuilder<PedidoItem> builder)
    {
        builder.ToTable("pedido_itens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.PedidoId).IsRequired();
        builder.Property(i => i.ReceitaId).IsRequired();
        builder.Property(i => i.ReceitaCodigo).HasMaxLength(30).IsRequired();
        builder.Property(i => i.ReceitaNome).HasMaxLength(120).IsRequired();
        builder.Property(i => i.TamanhoPacoteId).IsRequired();
        builder.Property(i => i.TamanhoNome).HasMaxLength(40).IsRequired();
        builder.Property(i => i.PesoGramas).IsRequired();
        builder.Property(i => i.Quantidade).IsRequired();
        builder.Property(i => i.PrecoUnitario).HasPrecision(12, 2);
        builder.Property(i => i.Observacao).HasMaxLength(500);
        builder.Property(i => i.ItemEstoqueId);

        builder.HasIndex(i => i.PedidoId).HasDatabaseName("ix_pedido_itens_pedido");
    }
}
