using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).HasMaxLength(120).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(160).IsRequired();
        builder.Property(u => u.SenhaHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Ativo).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        builder
            .HasMany(u => u.Papeis)
            .WithMany()
            .UsingEntity(join => join.ToTable("usuarios_papeis"));

        builder
            .HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Papeis é uma skip navigation (N:N); RefreshTokens é coleção 1:N.
        builder.Metadata.FindSkipNavigation(nameof(Usuario.Papeis))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(Usuario.RefreshTokens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
