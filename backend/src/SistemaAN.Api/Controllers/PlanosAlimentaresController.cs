using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Planos;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api")]
public sealed class PlanosAlimentaresController : ControllerBase
{
    private readonly IPlanoAlimentarService _service;

    public PlanosAlimentaresController(IPlanoAlimentarService service) => _service = service;

    /// <summary>Obtém o plano alimentar vigente do pet (ou vazio).</summary>
    [HttpGet("pets/{petId:long}/plano")]
    public async Task<ActionResult<PlanoAlimentarDto?>> Obter(long petId, CancellationToken ct)
        => Ok(await _service.ObterPorPetAsync(petId, ct));

    /// <summary>Cria ou atualiza o plano alimentar do pet.</summary>
    [HttpPut("pets/{petId:long}/plano")]
    public async Task<ActionResult<PlanoAlimentarDto>> Salvar(long petId, SalvarPlanoRequest request, CancellationToken ct)
        => Ok(await _service.SalvarAsync(petId, request, ct));

    /// <summary>Inativa/reativa o plano alimentar do pet.</summary>
    [HttpPut("pets/{petId:long}/plano/status")]
    public async Task<IActionResult> AlternarStatus(long petId, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(petId, request.Ativo, ct);
        return NoContent();
    }
}
