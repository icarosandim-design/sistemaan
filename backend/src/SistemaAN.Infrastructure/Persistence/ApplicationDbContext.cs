using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Consumo;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Infrastructure.Persistence;

/// <summary>
/// Contexto principal do EF Core. As configurações de mapeamento são aplicadas
/// por reflexão a partir das classes <c>IEntityTypeConfiguration</c> deste assembly.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Papel> Papeis => Set<Papel>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<CategoriaIngrediente> CategoriasIngredientes => Set<CategoriaIngrediente>();

    public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();

    public DbSet<FaixaConsumo> FaixasConsumo => Set<FaixaConsumo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
