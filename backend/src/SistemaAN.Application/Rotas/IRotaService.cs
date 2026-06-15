namespace SistemaAN.Application.Rotas;

/// <summary>Planejamento de rotas/saídas de entrega (várias por dia).</summary>
public interface IRotaService
{
    Task<IReadOnlyList<RotaResumoDto>> ListarPorDataAsync(DateOnly data, CancellationToken cancellationToken = default);

    Task<RotaDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EntregaDisponivelDto>> DisponiveisAsync(DateOnly data, CancellationToken cancellationToken = default);

    Task<RotaDto> CriarAsync(CriarRotaRequest request, CancellationToken cancellationToken = default);

    Task<RotaDto> AtualizarAsync(long id, AtualizarRotaRequest request, CancellationToken cancellationToken = default);

    Task<RotaDto> AdicionarEntregaAsync(long id, long entregaId, CancellationToken cancellationToken = default);

    Task<RotaDto> RemoverEntregaAsync(long id, long entregaId, CancellationToken cancellationToken = default);

    Task<RotaDto> ReordenarAsync(long id, IReadOnlyList<long> entregaIds, CancellationToken cancellationToken = default);

    Task<RotaDto> MudarStatusAsync(long id, string status, string usuario, CancellationToken cancellationToken = default);
}
