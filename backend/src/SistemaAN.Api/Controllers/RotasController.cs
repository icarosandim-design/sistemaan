using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Rotas;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/rotas")]
public sealed class RotasController : ControllerBase
{
    private readonly IRotaService _service;

    public RotasController(IRotaService service) => _service = service;

    private string Usuario => User.Identity?.Name ?? "sistema";

    /// <summary>Rotas/saídas de um dia.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RotaResumoDto>>> Listar([FromQuery] DateOnly data, CancellationToken ct)
        => Ok(await _service.ListarPorDataAsync(data, ct));

    /// <summary>Entregas do dia disponíveis para rota (não vinculadas a rota ativa).</summary>
    [HttpGet("disponiveis")]
    public async Task<ActionResult<IReadOnlyList<EntregaDisponivelDto>>> Disponiveis([FromQuery] DateOnly data, CancellationToken ct)
        => Ok(await _service.DisponiveisAsync(data, ct));

    /// <summary>Detalhe de uma rota (paradas ordenadas + alertas).</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<RotaDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria uma rota/saída.</summary>
    [HttpPost]
    public async Task<ActionResult<RotaDto>> Criar(CriarRotaRequest request, CancellationToken ct)
        => Ok(await _service.CriarAsync(request, ct));

    /// <summary>Atualiza dados da rota.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<RotaDto>> Atualizar(long id, AtualizarRotaRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Adiciona uma entrega à rota.</summary>
    [HttpPost("{id:long}/entregas")]
    public async Task<ActionResult<RotaDto>> Adicionar(long id, AdicionarEntregaRotaRequest request, CancellationToken ct)
        => Ok(await _service.AdicionarEntregaAsync(id, request.EntregaId, ct));

    /// <summary>Remove uma entrega da rota.</summary>
    [HttpDelete("{id:long}/entregas/{entregaId:long}")]
    public async Task<ActionResult<RotaDto>> Remover(long id, long entregaId, CancellationToken ct)
        => Ok(await _service.RemoverEntregaAsync(id, entregaId, ct));

    /// <summary>Reordena as paradas da rota.</summary>
    [HttpPut("{id:long}/ordem")]
    public async Task<ActionResult<RotaDto>> Reordenar(long id, ReordenarRotaRequest request, CancellationToken ct)
        => Ok(await _service.ReordenarAsync(id, request.EntregaIds, ct));

    /// <summary>Muda o status da rota (Despachada move as entregas desta rota para "Saiu para entrega").</summary>
    [HttpPut("{id:long}/status")]
    public async Task<ActionResult<RotaDto>> MudarStatus(long id, MudarStatusRotaRequest request, CancellationToken ct)
        => Ok(await _service.MudarStatusAsync(id, request.Status, Usuario, ct));
}
