namespace SistemaAN.Application.Catalog;

public sealed record CategoriaIngredienteDto(
    long Id,
    string Nome,
    string? Descricao,
    int Ordem,
    bool Ativo);

public sealed record SalvarCategoriaIngredienteRequest(
    string Nome,
    string? Descricao,
    int Ordem,
    bool Ativo);

public sealed record AlternarStatusRequest(bool Ativo);
