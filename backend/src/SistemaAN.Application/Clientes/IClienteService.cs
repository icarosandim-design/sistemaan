namespace SistemaAN.Application.Clientes;

public interface IClienteService
{
    Task<IReadOnlyList<ClienteDto>> ListarAsync(CancellationToken cancellationToken = default);

    Task<ClienteDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<ClienteDto> CriarAsync(SalvarClienteRequest request, CancellationToken cancellationToken = default);

    Task<ClienteDto> AtualizarAsync(long id, SalvarClienteRequest request, CancellationToken cancellationToken = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default);
}
