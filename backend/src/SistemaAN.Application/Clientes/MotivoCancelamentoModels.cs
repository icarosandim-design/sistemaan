namespace SistemaAN.Application.Clientes;

public sealed record MotivoCancelamentoDto(long Id, string Nome, int Ordem, bool Ativo, string? Observacoes);

public sealed record SalvarMotivoCancelamentoRequest(string Nome, int Ordem, bool Ativo, string? Observacoes);
