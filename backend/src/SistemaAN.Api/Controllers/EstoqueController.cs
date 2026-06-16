using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Estoque;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/estoque")]
public sealed class EstoqueController : ControllerBase
{
    private readonly IEstoqueMovimentacaoService _service;

    public EstoqueController(IEstoqueMovimentacaoService service) => _service = service;

    private string Usuario => User.Identity?.Name ?? "sistema";

    /// <summary>Registra uma entrada de estoque (compra de 1 item) gerando lote e movimentação.</summary>
    [HttpPost("entradas")]
    public async Task<ActionResult<ItemEstoqueDto>> RegistrarEntrada(RegistrarEntradaRequest request, CancellationToken ct)
        => Ok(await _service.RegistrarEntradaAsync(request, Usuario, ct));

    /// <summary>Registra uma compra com vários itens (uma nota) — gera uma entrada por item.</summary>
    [HttpPost("compras")]
    public async Task<ActionResult<CompraResultadoDto>> RegistrarCompra(RegistrarCompraRequest request, CancellationToken ct)
        => Ok(await _service.RegistrarCompraAsync(request, Usuario, ct));

    /// <summary>Registra uma saída de estoque (consumo/perda/descarte/etc.) por FIFO.</summary>
    [HttpPost("saidas")]
    public async Task<ActionResult<ItemEstoqueDto>> RegistrarSaida(RegistrarSaidaRequest request, CancellationToken ct)
        => Ok(await _service.RegistrarSaidaAsync(request, Usuario, ct));

    /// <summary>Registra um ajuste manual de estoque (com motivo obrigatório).</summary>
    [HttpPost("ajustes")]
    public async Task<ActionResult<ItemEstoqueDto>> RegistrarAjuste(RegistrarAjusteRequest request, CancellationToken ct)
        => Ok(await _service.RegistrarAjusteAsync(request, Usuario, ct));

    /// <summary>Livro-razão geral do estoque (consulta paginada e filtrada).</summary>
    [HttpGet("movimentacoes")]
    public async Task<ActionResult<MovimentacaoPaginaDto>> Movimentacoes(
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        [FromQuery] long? itemEstoqueId,
        [FromQuery] string? categoria,
        [FromQuery] string? tipo,
        [FromQuery] long? fornecedorId,
        [FromQuery] long? loteEstoqueId,
        [FromQuery] string? usuario,
        [FromQuery] string? motivo,
        [FromQuery] string? origem,
        [FromQuery] int pagina,
        [FromQuery] int tamanhoPagina,
        CancellationToken ct)
        => Ok(await _service.ListarMovimentacoesGeralAsync(
            new FiltroMovimentacoesRequest(dataInicio, dataFim, itemEstoqueId, categoria, tipo, fornecedorId,
                loteEstoqueId, usuario, motivo, origem, pagina <= 0 ? 1 : pagina, tamanhoPagina <= 0 ? 50 : tamanhoPagina), ct));

    /// <summary>Histórico de compras/entradas de estoque (consulta filtrada).</summary>
    [HttpGet("entradas")]
    public async Task<ActionResult<IReadOnlyList<EntradaCompraDto>>> Entradas(
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        [FromQuery] long? fornecedorId,
        [FromQuery] long? itemEstoqueId,
        [FromQuery] string? categoria,
        [FromQuery] long? loteEstoqueId,
        [FromQuery] string? usuario,
        [FromQuery] decimal? valorMin,
        [FromQuery] decimal? valorMax,
        [FromQuery] bool? comFrete,
        CancellationToken ct)
        => Ok(await _service.ListarEntradasAsync(
            new FiltroEntradasRequest(dataInicio, dataFim, fornecedorId, itemEstoqueId, categoria, loteEstoqueId,
                usuario, valorMin, valorMax, comFrete), ct));

    /// <summary>Lista os lotes de um item de estoque.</summary>
    [HttpGet("itens/{id:long}/lotes")]
    public async Task<ActionResult<IReadOnlyList<LoteEstoqueDto>>> Lotes(long id, CancellationToken ct)
        => Ok(await _service.ListarLotesAsync(id, ct));

    /// <summary>Lista as movimentações de um item de estoque.</summary>
    [HttpGet("itens/{id:long}/movimentacoes")]
    public async Task<ActionResult<IReadOnlyList<MovimentacaoEstoqueDto>>> Movimentacoes(long id, CancellationToken ct)
        => Ok(await _service.ListarMovimentacoesAsync(id, ct));
}
