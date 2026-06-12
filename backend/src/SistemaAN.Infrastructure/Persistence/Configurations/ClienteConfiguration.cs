using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes", t =>
        {
            t.HasCheckConstraint("ck_clientes_dia_cobranca", "dia_cobranca IS NULL OR (dia_cobranca BETWEEN 1 AND 31)");
            t.HasCheckConstraint("ck_clientes_valor", "valor_recorrente_mensal >= 0");
        });

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome).HasMaxLength(160).IsRequired();
        builder.Property(c => c.Cpf).HasMaxLength(11);
        builder.Property(c => c.Telefone).HasMaxLength(40);
        builder.Property(c => c.Email).HasMaxLength(160);
        builder.Property(c => c.OrigemVenda).HasMaxLength(60);
        builder.Property(c => c.Observacoes).HasMaxLength(1000);

        builder.Property(c => c.Rua).HasMaxLength(160);
        builder.Property(c => c.Numero).HasMaxLength(20);
        builder.Property(c => c.Complemento).HasMaxLength(120);
        builder.Property(c => c.Cep).HasMaxLength(12);
        builder.Property(c => c.Bairro).HasMaxLength(120);
        builder.Property(c => c.Cidade).HasMaxLength(120);
        builder.Property(c => c.Estado).HasMaxLength(2);

        builder.Property(c => c.Ativo).IsRequired();
        builder.Property(c => c.MotivoCancelamento).HasMaxLength(500);

        builder.Property(c => c.TipoCliente).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.FormaPagamento).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.DiaCobranca);
        builder.Property(c => c.ValorRecorrenteMensal).HasPrecision(12, 2).IsRequired();
        builder.Property(c => c.StatusFinanceiro).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(c => c.ObservacoesFinanceiras).HasMaxLength(1000);

        builder.HasIndex(c => c.Nome);
        builder.HasIndex(c => c.Cpf).IsUnique().HasFilter("cpf IS NOT NULL");
    }
}
