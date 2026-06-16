using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaAN.Application.Clientes;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Entregas;
using SistemaAN.Application.Estoque;
using SistemaAN.Application.Pedidos;
using SistemaAN.Application.Pets;
using SistemaAN.Application.Planos;
using SistemaAN.Application.Producao;
using SistemaAN.Application.Receitas;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>
/// Popula o banco com massa de dados de DEMONSTRAÇÃO/TESTE (clientes, pets,
/// planos, receitas, estoque, pedidos PJ, entregas e uma produção).
/// Roda apenas quando <c>Seed:DemoData = true</c> (ligado só no stack de teste).
/// Idempotente: não faz nada se já existirem clientes. Resiliente: blocos opcionais
/// são protegidos para que a API sempre suba mesmo se algo variar.
/// </summary>
public sealed class DemoDataSeeder
{
    private const string Usuario = "demo";
    private const int QtdClientesPf = 30;
    private const int QtdClientesPj = 8;

    private readonly IApplicationDbContext _db;
    private readonly ILogger<DemoDataSeeder> _logger;
    private readonly IReceitaCasaService _receitas;
    private readonly IItemEstoqueService _itens;
    private readonly IEstoqueMovimentacaoService _estoque;
    private readonly IClienteService _clientes;
    private readonly IPetService _pets;
    private readonly IPlanoAlimentarService _planos;
    private readonly IClientePjService _clientesPj;
    private readonly IPedidoService _pedidos;
    private readonly IEntregaService _entregas;
    private readonly IProducaoService _producao;

    public DemoDataSeeder(
        IApplicationDbContext db,
        ILogger<DemoDataSeeder> logger,
        IReceitaCasaService receitas,
        IItemEstoqueService itens,
        IEstoqueMovimentacaoService estoque,
        IClienteService clientes,
        IPetService pets,
        IPlanoAlimentarService planos,
        IClientePjService clientesPj,
        IPedidoService pedidos,
        IEntregaService entregas,
        IProducaoService producao)
    {
        _db = db;
        _logger = logger;
        _receitas = receitas;
        _itens = itens;
        _estoque = estoque;
        _clientes = clientes;
        _pets = pets;
        _planos = planos;
        _clientesPj = clientesPj;
        _pedidos = pedidos;
        _entregas = entregas;
        _producao = producao;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Clientes.AnyAsync(ct))
        {
            return; // já populado
        }

        _logger.LogWarning("DemoDataSeeder: populando banco de TESTE com massa de dados...");
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        // ----- Pré-requisitos (catálogo já semeado) -----
        var ingredientes = await _db.Ingredientes.Where(i => i.Ativo).OrderBy(i => i.Id).Select(i => i.Id).ToListAsync(ct);
        var tamanhos = await _db.TamanhosPacote.OrderBy(t => t.PesoGramas).ToListAsync(ct);
        var freq = await _db.FrequenciasEntrega.Where(f => !f.Personalizada && f.Ativo).OrderBy(f => f.DiasCiclo).FirstOrDefaultAsync(ct);

        if (ingredientes.Count < 3 || tamanhos.Count == 0 || freq is null)
        {
            _logger.LogWarning("DemoDataSeeder: catálogo insuficiente (ingredientes/tamanhos/frequência). Abortando demo.");
            return;
        }

        var t500 = tamanhos.FirstOrDefault(t => t.PesoGramas == 500) ?? tamanhos[^1];
        var t250 = tamanhos.FirstOrDefault(t => t.PesoGramas == 250) ?? tamanhos[0];

        // ----- 1) Receitas da Casa (soma exata 1000g) -----
        var nomesReceitas = new[] { "Frango & Arroz", "Carne & Batata", "Peixe & Legumes", "Mix Completo" };
        var receitaIds = new List<long>();
        for (var i = 0; i < nomesReceitas.Length; i++)
        {
            try
            {
                var req = new SalvarReceitaCasaRequest(
                    $"CASA-{i + 1:000}", nomesReceitas[i], true, "Receita de demonstração.",
                    new[]
                    {
                        new SalvarItemReceitaRequest(ingredientes[0], 500),
                        new SalvarItemReceitaRequest(ingredientes[1], 300),
                        new SalvarItemReceitaRequest(ingredientes[2], 200),
                    });
                var rec = await _receitas.CriarAsync(req, ct);
                receitaIds.Add(rec.Id);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: receita {I} falhou.", i); }
        }

        if (receitaIds.Count == 0)
        {
            _logger.LogWarning("DemoDataSeeder: nenhuma receita criada — abortando demo (sem quebrar a API).");
            return;
        }

        // ----- 2) Itens de estoque: insumos (com entrada) + produto acabado -----
        foreach (var ingId in ingredientes)
        {
            try
            {
                var item = await _itens.CriarInsumoAsync(new CriarItemInsumoRequest(
                    $"Insumo {ingId}", "Outros", "Kg", ingId, 5m, null, null, true, null, true), ct);
                await _estoque.RegistrarEntradaAsync(new RegistrarEntradaRequest(
                    item.Id, 50m, 20m, null, null, hoje, hoje, null, null, null, "Entrada demo"), Usuario, ct);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: insumo do ingrediente {Id} falhou.", ingId); }
        }

        foreach (var recId in receitaIds)
        {
            foreach (var tam in new[] { t250, t500 })
            {
                try
                {
                    await _itens.CriarProdutoAcabadoAsync(new CriarItemProdutoAcabadoRequest(
                        $"Produto {recId}-{tam.PesoGramas}", recId, tam.Id, 20m, null, true, null, true), ct);
                }
                catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: produto acabado {Rec}/{Tam} falhou.", recId, tam.Id); }
            }
        }

        // ----- 3) Clientes PF + pets + planos -----
        var prefs = new[] { "HorarioComercial", "Manha", "Tarde" };
        var bairros = new[] { "Centro", "Trindade", "Lagoa", "Campeche", "Santa Mônica", "Itacorubi" };
        for (var i = 1; i <= QtdClientesPf; i++)
        {
            try
            {
                var cliente = await _clientes.CriarAsync(new SalvarClienteRequest(
                    $"Cliente PF {i}", null, $"(48) 9{i:0000}-0000", null, "Demo", null,
                    $"Rua {i}", $"{100 + i}", null, "88000-000", bairros[i % bairros.Length], "Florianópolis", "SC",
                    freq.Id, hoje.AddDays(i % 7), "Assinante", "Pix", 5, 150m + i, "EmDia", null, prefs[i % prefs.Length]), ct);

                var qtdPets = (i % 2) + 1;
                for (var p = 1; p <= qtdPets; p++)
                {
                    var pet = await _pets.CriarAsync(cliente.Id, new SalvarPetRequest(
                        $"Pet {i}-{p}", null, 8m + (i % 20), null, "2 anos", p % 2 == 0 ? "Femea" : "Macho", null, null, null, null), ct);

                    var recId = receitaIds[i % receitaIds.Count];
                    await _planos.SalvarAsync(pet.Id, new SalvarPlanoRequest(
                        300, null, "Casa",
                        new[]
                        {
                            new SalvarPlanoItemRequest(recId, 3500, null,
                                new[] { new SalvarPlanoItemPacoteRequest(t500.Id, 7) }),
                        }), ct);
                }
            }
            catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: cliente PF {I} falhou.", i); }
        }

        // ----- 4) Clientes PJ + pedidos (confirmados → geram entrega) -----
        var tiposPj = new[] { "PetShop", "Mercado", "ClinicaVeterinaria", "Revendedor" };
        for (var j = 1; j <= QtdClientesPj; j++)
        {
            try
            {
                var cnpj = $"{10000000000000L + j}";
                await _clientesPj.CriarAsync(new SalvarClientePjRequest(
                    $"Empresa {j} LTDA", $"PetShop {j}", cnpj, null, $"(48) 3{j:000}-0000", null, null, "Contato", "Comprador",
                    $"Av. Comercial {j}", $"{j}0", null, "Centro", "Florianópolis", "SC", "88010-000",
                    $"Av. Comercial {j}", $"{j}0", null, "Centro", "Florianópolis", "SC", "88010-000",
                    tiposPj[j % tiposPj.Length], null, "30 dias", null, null, null, null, "HorarioComercial"), ct);

                var clienteId = await _db.ClientesPj.Where(p => p.Cnpj == cnpj).Select(p => p.ClienteId).FirstAsync(ct);
                var recId = receitaIds[j % receitaIds.Count];
                var pedido = await _pedidos.CriarAsync(new SalvarPedidoRequest(
                    clienteId, hoje, hoje.AddDays(3 + (j % 5)), "Pedido de demonstração.",
                    new[] { new SalvarPedidoItemRequest(recId, t500.Id, 10, null) }), ct);
                await _pedidos.ConfirmarAsync(pedido.Id, Usuario, ct);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: cliente PJ {J} falhou.", j); }
        }

        // ----- 5) Gerar entregas recorrentes (PF) -----
        try
        {
            var r = await _entregas.GerarAsync(45, Usuario, ct);
            _logger.LogWarning("DemoDataSeeder: {Geradas} entregas geradas para {Clientes} clientes.", r.Geradas, r.Clientes);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: geração de entregas falhou."); }

        // ----- 6) Uma produção do dia (planejada) -----
        try
        {
            var ordem = await _producao.CriarOuObterOrdemAsync(hoje, ct);
            await _producao.AdicionarFichasAsync(ordem.Id,
                new AdicionarFichasRequest(
                    Array.Empty<long>(),
                    new[] { new FichaCasaRequest(receitaIds[0], t500.Id, 10) }), ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: produção demo falhou."); }

        _logger.LogWarning("DemoDataSeeder: concluído.");
    }
}
