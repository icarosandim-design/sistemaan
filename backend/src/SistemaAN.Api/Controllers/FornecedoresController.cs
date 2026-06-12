using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Estoque;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fornecedores")]
public sealed class FornecedoresController : ControllerBase
{
    private readonly IFornecedorService _service;

    public FornecedoresController(IFornecedorService service) => _service = service;

    /// <summary>Lista fornecedores (opcionalmente apenas ativos).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FornecedorDto>>> Listar([FromQuery] bool apenasAtivos, CancellationToken ct)
        => Ok(await _service.ListarAsync(apenasAtivos, ct));

    /// <summary>Obtém um fornecedor.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<FornecedorDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um fornecedor.</summary>
    [HttpPost]
    public async Task<ActionResult<FornecedorDto>> Criar(SalvarFornecedorRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um fornecedor.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<FornecedorDto>> Atualizar(long id, SalvarFornecedorRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa um fornecedor.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
