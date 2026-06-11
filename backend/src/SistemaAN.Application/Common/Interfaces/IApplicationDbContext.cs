using Microsoft.EntityFrameworkCore;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Application.Common.Interfaces;

/// <summary>
/// Abstração do contexto de persistência exposta à camada de Aplicação.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Usuario> Usuarios { get; }

    DbSet<Papel> Papeis { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<CategoriaIngrediente> CategoriasIngredientes { get; }

    DbSet<Ingrediente> Ingredientes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
