using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Producao;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/producao")]
public sealed class ProducaoController : ControllerBase
{
    private readonly IProducaoService _service;

    public ProducaoController(IProducaoService service) => _service = service;

    /// <summary>Demanda de produção (personalizadas não prontas + Casa com falta) no período.</summary>
    [HttpGet("demanda")]
    public async Task<ActionResult<DemandaDto>> Demanda([FromQuery] DateOnly inicio, [FromQuery] DateOnly fim, CancellationToken ct)
        => Ok(await _service.ObterDemandaAsync(inicio, fim, ct));

    /// <summary>Lista as ordens de produção.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrdemProducaoResumoDto>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarOrdensAsync(ct));

    /// <summary>Obtém a ordem de produção de um dia (ou 404).</summary>
    [HttpGet("dia/{data}")]
    public async Task<ActionResult<OrdemProducaoDto>> PorData(DateOnly data, CancellationToken ct)
    {
        var ordem = await _service.ObterPorDataAsync(data, ct);
        return ordem is null ? NotFound() : Ok(ordem);
    }

    /// <summary>Obtém uma ordem de produção (fichas + consolidado).</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrdemProducaoDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria (ou abre) a ordem de produção de um dia.</summary>
    [HttpPost]
    public async Task<ActionResult<OrdemProducaoDto>> Criar(CriarOrdemRequest request, CancellationToken ct)
        => Ok(await _service.CriarOuObterOrdemAsync(request.Data, ct));

    /// <summary>Adiciona fichas (personalizadas e/ou Casa) à ordem.</summary>
    [HttpPost("{id:long}/fichas")]
    public async Task<ActionResult<OrdemProducaoDto>> AdicionarFichas(long id, AdicionarFichasRequest request, CancellationToken ct)
        => Ok(await _service.AdicionarFichasAsync(id, request, ct));

    /// <summary>Remove uma ficha da ordem.</summary>
    [HttpDelete("fichas/{fichaId:long}")]
    public async Task<ActionResult<OrdemProducaoDto>> RemoverFicha(long fichaId, CancellationToken ct)
        => Ok(await _service.RemoverFichaAsync(fichaId, ct));
}
