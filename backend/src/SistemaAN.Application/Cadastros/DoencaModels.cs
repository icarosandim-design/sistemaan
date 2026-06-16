namespace SistemaAN.Application.Cadastros;

public sealed record DoencaDto(long Id, string Nome, int Ordem, bool Ativo);

public sealed record SalvarDoencaRequest(string Nome, int Ordem, bool Ativo);
