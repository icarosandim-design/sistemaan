using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Pacotes;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/tamanhos-pacote")]
public sealed class TamanhosPacoteController : ControllerBase
{
    private readonly ITamanhoPacoteService _service;

    public TamanhosPacoteController(ITamanhoPacoteService service) => _service = service;

    /// <summary>Lista os tamanhos de pacote.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TamanhoPacoteDto>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarAsync(ct));

    /// <summary>Obtém um tamanho de pacote.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<TamanhoPacoteDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um tamanho de pacote.</summary>
    [HttpPost]
    public async Task<ActionResult<TamanhoPacoteDto>> Criar(SalvarTamanhoPacoteRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um tamanho de pacote.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<TamanhoPacoteDto>> Atualizar(long id, SalvarTamanhoPacoteRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa um tamanho de pacote.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
