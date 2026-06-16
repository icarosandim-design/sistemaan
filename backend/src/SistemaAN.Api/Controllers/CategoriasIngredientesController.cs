using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Catalog;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/categorias-ingredientes")]
public sealed class CategoriasIngredientesController : ControllerBase
{
    private readonly ICategoriaIngredienteService _service;

    public CategoriasIngredientesController(ICategoriaIngredienteService service) => _service = service;

    /// <summary>Lista as categorias de ingredientes (ativas por padrão).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaIngredienteDto>>> Listar([FromQuery] bool incluirInativas, CancellationToken ct)
        => Ok(await _service.ListarAsync(incluirInativas, ct));

    /// <summary>Obtém uma categoria de ingrediente.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<CategoriaIngredienteDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria uma categoria de ingrediente.</summary>
    [HttpPost]
    public async Task<ActionResult<CategoriaIngredienteDto>> Criar(SalvarCategoriaIngredienteRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza uma categoria de ingrediente.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<CategoriaIngredienteDto>> Atualizar(long id, SalvarCategoriaIngredienteRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa uma categoria de ingrediente.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
