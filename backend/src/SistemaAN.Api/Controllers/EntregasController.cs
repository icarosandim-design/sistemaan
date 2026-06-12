using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Entregas;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class EntregasController : ControllerBase
{
    private readonly IEntregaService _service;

    public EntregasController(IEntregaService service) => _service = service;

    private string Usuario => User.Identity?.Name ?? "sistema";

    /// <summary>Gera as entregas futuras (idempotente) para o horizonte informado.</summary>
    [HttpPost("entregas/gerar")]
    public async Task<ActionResult<GerarEntregasResultado>> Gerar(GerarEntregasRequest request, CancellationToken ct)
        => Ok(await _service.GerarAsync(request.HorizonteDias, Usuario, ct));

    /// <summary>Lista entregas com filtros.</summary>
    [HttpGet("entregas")]
    public async Task<ActionResult<IReadOnlyList<EntregaResumoDto>>> Listar(
        [FromQuery] DateOnly? data,
        [FromQuery] string? status,
        [FromQuery] long? clienteId,
        [FromQuery] string? bairro,
        [FromQuery] string? cidade,
        CancellationToken ct)
        => Ok(await _service.ListarAsync(data, status, clienteId, bairro, cidade, ct));

    /// <summary>Detalhe completo da entrega.</summary>
    [HttpGet("entregas/{id:long}")]
    public async Task<ActionResult<EntregaDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Transições simples (Confirmada/Saiu para entrega/Entregue).</summary>
    [HttpPut("entregas/{id:long}/status")]
    public async Task<ActionResult<EntregaDto>> MudarStatus(long id, MudarStatusEntregaRequest request, CancellationToken ct)
        => Ok(await _service.MudarStatusAsync(id, request.Status, Usuario, ct));

    /// <summary>Marca como Não entregue (motivo obrigatório).</summary>
    [HttpPut("entregas/{id:long}/nao-entregue")]
    public async Task<ActionResult<EntregaDto>> NaoEntregue(long id, MotivoRequest request, CancellationToken ct)
        => Ok(await _service.MarcarNaoEntregueAsync(id, request.Motivo, Usuario, ct));

    /// <summary>Reagendamento pontual (motivo obrigatório). Cria nova entrega na nova data.</summary>
    [HttpPut("entregas/{id:long}/reagendar")]
    public async Task<ActionResult<EntregaDto>> Reagendar(long id, ReagendarEntregaRequest request, CancellationToken ct)
        => Ok(await _service.ReagendarAsync(id, request.NovaData, request.Motivo, Usuario, ct));

    /// <summary>Cancela a entrega (motivo obrigatório).</summary>
    [HttpPut("entregas/{id:long}/cancelar")]
    public async Task<ActionResult<EntregaDto>> Cancelar(long id, MotivoRequest request, CancellationToken ct)
        => Ok(await _service.CancelarAsync(id, request.Motivo, Usuario, ct));

    /// <summary>Regera as entregas futuras ainda não operacionais do cliente.</summary>
    [HttpPost("clientes/{clienteId:long}/regerar-entregas")]
    public async Task<ActionResult<GerarEntregasResultado>> RegerarDoCliente(long clienteId, CancellationToken ct)
        => Ok(await _service.RegerarFuturasDoClienteAsync(clienteId, Usuario, ct));
}
