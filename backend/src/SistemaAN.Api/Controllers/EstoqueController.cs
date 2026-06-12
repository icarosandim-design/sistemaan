using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Estoque;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/estoque")]
public sealed class EstoqueController : ControllerBase
{
    private readonly IEstoqueMovimentacaoService _service;

    public EstoqueController(IEstoqueMovimentacaoService service) => _service = service;

    private string Usuario => User.Identity?.Name ?? "sistema";

    /// <summary>Registra uma entrada de estoque (compra) gerando lote e movimentação.</summary>
    [HttpPost("entradas")]
    public async Task<ActionResult<ItemEstoqueDto>> RegistrarEntrada(RegistrarEntradaRequest request, CancellationToken ct)
        => Ok(await _service.RegistrarEntradaAsync(request, Usuario, ct));

    /// <summary>Registra uma saída de estoque (consumo/perda/descarte/etc.) por FIFO.</summary>
    [HttpPost("saidas")]
    public async Task<ActionResult<ItemEstoqueDto>> RegistrarSaida(RegistrarSaidaRequest request, CancellationToken ct)
        => Ok(await _service.RegistrarSaidaAsync(request, Usuario, ct));

    /// <summary>Registra um ajuste manual de estoque (com motivo obrigatório).</summary>
    [HttpPost("ajustes")]
    public async Task<ActionResult<ItemEstoqueDto>> RegistrarAjuste(RegistrarAjusteRequest request, CancellationToken ct)
        => Ok(await _service.RegistrarAjusteAsync(request, Usuario, ct));

    /// <summary>Lista os lotes de um item de estoque.</summary>
    [HttpGet("itens/{id:long}/lotes")]
    public async Task<ActionResult<IReadOnlyList<LoteEstoqueDto>>> Lotes(long id, CancellationToken ct)
        => Ok(await _service.ListarLotesAsync(id, ct));

    /// <summary>Lista as movimentações de um item de estoque.</summary>
    [HttpGet("itens/{id:long}/movimentacoes")]
    public async Task<ActionResult<IReadOnlyList<MovimentacaoEstoqueDto>>> Movimentacoes(long id, CancellationToken ct)
        => Ok(await _service.ListarMovimentacoesAsync(id, ct));
}
