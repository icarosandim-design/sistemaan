namespace SistemaAN.Application.Relatorios;

/// <summary>
/// Relatórios gerenciais (somente leitura/agregação dos dados operacionais existentes).
/// Nenhum dado é inventado: indicadores sem fonte de dados são marcados como pendentes.
/// </summary>
public interface IRelatorioService
{
    Task<DashboardDto> ObterDashboardAsync(DateOnly inicio, DateOnly fim, long? ingredienteId, CancellationToken ct = default);

    Task<RelatorioVendasDto> ObterVendasAsync(RelatorioFiltro filtro, CancellationToken ct = default);

    Task<RelatorioCancelamentosDto> ObterCancelamentosAsync(RelatorioFiltro filtro, CancellationToken ct = default);

    Task<RelatorioProducaoDto> ObterProducaoAsync(RelatorioFiltro filtro, CancellationToken ct = default);

    Task<RelatorioPerdasDto> ObterPerdasAsync(RelatorioFiltro filtro, CancellationToken ct = default);

    Task<RelatorioCustosInsumosDto> ObterCustosInsumosAsync(RelatorioFiltro filtro, CancellationToken ct = default);

    Task<RelatorioEstoqueDto> ObterEstoqueAsync(RelatorioFiltro filtro, CancellationToken ct = default);
}
