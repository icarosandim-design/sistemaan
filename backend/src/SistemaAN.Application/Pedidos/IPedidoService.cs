namespace SistemaAN.Application.Pedidos;

/// <summary>Pedidos PJ (venda de produto acabado da Casa) e a geração da Entrega vinculada.</summary>
public interface IPedidoService
{
    Task<IReadOnlyList<PedidoResumoDto>> ListarPorClienteAsync(long clienteId, CancellationToken cancellationToken = default);

    Task<PedidoDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<PedidoDto> CriarAsync(SalvarPedidoRequest request, CancellationToken cancellationToken = default);

    Task<PedidoDto> AtualizarAsync(long id, SalvarPedidoRequest request, CancellationToken cancellationToken = default);

    Task<PedidoDto> ConfirmarAsync(long id, string usuario, CancellationToken cancellationToken = default);

    Task<PedidoDto> CancelarAsync(long id, string? motivo, string usuario, CancellationToken cancellationToken = default);
}
