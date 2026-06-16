using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Clientes;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/motivos-cancelamento")]
public sealed class MotivosCancelamentoController : ControllerBase
{
    private readonly IMotivoCancelamentoService _service;

    public MotivosCancelamentoController(IMotivoCancelamentoService service) => _service = service;

    /// <summary>Lista os motivos de cancelamento (por padrão só os ativos).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MotivoCancelamentoDto>>> Listar([FromQuery] bool incluirInativos, CancellationToken ct)
        => Ok(await _service.ListarAsync(incluirInativos, ct));

    /// <summary>Obtém um motivo de cancelamento.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<MotivoCancelamentoDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um motivo de cancelamento.</summary>
    [HttpPost]
    public async Task<ActionResult<MotivoCancelamentoDto>> Criar(SalvarMotivoCancelamentoRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um motivo de cancelamento.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<MotivoCancelamentoDto>> Atualizar(long id, SalvarMotivoCancelamentoRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa um motivo de cancelamento.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
