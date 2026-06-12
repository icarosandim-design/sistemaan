namespace SistemaAN.Application.Pets;

public interface IPetService
{
    Task<IReadOnlyList<PetDto>> ListarPorClienteAsync(long clienteId, CancellationToken cancellationToken = default);

    Task<PetDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<PetDto> CriarAsync(long clienteId, SalvarPetRequest request, CancellationToken cancellationToken = default);

    Task<PetDto> AtualizarAsync(long id, SalvarPetRequest request, CancellationToken cancellationToken = default);

    Task InativarAsync(long id, CancellationToken cancellationToken = default);

    Task ReativarAsync(long id, CancellationToken cancellationToken = default);
}
