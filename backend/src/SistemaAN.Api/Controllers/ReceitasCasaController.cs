using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Receitas;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/receitas-casa")]
public sealed class ReceitasCasaController : ControllerBase
{
    private readonly IReceitaCasaService _service;

    public ReceitasCasaController(IReceitaCasaService service) => _service = service;

    /// <summary>Lista as receitas da casa (com rendimento e custo calculados).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReceitaCasaDto>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarAsync(ct));

    /// <summary>Obtém uma receita da casa.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ReceitaCasaDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria uma receita da casa (valida soma = 1.000 g).</summary>
    [HttpPost]
    public async Task<ActionResult<ReceitaCasaDto>> Criar(SalvarReceitaCasaRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza uma receita da casa.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ReceitaCasaDto>> Atualizar(long id, SalvarReceitaCasaRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa uma receita da casa.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
