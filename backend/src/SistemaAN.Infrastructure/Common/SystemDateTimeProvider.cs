using SistemaAN.Application.Common.Interfaces;

namespace SistemaAN.Infrastructure.Common;

/// <summary>Implementação real de <see cref="IDateTimeProvider"/> baseada no relógio do sistema (UTC).</summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
