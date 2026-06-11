using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Catalog;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/categorias-ingredientes")]
public sealed class CategoriasIngredientesController : ControllerBase
{
    private readonly IIngredienteService _service;

    public CategoriasIngredientesController(IIngredienteService service) => _service = service;

    /// <summary>Lista as categorias de ingredientes ativas.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarCategoriasAsync(ct));
}
