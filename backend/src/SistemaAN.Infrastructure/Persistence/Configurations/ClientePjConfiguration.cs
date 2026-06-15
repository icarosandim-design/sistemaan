using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class ClientePjConfiguration : IEntityTypeConfiguration<ClientePj>
{
    public void Configure(EntityTypeBuilder<ClientePj> builder)
    {
        builder.ToTable("cliente_pj");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClienteId).IsRequired();
        builder.HasIndex(c => c.ClienteId).IsUnique().HasDatabaseName("ix_cliente_pj_cliente");

        builder.Property(c => c.RazaoSocial).HasMaxLength(180).IsRequired();
        builder.Property(c => c.NomeFantasia).HasMaxLength(160).IsRequired();
        builder.Property(c => c.Cnpj).HasMaxLength(14).IsRequired();
        builder.Property(c => c.InscricaoEstadual).HasMaxLength(30);
        builder.Property(c => c.Whatsapp).HasMaxLength(40);
        builder.Property(c => c.PessoaContato).HasMaxLength(120);
        builder.Property(c => c.CargoContato).HasMaxLength(80);

        builder.Property(c => c.EntregaRua).HasMaxLength(160);
        builder.Property(c => c.EntregaNumero).HasMaxLength(20);
        builder.Property(c => c.EntregaComplemento).HasMaxLength(120);
        builder.Property(c => c.EntregaBairro).HasMaxLength(120);
        builder.Property(c => c.EntregaCidade).HasMaxLength(120);
        builder.Property(c => c.EntregaEstado).HasMaxLength(2);
        builder.Property(c => c.EntregaCep).HasMaxLength(12);

        builder.Property(c => c.TipoPJ).HasConversion<string>().HasMaxLength(25).IsRequired();
        builder.Property(c => c.CondicaoComercial).HasMaxLength(255);
        builder.Property(c => c.PrazoPagamento).HasMaxLength(60);
        builder.Property(c => c.DiaEntregaPreferencial);
        builder.Property(c => c.FrequenciaCompra).HasMaxLength(60);
        builder.Property(c => c.ObservacoesComerciais).HasMaxLength(1000);

        builder.HasIndex(c => c.Cnpj).HasDatabaseName("ix_cliente_pj_cnpj");

        builder
            .HasOne<Cliente>()
            .WithOne()
            .HasForeignKey<ClientePj>(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_cliente_pj_cliente");
    }
}
