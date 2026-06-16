using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Consumo;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/faixas-consumo")]
public sealed class FaixasConsumoController : ControllerBase
{
    private readonly IFaixaConsumoService _service;

    public FaixasConsumoController(IFaixaConsumoService service) => _service = service;

    /// <summary>Lista todas as faixas de consumo.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FaixaConsumoDto>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarAsync(ct));

    /// <summary>Consulta qual faixa ativa se aplica a um peso (kg).</summary>
    [HttpGet("aplica")]
    public async Task<ActionResult<FaixaConsumoDto>> Aplica([FromQuery] decimal peso, CancellationToken ct)
    {
        var faixa = await _service.ConsultarPorPesoAsync(peso, ct);
        return faixa is null ? NoContent() : Ok(faixa);
    }

    /// <summary>Obtém uma faixa por id.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<FaixaConsumoDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria uma faixa.</summary>
    [HttpPost]
    public async Task<ActionResult<FaixaConsumoDto>> Criar(SalvarFaixaConsumoRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza uma faixa (inclui inativação via campo ativo).</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<FaixaConsumoDto>> Atualizar(long id, SalvarFaixaConsumoRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));
}
