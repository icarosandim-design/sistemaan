namespace SistemaAN.Domain.Common;

/// <summary>
/// Marcador para eventos de domínio. A timeline operacional descrita na
/// documentação (eventos_dominio) será construída sobre este conceito.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOnUtc => DateTimeOffset.UtcNow;
}
