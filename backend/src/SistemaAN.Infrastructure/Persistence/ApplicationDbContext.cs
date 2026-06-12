using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Consumo;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Identity;
using SistemaAN.Domain.Pacotes;
using SistemaAN.Domain.Pets;
using SistemaAN.Domain.Receitas;

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

    public DbSet<Receita> Receitas => Set<Receita>();

    public DbSet<ItemReceita> ItensReceita => Set<ItemReceita>();

    public DbSet<FrequenciaEntrega> FrequenciasEntrega => Set<FrequenciaEntrega>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Pet> Pets => Set<Pet>();

    public DbSet<TamanhoPacote> TamanhosPacote => Set<TamanhoPacote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
