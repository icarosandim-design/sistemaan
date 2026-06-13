using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Producao;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class OrdemProducaoConfiguration : IEntityTypeConfiguration<OrdemProducao>
{
    public void Configure(EntityTypeBuilder<OrdemProducao> builder)
    {
        builder.ToTable("ordens_producao");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Data).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(o => o.Observacoes).HasMaxLength(1000);
        builder.Property(o => o.FinalizadaEm);
        builder.Property(o => o.FinalizadaPor).HasMaxLength(160);
        builder.Property(o => o.TudoProduzido);
        builder.Property(o => o.ObservacoesFinalizacao).HasMaxLength(1000);

        builder.HasIndex(o => o.Data).IsUnique().HasDatabaseName("ix_ordens_producao_data");

        builder
            .HasMany(o => o.Fichas)
            .WithOne()
            .HasForeignKey(f => f.OrdemProducaoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_fichas_producao_ordem");

        builder
            .HasMany(o => o.Consumos)
            .WithOne()
            .HasForeignKey(c => c.OrdemProducaoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_consumo_producao_ordem");

        builder.Metadata.FindNavigation(nameof(OrdemProducao.Fichas))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(OrdemProducao.Consumos))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class FichaProducaoConfiguration : IEntityTypeConfiguration<FichaProducao>
{
    public void Configure(EntityTypeBuilder<FichaProducao> builder)
    {
        builder.ToTable("fichas_producao");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.OrdemProducaoId).IsRequired();
        builder.Property(f => f.Tipo).HasConversion<string>().HasMaxLength(15).IsRequired();

        // Referências externas por id (sem FK rígida — padrão snapshot/id).
        builder.Property(f => f.EntregaId);
        builder.Property(f => f.EntregaPetId);
        builder.Property(f => f.EntregaItemId);
        builder.Property(f => f.PetId);
        builder.Property(f => f.ClienteId);
        builder.Property(f => f.ReceitaId);
        builder.Property(f => f.TamanhoPacoteId);
        builder.Property(f => f.ItemEstoqueId);

        builder.Property(f => f.ClienteNome).HasMaxLength(160);
        builder.Property(f => f.PetNome).HasMaxLength(80);
        builder.Property(f => f.ReceitaCodigo).HasMaxLength(30).IsRequired();
        builder.Property(f => f.ReceitaNome).HasMaxLength(120).IsRequired();
        builder.Property(f => f.DataEntrega);

        builder.Property(f => f.QuantidadePacotes).IsRequired();
        builder.Property(f => f.PesoPacoteGramas).IsRequired();
        builder.Property(f => f.QuantidadeTotalGramas).IsRequired();

        builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(f => f.MotivoNaoFeita).HasMaxLength(255);
        builder.Property(f => f.QuantidadePacotesReal);
        builder.Property(f => f.PesoEnvasadoGramas);
        builder.Property(f => f.ConcluidaEm);
        builder.Property(f => f.ConcluidaPor).HasMaxLength(160);
        builder.Property(f => f.Observacoes).HasMaxLength(1000);

        builder.HasIndex(f => f.OrdemProducaoId).HasDatabaseName("ix_fichas_producao_ordem");
        builder.HasIndex(f => f.EntregaItemId).HasDatabaseName("ix_fichas_producao_entrega_item");

        builder
            .HasMany(f => f.Ingredientes)
            .WithOne()
            .HasForeignKey(i => i.FichaProducaoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_ficha_ingr_ficha");

        builder.Metadata.FindNavigation(nameof(FichaProducao.Ingredientes))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class FichaProducaoIngredienteConfiguration : IEntityTypeConfiguration<FichaProducaoIngrediente>
{
    public void Configure(EntityTypeBuilder<FichaProducaoIngrediente> builder)
    {
        builder.ToTable("ficha_producao_ingredientes");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.FichaProducaoId).IsRequired();
        builder.Property(i => i.IngredienteId).IsRequired();
        builder.Property(i => i.IngredienteNome).HasMaxLength(120).IsRequired();
        builder.Property(i => i.Categoria).HasMaxLength(60).IsRequired();
        builder.Property(i => i.GramasCozidas).IsRequired();
        builder.Property(i => i.Coeficiente).HasPrecision(8, 4).IsRequired();

        builder.HasIndex(i => i.FichaProducaoId).HasDatabaseName("ix_ficha_ingr_ficha");
    }
}

public sealed class ConsumoIngredienteProducaoConfiguration : IEntityTypeConfiguration<ConsumoIngredienteProducao>
{
    public void Configure(EntityTypeBuilder<ConsumoIngredienteProducao> builder)
    {
        builder.ToTable("consumo_ingrediente_producao");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.OrdemProducaoId).IsRequired();
        builder.Property(c => c.IngredienteId).IsRequired();
        builder.Property(c => c.IngredienteNome).HasMaxLength(120).IsRequired();
        builder.Property(c => c.ItemEstoqueId);
        builder.Property(c => c.ItemEstoqueNome).HasMaxLength(160);
        builder.Property(c => c.Coeficiente).HasPrecision(8, 4).IsRequired();
        builder.Property(c => c.UnidadeEstoque).HasMaxLength(15);
        builder.Property(c => c.PlanejadoCozidoGramas).IsRequired();
        builder.Property(c => c.PlanejadoCruGramas).IsRequired();
        builder.Property(c => c.RealCruGramas).HasPrecision(18, 3);
        builder.Property(c => c.RealCozidoGramas).HasPrecision(18, 3);
        builder.Property(c => c.BaixaRealizada).IsRequired();
        builder.Property(c => c.Observacao).HasMaxLength(1000);

        builder.HasIndex(c => c.OrdemProducaoId).HasDatabaseName("ix_consumo_producao_ordem");
    }
}
