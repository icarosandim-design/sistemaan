namespace SistemaAN.Application.Cadastros;

public sealed record RacaDto(long Id, string Nome, int Ordem, bool Ativo);

public sealed record SalvarRacaRequest(string Nome, int Ordem, bool Ativo);

public sealed record AlternarStatusRequest(bool Ativo);
