using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Estoque;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/estoque/itens")]
public sealed class ItensEstoqueController : ControllerBase
{
    private readonly IItemEstoqueService _service;

    public ItensEstoqueController(IItemEstoqueService service) => _service = service;

    /// <summary>Lista itens de estoque (opcionalmente apenas os abaixo do mínimo).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ItemEstoqueDto>>> Listar([FromQuery] bool abaixoMinimo, CancellationToken ct)
        => Ok(await _service.ListarAsync(abaixoMinimo, ct));

    /// <summary>Pacotes de Receita Personalizada prontos (reservados) e ainda não entregues.</summary>
    [HttpGet("personalizadas-prontas")]
    public async Task<ActionResult<IReadOnlyList<PersonalizadaProntaDto>>> PersonalizadasProntas(CancellationToken ct)
        => Ok(await _service.ListarPersonalizadasProntasAsync(ct));

    /// <summary>Obtém um item de estoque.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ItemEstoqueDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um item de estoque do tipo Insumo.</summary>
    [HttpPost("insumo")]
    public async Task<ActionResult<ItemEstoqueDto>> CriarInsumo(CriarItemInsumoRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarInsumoAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Cria um item de Produto Acabado da Receita da Casa.</summary>
    [HttpPost("produto-acabado")]
    public async Task<ActionResult<ItemEstoqueDto>> CriarProdutoAcabado(CriarItemProdutoAcabadoRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarProdutoAcabadoAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza os dados editáveis de um item de estoque.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ItemEstoqueDto>> Atualizar(long id, AtualizarItemEstoqueRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa um item de estoque.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }
}
