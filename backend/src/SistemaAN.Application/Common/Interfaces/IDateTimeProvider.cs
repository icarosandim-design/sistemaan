namespace SistemaAN.Application.Common.Interfaces;

/// <summary>
/// Abstração de tempo (facilita testes determinísticos do recálculo de datas
/// de entrega e dos ciclos de produção descritos na documentação).
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }
}
