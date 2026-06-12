namespace SistemaAN.Application.Catalog;

public interface ICategoriaIngredienteService
{
    Task<IReadOnlyList<CategoriaIngredienteDto>> ListarAsync(bool incluirInativas = false, CancellationToken cancellationToken = default);

    Task<CategoriaIngredienteDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<CategoriaIngredienteDto> CriarAsync(SalvarCategoriaIngredienteRequest request, CancellationToken cancellationToken = default);

    Task<CategoriaIngredienteDto> AtualizarAsync(long id, SalvarCategoriaIngredienteRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
