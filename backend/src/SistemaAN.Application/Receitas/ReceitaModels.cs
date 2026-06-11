namespace SistemaAN.Application.Receitas;

public sealed record ItemReceitaDto(long IngredienteId, string Ingrediente, string Categoria, int Gramas);

public sealed record ReceitaCasaDto(
    long Id,
    string Codigo,
    string Nome,
    bool Ativo,
    string Observacoes,
    IReadOnlyList<ItemReceitaDto> Itens,
    int Rendimento,
    decimal CustoTotal,
    decimal CustoPorKgCozido);

public sealed record SalvarItemReceitaRequest(long IngredienteId, int Gramas);

public sealed record SalvarReceitaCasaRequest(
    string Codigo,
    string Nome,
    bool Ativo,
    string Observacoes,
    IReadOnlyList<SalvarItemReceitaRequest> Itens);

public sealed record AlternarStatusRequest(bool Ativo);
