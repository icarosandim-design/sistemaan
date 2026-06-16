using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Produtos;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador")]
[Route("api/produtos")]
public sealed class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutosController(IProdutoService service) => _service = service;

    /// <summary>Lista produtos (filtros: tipo, receita, tamanho).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProdutoDto>>> Listar(
        [FromQuery] bool incluirInativos, [FromQuery] string? tipo, [FromQuery] long? receitaCasaId, [FromQuery] long? tamanhoPacoteId, CancellationToken ct)
        => Ok(await _service.ListarAsync(incluirInativos, tipo, receitaCasaId, tamanhoPacoteId, ct));

    /// <summary>Obtém um produto.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProdutoDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria um produto.</summary>
    [HttpPost]
    public async Task<ActionResult<ProdutoDto>> Criar(SalvarProdutoRequest request, CancellationToken ct)
    {
        var criado = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza um produto.</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProdutoDto>> Atualizar(long id, SalvarProdutoRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Inativa/reativa um produto.</summary>
    [HttpPut("{id:long}/status")]
    public async Task<IActionResult> AlternarStatus(long id, AlternarStatusProdutoRequest request, CancellationToken ct)
    {
        await _service.AlternarStatusAsync(id, request.Ativo, ct);
        return NoContent();
    }

    /// <summary>Gera/atualiza Produtos (ReceitaDaCasa) para os tamanhos informados.</summary>
    [HttpPost("gerar-receita/{receitaCasaId:long}")]
    public async Task<ActionResult<IReadOnlyList<ProdutoDto>>> GerarReceita(long receitaCasaId, GerarProdutosReceitaRequest request, CancellationToken ct)
        => Ok(await _service.GerarParaReceitaAsync(receitaCasaId, request, ct));
}

public sealed record AlternarStatusProdutoRequest(bool Ativo);
