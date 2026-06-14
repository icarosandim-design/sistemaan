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

    Task<OrdemProducaoDto> MudarStatusFichaAsync(long fichaId, string status, CancellationToken cancellationToken = default);

    Task<OrdemProducaoDto> ConcluirFichaAsync(long fichaId, ConcluirFichaRequest request, string usuario, CancellationToken cancellationToken = default);

    Task<OrdemProducaoDto> MarcarNaoFeitaAsync(long fichaId, string motivo, string usuario, CancellationToken cancellationToken = default);

    Task<OrdemProducaoDto> RegistrarConsumoAsync(long ordemId, RegistrarConsumoRequest request, CancellationToken cancellationToken = default);

    Task<FinalizacaoResultadoDto> FinalizarAsync(long ordemId, FinalizarProducaoRequest request, string usuario, CancellationToken cancellationToken = default);
}
