using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaAN.Application.Producao;

namespace SistemaAN.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador,Operador,Cozinha")]
[Route("api/producao")]
public sealed class ProducaoController : ControllerBase
{
    // Papéis por tipo de ação:
    private const string Planejamento = "Administrador,Operador";    // planejar produção / análise
    private const string ExecucaoCozinha = "Administrador,Cozinha";  // execução operacional da cozinha

    private readonly IProducaoService _service;

    public ProducaoController(IProducaoService service) => _service = service;

    private string Usuario => User.Identity?.Name ?? "sistema";

    /// <summary>Demanda de produção (personalizadas não prontas + Casa com falta) no período.</summary>
    [HttpGet("demanda")]
    [Authorize(Roles = Planejamento)]
    public async Task<ActionResult<DemandaDto>> Demanda([FromQuery] DateOnly inicio, [FromQuery] DateOnly fim, CancellationToken ct)
        => Ok(await _service.ObterDemandaAsync(inicio, fim, ct));

    /// <summary>Lista as ordens de produção (resumo). Leitura usada também pela tela do dia/Cozinha.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrdemProducaoResumoDto>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarOrdensAsync(ct));

    /// <summary>Obtém a ordem de produção de um dia (ou 404). Leitura (inclui Cozinha).</summary>
    [HttpGet("dia/{data}")]
    public async Task<ActionResult<OrdemProducaoDto>> PorData(DateOnly data, CancellationToken ct)
    {
        var ordem = await _service.ObterPorDataAsync(data, ct);
        return ordem is null ? NotFound() : Ok(ordem);
    }

    /// <summary>Obtém uma ordem de produção (fichas + consolidado). Leitura (inclui Cozinha).</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrdemProducaoDto>> Obter(long id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cria (ou abre) a ordem de produção de um dia.</summary>
    [HttpPost]
    [Authorize(Roles = Planejamento)]
    public async Task<ActionResult<OrdemProducaoDto>> Criar(CriarOrdemRequest request, CancellationToken ct)
        => Ok(await _service.CriarOuObterOrdemAsync(request.Data, ct));

    /// <summary>Adiciona fichas (personalizadas e/ou Casa) à ordem.</summary>
    [HttpPost("{id:long}/fichas")]
    [Authorize(Roles = Planejamento)]
    public async Task<ActionResult<OrdemProducaoDto>> AdicionarFichas(long id, AdicionarFichasRequest request, CancellationToken ct)
        => Ok(await _service.AdicionarFichasAsync(id, request, ct));

    /// <summary>Remove uma ficha da ordem.</summary>
    [HttpDelete("fichas/{fichaId:long}")]
    [Authorize(Roles = Planejamento)]
    public async Task<ActionResult<OrdemProducaoDto>> RemoverFicha(long fichaId, CancellationToken ct)
        => Ok(await _service.RemoverFichaAsync(fichaId, ct));

    /// <summary>Avança/define o status de uma ficha (fila da cozinha).</summary>
    [HttpPut("fichas/{fichaId:long}/status")]
    [Authorize(Roles = ExecucaoCozinha)]
    public async Task<ActionResult<OrdemProducaoDto>> MudarStatusFicha(long fichaId, MudarStatusFichaRequest request, CancellationToken ct)
        => Ok(await _service.MudarStatusFichaAsync(fichaId, request.Status, ct));

    /// <summary>Conclui uma ficha (pacotes reais + envasado).</summary>
    [HttpPut("fichas/{fichaId:long}/concluir")]
    [Authorize(Roles = ExecucaoCozinha)]
    public async Task<ActionResult<OrdemProducaoDto>> ConcluirFicha(long fichaId, ConcluirFichaRequest request, CancellationToken ct)
        => Ok(await _service.ConcluirFichaAsync(fichaId, request, Usuario, ct));

    /// <summary>Marca uma ficha como não feita (com motivo).</summary>
    [HttpPut("fichas/{fichaId:long}/nao-feita")]
    [Authorize(Roles = ExecucaoCozinha)]
    public async Task<ActionResult<OrdemProducaoDto>> NaoFeita(long fichaId, MarcarNaoFeitaRequest request, CancellationToken ct)
        => Ok(await _service.MarcarNaoFeitaAsync(fichaId, request.Motivo, Usuario, ct));

    /// <summary>Registra a pesagem real (cru/cozido) por ingrediente.</summary>
    [HttpPut("{id:long}/consumo")]
    [Authorize(Roles = ExecucaoCozinha)]
    public async Task<ActionResult<OrdemProducaoDto>> RegistrarConsumo(long id, RegistrarConsumoRequest request, CancellationToken ct)
        => Ok(await _service.RegistrarConsumoAsync(id, request, ct));

    /// <summary>Finaliza a ordem: baixa de insumos, produto acabado e prontidão.</summary>
    [HttpPost("{id:long}/finalizar")]
    [Authorize(Roles = ExecucaoCozinha)]
    public async Task<ActionResult<FinalizacaoResultadoDto>> Finalizar(long id, FinalizarProducaoRequest request, CancellationToken ct)
        => Ok(await _service.FinalizarAsync(id, request, Usuario, ct));

    /// <summary>Análise de rendimentos e perdas (planejado × real) no período.</summary>
    [HttpGet("rendimentos")]
    [Authorize(Roles = Planejamento)]
    public async Task<ActionResult<RendimentoDto>> Rendimentos([FromQuery] DateOnly inicio, [FromQuery] DateOnly fim, CancellationToken ct)
        => Ok(await _service.ObterRendimentosAsync(inicio, fim, ct));
}
