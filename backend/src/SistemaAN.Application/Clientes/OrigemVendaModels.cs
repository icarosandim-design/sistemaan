namespace SistemaAN.Application.Clientes;

public sealed record OrigemVendaDto(long Id, string Nome, int Ordem, bool Ativo);

public sealed record SalvarOrigemVendaRequest(string Nome, int Ordem, bool Ativo);

public sealed record AlternarStatusRequest(bool Ativo);
