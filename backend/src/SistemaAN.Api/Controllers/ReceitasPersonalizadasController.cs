using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Receitas;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api")]
public sealed class ReceitasPersonalizadasController : ControllerBase
{
    private readonly IReceitaPersonalizadaService _service;

    public ReceitasPersonalizadasController(IReceitaPersonalizadaService service) => _service = service;

    /// <summary>Lista as receitas personalizadas de um pet.</summary>
    [HttpGet("pets/{petId:long}/receitas-personalizadas")]
    public async Task<ActionResult<IReadOnlyList<ReceitaPersonalizadaDto>>> ListarPorPet(long petId, CancellationToken ct)
        => Ok(await _service.ListarPorPetAsync(petId, ct));

    /// <summary>Obtém uma receita personalizada.</summary>
    [HttpGet("receitas-personalizadas/{id:long}")]
    public async Task<ActionResult<ReceitaPersonalizadaDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria uma receita personalizada para o pet.</summary>
    [HttpPost("pets/{petId:long}/receitas-personalizadas")]
    public async Task<ActionResult<ReceitaPersonalizadaDto>> Criar(long petId, SalvarReceitaPersonalizadaRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(petId, request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza uma receita personalizada.</summary>
    [HttpPut("receitas-personalizadas/{id:long}")]
    public async Task<ActionResult<ReceitaPersonalizadaDto>> Atualizar(long id, SalvarReceitaPersonalizadaRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa uma receita personalizada.</summary>
    [HttpPut("receitas-personalizadas/{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
