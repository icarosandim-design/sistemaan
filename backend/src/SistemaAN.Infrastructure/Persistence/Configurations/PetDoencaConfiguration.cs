using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Pets;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class PetDoencaConfiguration : IEntityTypeConfiguration<PetDoenca>
{
    public void Configure(EntityTypeBuilder<PetDoenca> builder)
    {
        builder.ToTable("pet_doencas");
        builder.HasKey(pd => pd.Id);

        builder.Property(pd => pd.PetId).IsRequired();
        builder.Property(pd => pd.DoencaId).IsRequired();

        builder.HasIndex(pd => new { pd.PetId, pd.DoencaId }).IsUnique();
        builder.HasIndex(pd => pd.DoencaId);

        builder
            .HasOne<Pet>()
            .WithMany()
            .HasForeignKey(pd => pd.PetId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_pet_doencas_pet");

        builder
            .HasOne<Doenca>()
            .WithMany()
            .HasForeignKey(pd => pd.DoencaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_pet_doencas_doenca");
    }
}
