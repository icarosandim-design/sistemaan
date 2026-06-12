namespace SistemaAN.Application.Entregas;

public interface IEntregaService
{
    Task<GerarEntregasResultado> GerarAsync(int horizonteDias, string usuario, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EntregaResumoDto>> ListarAsync(
        DateOnly? data, string? status, long? clienteId, string? bairro, string? cidade,
        CancellationToken cancellationToken = default);

    Task<EntregaDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<EntregaDto> MudarStatusAsync(long id, string status, string usuario, CancellationToken cancellationToken = default);

    Task<EntregaDto> MarcarNaoEntregueAsync(long id, string motivo, string usuario, CancellationToken cancellationToken = default);

    Task<EntregaDto> ReagendarAsync(long id, DateOnly novaData, string motivo, string usuario, CancellationToken cancellationToken = default);

    Task<EntregaDto> CancelarAsync(long id, string motivo, string usuario, CancellationToken cancellationToken = default);

    /// <summary>Regerar entregas futuras ainda não operacionais (Programada) do cliente.</summary>
    Task<GerarEntregasResultado> RegerarFuturasDoClienteAsync(long clienteId, string usuario, CancellationToken cancellationToken = default);
}
