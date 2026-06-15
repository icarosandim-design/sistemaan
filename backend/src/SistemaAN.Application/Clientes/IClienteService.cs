namespace SistemaAN.Application.Clientes;

public interface IClienteService
{
    Task<IReadOnlyList<ClienteDto>> ListarAsync(string? natureza = null, CancellationToken cancellationToken = default);

    Task<ClienteDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<ClienteDto> CriarAsync(SalvarClienteRequest request, CancellationToken cancellationToken = default);

    Task<ClienteDto> AtualizarAsync(long id, SalvarClienteRequest request, CancellationToken cancellationToken = default);

    Task CancelarAsync(long id, string motivo, CancellationToken cancellationToken = default);

    Task ReativarAsync(long id, CancellationToken cancellationToken = default);
}
