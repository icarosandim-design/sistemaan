using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Catalog;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class IngredientesController : ControllerBase
{
    private readonly IIngredienteService _service;

    public IngredientesController(IIngredienteService service) => _service = service;

    /// <summary>Lista todos os ingredientes.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IngredienteDto>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarAsync(ct));

    /// <summary>Obtém um ingrediente por id.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<IngredienteDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um ingrediente.</summary>
    [HttpPost]
    public async Task<ActionResult<IngredienteDto>> Criar(SalvarIngredienteRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um ingrediente.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<IngredienteDto>> Atualizar(long id, SalvarIngredienteRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Exclui um ingrediente.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id, CancellationToken ct)
    {
        await _service.ExcluirAsync(id, ct);
        return NoContent();
    }
}
