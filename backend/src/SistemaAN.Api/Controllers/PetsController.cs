using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Pets;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class PetsController : ControllerBase
{
    private readonly IPetService _service;

    public PetsController(IPetService service) => _service = service;

    /// <summary>Lista os pets de um cliente.</summary>
    [HttpGet("clientes/{clienteId:long}/pets")]
    public async Task<ActionResult<IReadOnlyList<PetDto>>> ListarPorCliente(long clienteId, CancellationToken ct)
        => Ok(await _service.ListarPorClienteAsync(clienteId, ct));

    /// <summary>Obtém um pet.</summary>
    [HttpGet("pets/{id:long}")]
    public async Task<ActionResult<PetDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cadastra um pet para um cliente.</summary>
    [HttpPost("clientes/{clienteId:long}/pets")]
    public async Task<ActionResult<PetDto>> Criar(long clienteId, SalvarPetRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(clienteId, request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um pet.</summary>
    [HttpPut("pets/{id:long}")]
    public async Task<ActionResult<PetDto>> Atualizar(long id, SalvarPetRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa um pet.</summary>
    [HttpPut("pets/{id:long}/inativar")]
    public async Task<IActionResult> Inativar(long id, CancellationToken ct)
    {
        await _service.InativarAsync(id, ct);
        return NoContent();
    }

    /// <summary>Reativa um pet inativo.</summary>
    [HttpPut("pets/{id:long}/reativar")]
    public async Task<IActionResult> Reativar(long id, CancellationToken ct)
    {
        await _service.ReativarAsync(id, ct);
        return NoContent();
    }
}
