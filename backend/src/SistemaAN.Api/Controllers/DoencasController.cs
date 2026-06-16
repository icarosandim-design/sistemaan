using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Cadastros;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/doencas")]
public sealed class DoencasController : ControllerBase
{
    private readonly IDoencaService _service;

    public DoencasController(IDoencaService service) => _service = service;

    /// <summary>Lista as doenças (por padrão só as ativas).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DoencaDto>>> Listar([FromQuery] bool incluirInativas, CancellationToken ct)
        => Ok(await _service.ListarAsync(incluirInativas, ct));

    /// <summary>Obtém uma doença.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<DoencaDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria uma doença.</summary>
    [HttpPost]
    public async Task<ActionResult<DoencaDto>> Criar(SalvarDoencaRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza uma doença.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<DoencaDto>> Atualizar(long id, SalvarDoencaRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa uma doença.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
