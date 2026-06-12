namespace SistemaAN.Application.Pacotes;

public sealed record TamanhoPacoteDto(
    long Id,
    string Nome,
    int PesoGramas,
    bool Ativo,
    string? Observacao);

public sealed record SalvarTamanhoPacoteRequest(
    string Nome,
    int PesoGramas,
    string? Observacao,
    bool Ativo);

public sealed record AlternarStatusRequest(bool Ativo);
