using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Central;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{PapeisDoSistema.Administrador},{PapeisDoSistema.Operador}")]
[Route("api/central")]
public sealed class CentralController : ControllerBase
{
    private readonly ICentralService _service;

    public CentralController(ICentralService service) => _service = service;

    /// <summary>Resumo agregado da Central Operacional (dados reais) para o dia informado ou hoje.</summary>
    [HttpGet("resumo")]
    public async Task<ActionResult<CentralResumoDto>> Resumo([FromQuery] DateOnly? data, CancellationToken ct)
        => Ok(await _service.ObterResumoAsync(data, ct));
}
