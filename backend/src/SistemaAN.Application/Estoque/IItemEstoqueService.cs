namespace SistemaAN.Application.Estoque;

public interface IItemEstoqueService
{
    Task<IReadOnlyList<ItemEstoqueDto>> ListarAsync(bool apenasAbaixoMinimo = false, CancellationToken cancellationToken = default);

    Task<ItemEstoqueDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<ItemEstoqueDto> CriarInsumoAsync(CriarItemInsumoRequest request, CancellationToken cancellationToken = default);

    Task<ItemEstoqueDto> CriarProdutoAcabadoAsync(CriarItemProdutoAcabadoRequest request, CancellationToken cancellationToken = default);

    Task<ItemEstoqueDto> AtualizarAsync(long id, AtualizarItemEstoqueRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
