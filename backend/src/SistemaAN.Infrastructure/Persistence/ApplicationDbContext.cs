using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Interfaces;

namespace SistemaAN.Infrastructure.Persistence;

/// <summary>
/// Contexto principal do EF Core. Os <c>DbSet</c> dos módulos de negócio serão
/// adicionados aqui (e refletidos em <see cref="IApplicationDbContext"/>) nas
/// próximas etapas. As configurações de mapeamento são aplicadas por reflexão a
/// partir das classes <c>IEntityTypeConfiguration</c> deste assembly.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
