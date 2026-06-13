namespace SistemaAN.Application.Producao;

public interface IProducaoService
{
    Task<DemandaDto> ObterDemandaAsync(DateOnly inicio, DateOnly fim, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrdemProducaoResumoDto>> ListarOrdensAsync(CancellationToken cancellationToken = default);

    Task<OrdemProducaoDto?> ObterPorDataAsync(DateOnly data, CancellationToken cancellationToken = default);

    Task<OrdemProducaoDto> ObterAsync(long id, CancellationToken cancellationToken = default);

    Task<OrdemProducaoDto> CriarOuObterOrdemAsync(DateOnly data, CancellationToken cancellationToken = default);

    Task<OrdemProducaoDto> AdicionarFichasAsync(long ordemId, AdicionarFichasRequest request, CancellationToken cancellationToken = default);

    Task<OrdemProducaoDto> RemoverFichaAsync(long fichaId, CancellationToken cancellationToken = default);
}
