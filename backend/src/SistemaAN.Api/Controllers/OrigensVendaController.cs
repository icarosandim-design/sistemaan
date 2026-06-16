using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Clientes;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/origens-venda")]
public sealed class OrigensVendaController : ControllerBase
{
    private readonly IOrigemVendaService _service;

    public OrigensVendaController(IOrigemVendaService service) => _service = service;

    /// <summary>Lista as origens de venda (por padrão só as ativas).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrigemVendaDto>>> Listar([FromQuery] bool incluirInativas, CancellationToken ct)
        => Ok(await _service.ListarAsync(incluirInativas, ct));

    /// <summary>Obtém uma origem de venda.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrigemVendaDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria uma origem de venda.</summary>
    [HttpPost]
    public async Task<ActionResult<OrigemVendaDto>> Criar(SalvarOrigemVendaRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza uma origem de venda.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<OrigemVendaDto>> Atualizar(long id, SalvarOrigemVendaRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa uma origem de venda.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
