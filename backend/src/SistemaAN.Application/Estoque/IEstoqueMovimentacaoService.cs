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

    /// <summary>
    /// Baixa de insumo por produção (FIFO). NÃO chama SaveChanges (o chamador controla a transação).
    /// Retorna false se não houver saldo suficiente (não baixa nada).
    /// </summary>
    Task<bool> BaixarPorProducaoAsync(long itemEstoqueId, decimal quantidade, long ordemProducaoId, string usuario, string? observacao, CancellationToken cancellationToken = default);

    /// <summary>
    /// Entrada de produto acabado por produção (custo 0 nesta fase). NÃO chama SaveChanges.
    /// </summary>
    Task EntrarPorProducaoAsync(long itemEstoqueId, decimal quantidade, long ordemProducaoId, string usuario, CancellationToken cancellationToken = default);
}
