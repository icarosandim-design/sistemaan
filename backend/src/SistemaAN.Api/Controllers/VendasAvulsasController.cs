using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Vendas;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{PapeisDoSistema.Administrador},{PapeisDoSistema.Operador}")]
[Route("api/vendas-avulsas")]
public sealed class VendasAvulsasController : ControllerBase
{
    private readonly IVendaAvulsaService _service;

    public VendasAvulsasController(IVendaAvulsaService service) => _service = service;

    private string Usuario => User.Identity?.Name ?? "sistema";

    /// <summary>Registra uma venda avulsa PF e gera a entrega única (Programada).</summary>
    [HttpPost]
    public async Task<ActionResult<VendaAvulsaResultadoDto>> Criar(SalvarVendaAvulsaRequest request, CancellationToken ct)
        => Ok(await _service.CriarAsync(request, Usuario, ct));
}
