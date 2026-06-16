using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Relatorios;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Api.Controllers;

/// <summary>
/// Relatórios gerenciais. Dados sensíveis (vendas, custos, perdas, cancelamentos):
/// acesso restrito ao Administrador nesta fase.
/// </summary>
[ApiController]
[Authorize(Roles = PapeisDoSistema.Administrador)]
[Route("api/relatorios")]
public sealed class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _service;

    public RelatoriosController(IRelatorioService service) => _service = service;

    /// <summary>Dashboard gerencial: cards e os 6 gráficos do período.</summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard(
        [FromQuery] DateOnly? inicio, [FromQuery] DateOnly? fim, [FromQuery] long? ingredienteId, CancellationToken ct)
    {
        var f = fim ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var i = inicio ?? f.AddDays(-180);
        return Ok(await _service.ObterDashboardAsync(i, f, ingredienteId, ct));
    }

    /// <summary>Relatório de vendas (Assinatura PF, Venda Avulsa PF e Pedido PJ).</summary>
    [HttpGet("vendas")]
    public async Task<ActionResult<RelatorioVendasDto>> Vendas([FromQuery] RelatorioFiltro filtro, CancellationToken ct)
        => Ok(await _service.ObterVendasAsync(filtro, ct));

    /// <summary>Relatório de cancelamentos de clientes/assinaturas.</summary>
    [HttpGet("cancelamentos")]
    public async Task<ActionResult<RelatorioCancelamentosDto>> Cancelamentos([FromQuery] RelatorioFiltro filtro, CancellationToken ct)
        => Ok(await _service.ObterCancelamentosAsync(filtro, ct));

    /// <summary>Relatório de produção planejado × real.</summary>
    [HttpGet("producao-planejado-real")]
    public async Task<ActionResult<RelatorioProducaoDto>> Producao([FromQuery] RelatorioFiltro filtro, CancellationToken ct)
        => Ok(await _service.ObterProducaoAsync(filtro, ct));

    /// <summary>Relatório de perdas de ingredientes.</summary>
    [HttpGet("perdas-ingredientes")]
    public async Task<ActionResult<RelatorioPerdasDto>> Perdas([FromQuery] RelatorioFiltro filtro, CancellationToken ct)
        => Ok(await _service.ObterPerdasAsync(filtro, ct));

    /// <summary>Relatório de evolução de custo dos insumos e custo médio.</summary>
    [HttpGet("evolucao-custos-insumos")]
    public async Task<ActionResult<RelatorioCustosInsumosDto>> CustosInsumos([FromQuery] RelatorioFiltro filtro, CancellationToken ct)
        => Ok(await _service.ObterCustosInsumosAsync(filtro, ct));

    /// <summary>Relatório de estoque e produto acabado.</summary>
    [HttpGet("estoque-produto-acabado")]
    public async Task<ActionResult<RelatorioEstoqueDto>> Estoque([FromQuery] RelatorioFiltro filtro, CancellationToken ct)
        => Ok(await _service.ObterEstoqueAsync(filtro, ct));
}
