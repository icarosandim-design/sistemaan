namespace SistemaAN.Application.Catalog;

public interface IIngredienteService
{
    Task<IReadOnlyList<CategoriaDto>> ListarCategoriasAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IngredienteDto>> ListarAsync(CancellationToken cancellationToken = default);

    Task<IngredienteDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<IngredienteDto> CriarAsync(SalvarIngredienteRequest request, CancellationToken cancellationToken = default);

    Task<IngredienteDto> AtualizarAsync(long id, SalvarIngredienteRequest request, CancellationToken cancellationToken = default);

    Task ExcluirAsync(long id, CancellationToken cancellationToken = default);
}
