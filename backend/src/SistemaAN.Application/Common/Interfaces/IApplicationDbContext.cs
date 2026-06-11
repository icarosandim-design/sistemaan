namespace SistemaAN.Application.Common.Interfaces;

/// <summary>
/// Abstração do contexto de persistência exposta à camada de Aplicação.
/// Os <c>DbSet</c> dos módulos de negócio serão declarados aqui conforme forem criados,
/// mantendo a Application desacoplada do EF Core/Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
