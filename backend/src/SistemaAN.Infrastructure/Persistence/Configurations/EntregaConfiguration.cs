using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class EntregaConfiguration : IEntityTypeConfiguration<Entrega>
{
    public void Configure(EntityTypeBuilder<Entrega> builder)
    {
        builder.ToTable("entregas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ClienteId).IsRequired();
        builder.Property(e => e.DataPrevista).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(25).IsRequired();

        builder.Property(e => e.ClienteNome).HasMaxLength(160).IsRequired();
        builder.Property(e => e.Telefone).HasMaxLength(40);
        builder.Property(e => e.Rua).HasMaxLength(160);
        builder.Property(e => e.Numero).HasMaxLength(20);
        builder.Property(e => e.Complemento).HasMaxLength(120);
        builder.Property(e => e.Cep).HasMaxLength(12);
        builder.Property(e => e.Bairro).HasMaxLength(120);
        builder.Property(e => e.Cidade).HasMaxLength(120);
        builder.Property(e => e.Estado).HasMaxLength(2);
        builder.Property(e => e.FrequenciaNome).HasMaxLength(60).IsRequired();
        builder.Property(e => e.DiasCiclo).IsRequired();
        builder.Property(e => e.PreferenciaHorario).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(e => e.ObservacoesInternas).HasMaxLength(1000);
        builder.Property(e => e.ObservacoesEntregador).HasMaxLength(1000);
        builder.Property(e => e.EntregadorId);

        builder.Property(e => e.MotivoNaoEntrega).HasMaxLength(255);
        builder.Property(e => e.MotivoReagendamento).HasMaxLength(255);
        builder.Property(e => e.MotivoCancelamento).HasMaxLength(255);

        // Pedido PJ de origem (id-only, sem FK rígida — padrão snapshot).
        builder.Property(e => e.PedidoId);
        builder.HasIndex(e => e.PedidoId).HasDatabaseName("ix_entregas_pedido");

        builder.Property(e => e.EstoqueBaixado).IsRequired();

        builder.HasIndex(e => new { e.ClienteId, e.DataPrevista }).HasDatabaseName("ix_entregas_cliente_data");
        builder.HasIndex(e => e.DataPrevista).HasDatabaseName("ix_entregas_data");
        builder.HasIndex(e => e.Status).HasDatabaseName("ix_entregas_status");

        builder
            .HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(e => e.ClienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_entregas_cliente");

        builder
            .HasMany(e => e.Pets)
            .WithOne()
            .HasForeignKey(p => p.EntregaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_entrega_pets_entrega");

        builder
            .HasMany(e => e.Historico)
            .WithOne()
            .HasForeignKey(h => h.EntregaId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_entrega_historico_entrega");

        builder.Metadata.FindNavigation(nameof(Entrega.Pets))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Entrega.Historico))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class EntregaPetConfiguration : IEntityTypeConfiguration<EntregaPet>
{
    public void Configure(EntityTypeBuilder<EntregaPet> builder)
    {
        builder.ToTable("entrega_pets");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.EntregaId).IsRequired();
        // PetId é id-only e NULLABLE: entregas de Pedido PJ usam um grupo "container" sem pet.
        builder.Property(p => p.PetId);
        builder.Property(p => p.PetNome).HasMaxLength(80).IsRequired();
        builder.Property(p => p.TipoAlimentacao).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(p => p.GramasDia);
        builder.Property(p => p.QuantidadeTotalGramas).IsRequired();

        builder.HasIndex(p => p.EntregaId).HasDatabaseName("ix_entrega_pets_entrega");
        builder.HasIndex(p => p.PetId).HasDatabaseName("ix_entrega_pets_pet");

        builder
            .HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.EntregaPetId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_entrega_itens_pet");

        builder.Metadata.FindNavigation(nameof(EntregaPet.Itens))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class EntregaItemConfiguration : IEntityTypeConfiguration<EntregaItem>
{
    public void Configure(EntityTypeBuilder<EntregaItem> builder)
    {
        builder.ToTable("entrega_itens");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.EntregaPetId).IsRequired();
        builder.Property(i => i.ReceitaId).IsRequired();
        builder.Property(i => i.ReceitaCodigo).HasMaxLength(30).IsRequired();
        builder.Property(i => i.ReceitaNome).HasMaxLength(120).IsRequired();
        builder.Property(i => i.Tipo).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(i => i.QuantidadeCicloGramas);
        builder.Property(i => i.TamanhoPacoteGramas);
        builder.Property(i => i.QuantidadePacotes);
        builder.Property(i => i.StatusPreparo).HasConversion<string>().HasMaxLength(25).IsRequired();
        builder.Property(i => i.PacotesProntos);
        builder.Property(i => i.PreparadoEm);
        builder.Property(i => i.PreparadoPor).HasMaxLength(160);

        builder.HasIndex(i => i.EntregaPetId).HasDatabaseName("ix_entrega_itens_pet");
        builder.HasIndex(i => i.ReceitaId).HasDatabaseName("ix_entrega_itens_receita");

        builder
            .HasOne<Receita>()
            .WithMany()
            .HasForeignKey(i => i.ReceitaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_entrega_itens_receita");

        builder
            .HasMany(i => i.Pacotes)
            .WithOne()
            .HasForeignKey(p => p.EntregaItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_entrega_pacotes_item");

        builder
            .HasMany(i => i.Ingredientes)
            .WithOne()
            .HasForeignKey(g => g.EntregaItemId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_entrega_ingr_item");

        builder.Metadata.FindNavigation(nameof(EntregaItem.Pacotes))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(EntregaItem.Ingredientes))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class EntregaItemPacoteConfiguration : IEntityTypeConfiguration<EntregaItemPacote>
{
    public void Configure(EntityTypeBuilder<EntregaItemPacote> builder)
    {
        builder.ToTable("entrega_item_pacotes");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.EntregaItemId).IsRequired();
        builder.Property(p => p.TamanhoLabel).HasMaxLength(30).IsRequired();
        builder.Property(p => p.PesoGramas).IsRequired();
        builder.Property(p => p.Quantidade).IsRequired();

        builder.HasIndex(p => p.EntregaItemId).HasDatabaseName("ix_entrega_pacotes_item");
    }
}

public sealed class EntregaItemIngredienteConfiguration : IEntityTypeConfiguration<EntregaItemIngrediente>
{
    public void Configure(EntityTypeBuilder<EntregaItemIngrediente> builder)
    {
        builder.ToTable("entrega_item_ingredientes");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.EntregaItemId).IsRequired();
        builder.Property(g => g.IngredienteId).IsRequired();
        builder.Property(g => g.IngredienteNome).HasMaxLength(120).IsRequired();
        builder.Property(g => g.Categoria).HasMaxLength(60).IsRequired();
        builder.Property(g => g.GramasCozidas).IsRequired();
        builder.Property(g => g.CustoKgCru).HasPrecision(12, 2).IsRequired();
        builder.Property(g => g.Coeficiente).HasPrecision(8, 4).IsRequired();

        builder.HasIndex(g => g.EntregaItemId).HasDatabaseName("ix_entrega_ingr_item");
        builder.HasIndex(g => g.IngredienteId).HasDatabaseName("ix_entrega_ingr_ingrediente");

        builder
            .HasOne<Ingrediente>()
            .WithMany()
            .HasForeignKey(g => g.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_entrega_ingr_ingrediente");
    }
}

public sealed class EntregaHistoricoConfiguration : IEntityTypeConfiguration<EntregaHistorico>
{
    public void Configure(EntityTypeBuilder<EntregaHistorico> builder)
    {
        builder.ToTable("entrega_historico");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.EntregaId).IsRequired();
        builder.Property(h => h.Quando).IsRequired();
        builder.Property(h => h.Usuario).HasMaxLength(160).IsRequired();
        builder.Property(h => h.Evento).HasMaxLength(400).IsRequired();
        builder.Property(h => h.StatusDe).HasConversion<string>().HasMaxLength(25);
        builder.Property(h => h.StatusPara).HasConversion<string>().HasMaxLength(25);

        builder.HasIndex(h => h.EntregaId).HasDatabaseName("ix_entrega_historico_entrega");
    }
}
