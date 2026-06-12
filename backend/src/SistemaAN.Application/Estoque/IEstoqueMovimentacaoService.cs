namespace SistemaAN.Application.Estoque;

public interface IEstoqueMovimentacaoService
{
    Task<ItemEstoqueDto> RegistrarEntradaAsync(RegistrarEntradaRequest request, string usuario, CancellationToken cancellationToken = default);

    Task<ItemEstoqueDto> RegistrarSaidaAsync(RegistrarSaidaRequest request, string usuario, CancellationToken cancellationToken = default);

    Task<ItemEstoqueDto> RegistrarAjusteAsync(RegistrarAjusteRequest request, string usuario, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LoteEstoqueDto>> ListarLotesAsync(long itemEstoqueId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MovimentacaoEstoqueDto>> ListarMovimentacoesAsync(long itemEstoqueId, CancellationToken cancellationToken = default);

    Task<MovimentacaoPaginaDto> ListarMovimentacoesGeralAsync(FiltroMovimentacoesRequest filtro, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EntradaCompraDto>> ListarEntradasAsync(FiltroEntradasRequest filtro, CancellationToken cancellationToken = default);
}
