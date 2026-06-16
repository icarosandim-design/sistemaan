using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Clientes;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/clientes-pj")]
public sealed class ClientesPjController : ControllerBase
{
    private readonly IClientePjService _service;

    public ClientesPjController(IClientePjService service) => _service = service;

    /// <summary>Lista os clientes PJ (filtros opcionais por busca e status).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientePjResumoDto>>> Listar(
        [FromQuery] string? busca, [FromQuery] bool? ativo, CancellationToken ct)
        => Ok(await _service.ListarAsync(busca, ativo, ct));

    /// <summary>Tipos de cliente PJ (fixos).</summary>
    [HttpGet("tipos")]
    public ActionResult<IReadOnlyList<TipoPjOpcaoDto>> Tipos() => Ok(_service.ListarTipos());

    /// <summary>Obtém um cliente PJ.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ClientePjDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um cliente PJ.</summary>
    [HttpPost]
    public async Task<ActionResult<ClientePjDto>> Criar(SalvarClientePjRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um cliente PJ.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ClientePjDto>> Atualizar(long id, SalvarClientePjRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Ativa/inativa um cliente PJ (sem exclusão física).</summary>
    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<ClientePjDto>> AlternarStatus(long id, [FromBody] AlternarStatusPjRequest request, CancellationToken ct)
        => Ok(await _service.AlternarStatusAsync(id, request.Ativo, ct));
}

public sealed record AlternarStatusPjRequest(bool Ativo);
