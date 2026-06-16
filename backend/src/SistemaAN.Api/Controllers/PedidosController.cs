using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Estoque;
using SistemaAN.Application.Pedidos;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/pedidos")]
public sealed class PedidosController : ControllerBase
{
    private readonly IPedidoService _service;
    private readonly IProdutoAcabadoService _produtoAcabado;

    public PedidosController(IPedidoService service, IProdutoAcabadoService produtoAcabado)
    {
        _service = service;
        _produtoAcabado = produtoAcabado;
    }

    private string Usuario => User.Identity?.Name ?? "sistema";

    /// <summary>Situação de estoque (físico/comprometido/disponível/falta) dos itens do pedido.</summary>
    [HttpGet("{id:long}/estoque")]
    public async Task<ActionResult<IReadOnlyList<SituacaoEstoqueItemDto>>> Estoque(long id, CancellationToken ct)
        => Ok(await _produtoAcabado.SituacaoPedidoAsync(id, ct));

    /// <summary>Lista os pedidos de um cliente PJ.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PedidoResumoDto>>> Listar([FromQuery] long clienteId, CancellationToken ct)
        => Ok(await _service.ListarPorClienteAsync(clienteId, ct));

    /// <summary>Obtém um pedido (com itens e status da entrega).</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PedidoDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um pedido (rascunho).</summary>
    [HttpPost]
    public async Task<ActionResult<PedidoDto>> Criar(SalvarPedidoRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um pedido (rascunho livre; confirmado só com entrega Programada).</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PedidoDto>> Atualizar(long id, SalvarPedidoRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Confirma o pedido e gera a Entrega vinculada.</summary>
    [HttpPost("{id:long}/confirmar")]
    public async Task<ActionResult<PedidoDto>> Confirmar(long id, CancellationToken ct)
        => Ok(await _service.ConfirmarAsync(id, Usuario, ct));

    /// <summary>Cancela o pedido (e a entrega vinculada, se houver).</summary>
    [HttpPost("{id:long}/cancelar")]
    public async Task<ActionResult<PedidoDto>> Cancelar(long id, CancelarPedidoRequest request, CancellationToken ct)
        => Ok(await _service.CancelarAsync(id, request.Motivo, Usuario, ct));
}
