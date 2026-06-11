namespace SistemaAN.Application.Catalog;

public sealed record CategoriaDto(long Id, string Nome);

public sealed record IngredienteDto(
    long Id,
    string Nome,
    long CategoriaId,
    string Categoria,
    string TipoConversao,
    decimal Coeficiente,
    decimal CustoKg,
    bool Ativo);

public sealed record SalvarIngredienteRequest(
    string Nome,
    long CategoriaId,
    string TipoConversao,
    decimal Coeficiente,
    decimal CustoKg,
    bool Ativo);
