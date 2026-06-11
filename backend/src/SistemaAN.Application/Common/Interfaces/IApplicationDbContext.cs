using Microsoft.EntityFrameworkCore;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Application.Common.Interfaces;

/// <summary>
/// Abstração do contexto de persistência exposta à camada de Aplicação.
/// Os <c>DbSet</c> dos demais módulos de negócio serão adicionados aqui conforme
/// forem criados, mantendo a Application desacoplada da Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Usuario> Usuarios { get; }

    DbSet<Papel> Papeis { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
