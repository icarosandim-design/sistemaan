using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Clientes;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service) => _service = service;

    /// <summary>Lista os clientes.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteDto>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarAsync(ct));

    /// <summary>Obtém um cliente.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ClienteDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um cliente.</summary>
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Criar(SalvarClienteRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um cliente.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ClienteDto>> Atualizar(long id, SalvarClienteRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Ativa/inativa um cliente.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusClienteRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
