using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Cadastros;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/racas")]
public sealed class RacasController : ControllerBase
{
    private readonly IRacaService _service;

    public RacasController(IRacaService service) => _service = service;

    /// <summary>Lista as raças (por padrão só as ativas).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RacaDto>>> Listar([FromQuery] bool incluirInativas, CancellationToken ct)
        => Ok(await _service.ListarAsync(incluirInativas, ct));

    /// <summary>Obtém uma raça.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<RacaDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria uma raça.</summary>
    [HttpPost]
    public async Task<ActionResult<RacaDto>> Criar(SalvarRacaRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza uma raça.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<RacaDto>> Atualizar(long id, SalvarRacaRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa uma raça.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
