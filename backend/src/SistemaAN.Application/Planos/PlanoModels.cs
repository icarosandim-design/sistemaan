namespace SistemaAN.Application.Planos;

public sealed record PlanoItemPacoteDto(long TamanhoPacoteId, int Quantidade);

public sealed record PlanoItemDto(
    long Id,
    long ReceitaId,
    int? QuantidadeCicloGramas,
    int? QuantidadePacotes,
    IReadOnlyList<PlanoItemPacoteDto> Pacotes);

public sealed record PlanoAlimentarDto(
    long Id,
    long PetId,
    long FrequenciaEntregaId,
    DateOnly PrimeiraEntrega,
    int? GramasDiaSugeridas,
    int? GramasDiaAjustadas,
    string Tipo,
    bool Ativo,
    IReadOnlyList<PlanoItemDto> Itens);

public sealed record SalvarPlanoItemPacoteRequest(long TamanhoPacoteId, int Quantidade);

public sealed record SalvarPlanoItemRequest(
    long ReceitaId,
    int? QuantidadeCicloGramas,
    int? QuantidadePacotes,
    IReadOnlyList<SalvarPlanoItemPacoteRequest>? Pacotes);

public sealed record SalvarPlanoRequest(
    long FrequenciaEntregaId,
    DateOnly PrimeiraEntrega,
    int? GramasDiaSugeridas,
    int? GramasDiaAjustadas,
    string Tipo,
    IReadOnlyList<SalvarPlanoItemRequest> Itens);

public sealed record AlternarStatusRequest(bool Ativo);
