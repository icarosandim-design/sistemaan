using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Pacotes;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class TamanhoPacoteConfiguration : IEntityTypeConfiguration<TamanhoPacote>
{
    public void Configure(EntityTypeBuilder<TamanhoPacote> builder)
    {
        builder.ToTable("tamanhos_pacote", t =>
            t.HasCheckConstraint("ck_tamanhos_pacote_peso", "peso_gramas > 0"));

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nome).HasMaxLength(60).IsRequired();
        builder.Property(t => t.PesoGramas).IsRequired();
        builder.Property(t => t.Ativo).IsRequired();
        builder.Property(t => t.Observacao).HasMaxLength(255);

        builder.HasIndex(t => t.Nome).IsUnique();
    }
}
