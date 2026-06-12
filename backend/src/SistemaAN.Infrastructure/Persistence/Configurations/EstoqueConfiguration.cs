using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Pacotes;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("fornecedores");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Nome).HasMaxLength(160).IsRequired();
        builder.Property(f => f.NomeFantasia).HasMaxLength(160);
        builder.Property(f => f.Documento).HasMaxLength(20);
        builder.Property(f => f.Telefone).HasMaxLength(40);
        builder.Property(f => f.WhatsApp).HasMaxLength(40);
        builder.Property(f => f.Email).HasMaxLength(160);
        builder.Property(f => f.PessoaContato).HasMaxLength(120);
        builder.Property(f => f.Endereco).HasMaxLength(200);
        builder.Property(f => f.Cidade).HasMaxLength(120);
        builder.Property(f => f.Estado).HasMaxLength(2);
        builder.Property(f => f.Categoria).HasConversion<string>().HasMaxLength(25);
        builder.Property(f => f.Observacoes).HasMaxLength(1000);
        builder.Property(f => f.Ativo).IsRequired();
        builder.Property(f => f.PrazoPagamentoDias);
        builder.Property(f => f.FormaPagamentoPreferida).HasMaxLength(60);
        builder.Property(f => f.ChavePix).HasMaxLength(140);
        builder.Property(f => f.DadosBancarios).HasMaxLength(255);

        builder.HasIndex(f => f.Nome).HasDatabaseName("ix_fornecedores_nome");
    }
}

public sealed class ItemEstoqueConfiguration : IEntityTypeConfiguration<ItemEstoque>
{
    public void Configure(EntityTypeBuilder<ItemEstoque> builder)
    {
        builder.ToTable("itens_estoque");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Tipo).HasConversion<string>().HasMaxLength(25).IsRequired();
        builder.Property(i => i.Nome).HasMaxLength(160).IsRequired();
        builder.Property(i => i.Categoria).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(i => i.UnidadeMedida).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(i => i.IngredienteId);
        builder.Property(i => i.ReceitaId);
        builder.Property(i => i.TamanhoPacoteId);
        builder.Property(i => i.QuantidadeAtual).HasPrecision(18, 3).IsRequired();
        builder.Property(i => i.QuantidadeMinima).HasPrecision(18, 3).IsRequired();
        builder.Property(i => i.CustoMedio).HasPrecision(18, 4).IsRequired();
        builder.Property(i => i.FornecedorPrincipalId);
        builder.Property(i => i.LocalArmazenamento).HasMaxLength(120);
        builder.Property(i => i.ControlaValidade).IsRequired();
        builder.Property(i => i.Ativo).IsRequired();
        builder.Property(i => i.Observacoes).HasMaxLength(1000);

        builder.Ignore(i => i.AbaixoDoMinimo);

        builder.HasIndex(i => i.IngredienteId).IsUnique().HasDatabaseName("ix_itens_estoque_ingrediente");
        builder.HasIndex(i => new { i.ReceitaId, i.TamanhoPacoteId }).IsUnique().HasDatabaseName("ix_itens_estoque_receita_tamanho");
        builder.HasIndex(i => i.Nome).HasDatabaseName("ix_itens_estoque_nome");
        builder.HasIndex(i => i.Tipo).HasDatabaseName("ix_itens_estoque_tipo");

        builder
            .HasOne<Ingrediente>()
            .WithMany()
            .HasForeignKey(i => i.IngredienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_itens_estoque_ingrediente");

        builder
            .HasOne<Receita>()
            .WithMany()
            .HasForeignKey(i => i.ReceitaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_itens_estoque_receita");

        builder
            .HasOne<TamanhoPacote>()
            .WithMany()
            .HasForeignKey(i => i.TamanhoPacoteId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_itens_estoque_tamanho");

        builder
            .HasOne<Fornecedor>()
            .WithMany()
            .HasForeignKey(i => i.FornecedorPrincipalId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_itens_estoque_fornecedor");
    }
}

public sealed class LoteEstoqueConfiguration : IEntityTypeConfiguration<LoteEstoque>
{
    public void Configure(EntityTypeBuilder<LoteEstoque> builder)
    {
        builder.ToTable("lotes_estoque");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.ItemEstoqueId).IsRequired();
        builder.Property(l => l.Codigo).HasMaxLength(60).IsRequired();
        builder.Property(l => l.DataEntrada).IsRequired();
        builder.Property(l => l.Validade);
        builder.Property(l => l.QuantidadeInicial).HasPrecision(18, 3).IsRequired();
        builder.Property(l => l.QuantidadeAtual).HasPrecision(18, 3).IsRequired();
        builder.Property(l => l.CustoUnitario).HasPrecision(18, 4).IsRequired();
        builder.Property(l => l.FornecedorId);
        builder.Property(l => l.Origem).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(l => l.OrigemId);
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(15).IsRequired();

        builder.HasIndex(l => l.ItemEstoqueId).HasDatabaseName("ix_lotes_estoque_item");
        builder.HasIndex(l => l.Validade).HasDatabaseName("ix_lotes_estoque_validade");

        builder
            .HasOne(l => l.Item)
            .WithMany()
            .HasForeignKey(l => l.ItemEstoqueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_lotes_estoque_item");

        builder
            .HasOne<Fornecedor>()
            .WithMany()
            .HasForeignKey(l => l.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_lotes_estoque_fornecedor");
    }
}

public sealed class MovimentacaoEstoqueConfiguration : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.ToTable("movimentacoes_estoque");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.ItemEstoqueId).IsRequired();
        builder.Property(m => m.LoteEstoqueId);
        builder.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(25).IsRequired();
        builder.Property(m => m.Sentido).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(m => m.Quantidade).HasPrecision(18, 3).IsRequired();
        builder.Property(m => m.SaldoAnteriorItem).HasPrecision(18, 3).IsRequired();
        builder.Property(m => m.SaldoPosteriorItem).HasPrecision(18, 3).IsRequired();
        builder.Property(m => m.CustoUnitario).HasPrecision(18, 4).IsRequired();
        builder.Property(m => m.ValorTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(m => m.Usuario).HasMaxLength(160).IsRequired();
        builder.Property(m => m.DataHora).IsRequired();
        builder.Property(m => m.MotivoCodigo).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.Motivo).HasMaxLength(255);
        builder.Property(m => m.Observacao).HasMaxLength(1000);
        builder.Property(m => m.EntradaEstoqueId);
        builder.Property(m => m.AjusteEstoqueId);
        builder.Property(m => m.OrdemProducaoId);
        builder.Property(m => m.EntregaId);

        builder.HasIndex(m => m.ItemEstoqueId).HasDatabaseName("ix_mov_estoque_item");
        builder.HasIndex(m => m.DataHora).HasDatabaseName("ix_mov_estoque_data");
        builder.HasIndex(m => m.Tipo).HasDatabaseName("ix_mov_estoque_tipo");

        builder
            .HasOne(m => m.Item)
            .WithMany()
            .HasForeignKey(m => m.ItemEstoqueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_mov_estoque_item");

        builder
            .HasOne(m => m.Lote)
            .WithMany()
            .HasForeignKey(m => m.LoteEstoqueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_mov_estoque_lote");

        builder
            .HasOne(m => m.Entrada)
            .WithMany()
            .HasForeignKey(m => m.EntradaEstoqueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_mov_estoque_entrada");

        builder
            .HasOne(m => m.Ajuste)
            .WithMany()
            .HasForeignKey(m => m.AjusteEstoqueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_mov_estoque_ajuste");
    }
}

public sealed class EntradaEstoqueConfiguration : IEntityTypeConfiguration<EntradaEstoque>
{
    public void Configure(EntityTypeBuilder<EntradaEstoque> builder)
    {
        builder.ToTable("entradas_estoque");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ItemEstoqueId).IsRequired();
        builder.Property(e => e.LoteEstoqueId).IsRequired();
        builder.Property(e => e.FornecedorId);
        builder.Property(e => e.Quantidade).HasPrecision(18, 3).IsRequired();
        builder.Property(e => e.UnidadeMedida).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(e => e.ValorUnitario).HasPrecision(18, 4).IsRequired();
        builder.Property(e => e.ValorTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.DataCompra).IsRequired();
        builder.Property(e => e.DataEntrada).IsRequired();
        builder.Property(e => e.Validade);
        builder.Property(e => e.LoteCodigo).HasMaxLength(60).IsRequired();
        builder.Property(e => e.LocalArmazenamento).HasMaxLength(120);
        builder.Property(e => e.Usuario).HasMaxLength(160).IsRequired();
        builder.Property(e => e.Observacoes).HasMaxLength(1000);

        builder.HasIndex(e => e.ItemEstoqueId).HasDatabaseName("ix_entradas_estoque_item");

        builder
            .HasOne(e => e.Item)
            .WithMany()
            .HasForeignKey(e => e.ItemEstoqueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_entradas_estoque_item");

        builder
            .HasOne(e => e.Lote)
            .WithMany()
            .HasForeignKey(e => e.LoteEstoqueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_entradas_estoque_lote");

        builder
            .HasOne<Fornecedor>()
            .WithMany()
            .HasForeignKey(e => e.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_entradas_estoque_fornecedor");
    }
}

public sealed class AjusteEstoqueConfiguration : IEntityTypeConfiguration<AjusteEstoque>
{
    public void Configure(EntityTypeBuilder<AjusteEstoque> builder)
    {
        builder.ToTable("ajustes_estoque");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ItemEstoqueId).IsRequired();
        builder.Property(a => a.LoteEstoqueId);
        builder.Property(a => a.QuantidadeAnterior).HasPrecision(18, 3).IsRequired();
        builder.Property(a => a.QuantidadeNova).HasPrecision(18, 3).IsRequired();
        builder.Property(a => a.Diferenca).HasPrecision(18, 3).IsRequired();
        builder.Property(a => a.Motivo).HasMaxLength(255).IsRequired();
        builder.Property(a => a.Usuario).HasMaxLength(160).IsRequired();
        builder.Property(a => a.DataHora).IsRequired();
        builder.Property(a => a.Observacao).HasMaxLength(1000);

        builder.HasIndex(a => a.ItemEstoqueId).HasDatabaseName("ix_ajustes_estoque_item");

        builder
            .HasOne(a => a.Item)
            .WithMany()
            .HasForeignKey(a => a.ItemEstoqueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ajustes_estoque_item");

        builder
            .HasOne(a => a.Lote)
            .WithMany()
            .HasForeignKey(a => a.LoteEstoqueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ajustes_estoque_lote");
    }
}
