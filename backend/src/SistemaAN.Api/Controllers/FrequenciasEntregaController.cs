using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Entregas;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/frequencias-entrega")]
public sealed class FrequenciasEntregaController : ControllerBase
{
    private readonly IFrequenciaEntregaService _service;

    public FrequenciasEntregaController(IFrequenciaEntregaService service) => _service = service;

    /// <summary>Lista as frequências de entrega.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FrequenciaEntregaDto>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarAsync(ct));

    /// <summary>Obtém uma frequência.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<FrequenciaEntregaDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria uma frequência.</summary>
    [HttpPost]
    public async Task<ActionResult<FrequenciaEntregaDto>> Criar(SalvarFrequenciaEntregaRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza uma frequência.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<FrequenciaEntregaDto>> Atualizar(long id, SalvarFrequenciaEntregaRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa uma frequência.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }

    /// <summary>
    /// Pré-visualização da agenda de entregas (cálculo, sem persistir).
    /// Demonstra o serviço que futuramente gerará as entregas do pet.
    /// </summary>
    [HttpGet("preview-agenda")]
    public ActionResult<IReadOnlyList<DateOnly>> PreviewAgenda(
        [FromQuery] DateOnly dataInicial,
        [FromQuery] int diasCiclo,
        [FromQuery] int horizonteDias = 60)
    {
        if (diasCiclo <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["diasCiclo"] = ["A quantidade de dias deve ser maior que zero."],
            });
        }

        return Ok(CalculadoraAgenda.Gerar(dataInicial, diasCiclo, horizonteDias));
    }
}
