namespace SistemaAN.Application.Consumo;

public sealed record FaixaConsumoDto(
    long Id,
    decimal PesoInicial,
    decimal PesoFinal,
    int GramasPorDia,
    bool Ativo);

public sealed record SalvarFaixaConsumoRequest(
    decimal PesoInicial,
    decimal PesoFinal,
    int GramasPorDia,
    bool Ativo);
