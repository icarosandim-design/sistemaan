namespace SistemaAN.Application.Receitas;

public sealed record ReceitaPersonalizadaDto(
    long Id,
    long PetId,
    string Codigo,
    string Nome,
    bool Ativo,
    string Observacoes,
    IReadOnlyList<ItemReceitaDto> Itens,
    int PesoTotalGramas,
    decimal CustoPacote,
    decimal CustoPorKgCozido);

public sealed record SalvarReceitaPersonalizadaRequest(
    string Codigo,
    string Nome,
    bool Ativo,
    string Observacoes,
    IReadOnlyList<SalvarItemReceitaRequest> Itens);
