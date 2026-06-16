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
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>
/// Popula o banco com massa de dados de DEMONSTRAÇÃO/TESTE (clientes PF e PJ, pets
/// com receitas Casa e Personalizadas, raças/doenças variadas, estoque com evolução
/// de custo, ~3 meses de entregas concluídas, cancelamentos e uma produção).
/// Roda apenas quando <c>Seed:DemoData = true</c> (ligado só no stack de teste).
/// Idempotente: não faz nada se já existirem clientes. Resiliente: blocos opcionais
/// são protegidos para que a API sempre suba mesmo se algo variar.
/// </summary>
public sealed class DemoDataSeeder
{
    private const string Usuario = "demo";
    private const int QtdClientesPf = 80;
    private const int QtdClientesPj = 20;
    private const int DiasHistorico = 95; // ~3 meses de registros

    private readonly IApplicationDbContext _db;
    private readonly ILogger<DemoDataSeeder> _logger;
    private readonly IReceitaCasaService _receitas;
    private readonly IReceitaPersonalizadaService _receitasPers;
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
        IReceitaPersonalizadaService receitasPers,
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
        _receitasPers = receitasPers;
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

        // Cadastros auxiliares já semeados (RacasDataSeeder/DoencasDataSeeder rodam antes).
        var racaIds = await _db.Racas.OrderBy(r => r.Ordem).Select(r => r.Id).ToListAsync(ct);
        var doencaIds = await _db.Doencas.OrderBy(d => d.Ordem).Select(d => d.Id).ToListAsync(ct);
        var motivoIds = await _db.MotivosCancelamento.OrderBy(m => m.Ordem).Select(m => m.Id).ToListAsync(ct);

        // ----- 1) Receitas da Casa (soma exata 1000g, ingredientes variados) -----
        var nomesReceitas = new[] { "Frango & Arroz", "Carne & Batata", "Peixe & Legumes", "Mix Completo", "Frango & Abóbora", "Carne & Cenoura" };
        var receitaIds = new List<long>();
        for (var i = 0; i < nomesReceitas.Length; i++)
        {
            try
            {
                var a = ingredientes[i % ingredientes.Count];
                var b = ingredientes[(i + 1) % ingredientes.Count];
                var c = ingredientes[(i + 2) % ingredientes.Count];
                var req = new SalvarReceitaCasaRequest(
                    $"CASA-{i + 1:000}", nomesReceitas[i], true, "Receita de demonstração.",
                    new[]
                    {
                        new SalvarItemReceitaRequest(a, 500),
                        new SalvarItemReceitaRequest(b, 300),
                        new SalvarItemReceitaRequest(c, 200),
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

        // ----- 2) Itens de estoque: insumos com VÁRIAS entradas (evolução de custo) -----
        foreach (var ingId in ingredientes)
        {
            try
            {
                var item = await _itens.CriarInsumoAsync(new CriarItemInsumoRequest(
                    $"Insumo {ingId}", "Outros", "Kg", ingId, 5m, null, null, true, null, true), ct);

                // Entradas ao longo de ~3 meses, com preço crescente (pressão de custo).
                var precoBase = 16m + (ingId % 6);
                var dias = new[] { DiasHistorico, 60, 30, 7 };
                for (var k = 0; k < dias.Length; k++)
                {
                    var data = hoje.AddDays(-dias[k]);
                    var preco = precoBase + k * 1.5m;
                    await _estoque.RegistrarEntradaAsync(new RegistrarEntradaRequest(
                        item.Id, 40m, preco, null, null, data, data, null, null, null, "Entrada demo"), Usuario, ct);
                }
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

        // ----- 3) Clientes PF + pets (raça/doenças) + planos (Casa e Personalizada) -----
        var prefs = new[] { "HorarioComercial", "Manha", "Tarde" };
        var bairros = new[] { "Centro", "Trindade", "Lagoa", "Campeche", "Santa Mônica", "Itacorubi", "Ingleses", "Canasvieiras", "Coqueiros", "Estreito" };
        for (var i = 1; i <= QtdClientesPf; i++)
        {
            try
            {
                var cliente = await _clientes.CriarAsync(new SalvarClienteRequest(
                    $"Cliente PF {i}", null, $"(48) 9{i:0000}-0000", null, "Demo", null,
                    $"Rua {i}", $"{100 + i}", null, "88000-000", bairros[i % bairros.Length], "Florianópolis", "SC",
                    freq.Id, hoje.AddDays(i % 7), "Assinante", "Pix", 5, 120m + (i % 12) * 10m, "EmDia", null, prefs[i % prefs.Length]), ct);

                var qtdPets = (i % 2) + 1;
                for (var p = 1; p <= qtdPets; p++)
                {
                    var racaId = racaIds.Count > 0 ? (long?)racaIds[(i + p) % racaIds.Count] : null;
                    var doencas = new List<long>();
                    if (doencaIds.Count > 0 && i % 3 == 0) doencas.Add(doencaIds[i % doencaIds.Count]);
                    if (doencaIds.Count > 1 && i % 6 == 0) doencas.Add(doencaIds[(i + 1) % doencaIds.Count]);

                    var pet = await _pets.CriarAsync(cliente.Id, new SalvarPetRequest(
                        $"Pet {i}-{p}", racaId, 6m + (i % 22), null, "2 anos", p % 2 == 0 ? "Femea" : "Macho", null, null, null, doencas), ct);

                    // ~25% dos pets com receita PERSONALIZADA; o restante com receita da Casa.
                    if (i % 4 == 0)
                    {
                        try
                        {
                            var recPers = await _receitasPers.CriarAsync(pet.Id, new SalvarReceitaPersonalizadaRequest(
                                $"PERS-{pet.Id}", $"Personalizada {pet.Nome}", true, "Receita personalizada de demonstração.",
                                new[]
                                {
                                    new SalvarItemReceitaRequest(ingredientes[i % ingredientes.Count], 250),
                                    new SalvarItemReceitaRequest(ingredientes[(i + 1) % ingredientes.Count], 150),
                                    new SalvarItemReceitaRequest(ingredientes[(i + 2) % ingredientes.Count], 100),
                                }), ct);
                            await _planos.SalvarAsync(pet.Id, new SalvarPlanoRequest(
                                250, null, "Personalizada",
                                new[] { new SalvarPlanoItemRequest(recPers.Id, null, 7, null) }), ct);
                        }
                        catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: plano personalizado do pet {Pet} falhou.", pet.Id); }
                    }
                    else
                    {
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
            }
            catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: cliente PF {I} falhou.", i); }
        }

        // ----- 4) Clientes PJ + pedidos (confirmados → geram entrega) -----
        var tiposPj = new[] { "PetShop", "Mercado", "ClinicaVeterinaria", "Revendedor", "Parceiro" };
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
                    clienteId, hoje, hoje.AddDays(2 + (j % 5)), "Pedido de demonstração.",
                    new[]
                    {
                        new SalvarPedidoItemRequest(recId, t500.Id, 10 + (j % 10), null),
                        new SalvarPedidoItemRequest(receitaIds[(j + 1) % receitaIds.Count], t250.Id, 8 + (j % 6), null),
                    }), ct);
                await _pedidos.ConfirmarAsync(pedido.Id, Usuario, ct);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: cliente PJ {J} falhou.", j); }
        }

        // ----- 5) Gerar entregas recorrentes futuras (PF) -----
        try
        {
            var r = await _entregas.GerarAsync(60, Usuario, ct);
            _logger.LogWarning("DemoDataSeeder: {Geradas} entregas futuras geradas para {Clientes} clientes.", r.Geradas, r.Clientes);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: geração de entregas falhou."); }

        // ----- 6) Histórico de ~3 meses: clona entregas futuras para o passado como Entregue -----
        try
        {
            await GerarHistoricoEntregasAsync(hoje, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: histórico de entregas falhou."); }

        // ----- 7) Cancelamentos com data passada + motivo (relatório de cancelamentos) -----
        try
        {
            await GerarCancelamentosAsync(hoje, motivoIds, ct);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "DemoDataSeeder: cancelamentos demo falharam."); }

        // ----- 8) Uma produção do dia (planejada) -----
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

    /// <summary>Clona as entregas programadas para datas passadas (status Entregue), cobrindo ~3 meses.</summary>
    private async Task GerarHistoricoEntregasAsync(DateOnly hoje, CancellationToken ct)
    {
        var programadas = await _db.Entregas
            .Where(e => e.Status == EntregaStatus.Programada && e.DataPrevista >= hoje && e.PedidoId == null)
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Ingredientes)
            .ToListAsync(ct);

        // Um template por cliente (a entrega futura mais próxima).
        var templates = programadas
            .GroupBy(e => e.ClienteId)
            .Select(g => g.OrderBy(e => e.DataPrevista).First())
            .ToList();

        var criadas = 0;
        var limite = hoje.AddDays(-DiasHistorico);
        foreach (var t in templates)
        {
            var ciclo = t.DiasCiclo > 0 ? t.DiasCiclo : 7;
            for (var k = 1; k <= 20; k++)
            {
                var data = hoje.AddDays(-ciclo * k);
                if (data < limite)
                {
                    break;
                }

                var nova = Entrega.Criar(t.ClienteId, data, SnapshotDe(t));
                foreach (var pet in ClonarPets(t))
                {
                    nova.AdicionarPet(pet);
                }
                nova.RegistrarHistorico(Usuario, "Entrega histórica (demo)", null, EntregaStatus.Programada);
                nova.MudarStatus(EntregaStatus.Entregue, Usuario, "Entregue (demo)");
                _db.Entregas.Add(nova);
                criadas++;
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogWarning("DemoDataSeeder: {N} entregas históricas (Entregue) criadas.", criadas);
    }

    private async Task GerarCancelamentosAsync(DateOnly hoje, IReadOnlyList<long> motivoIds, CancellationToken ct)
    {
        if (motivoIds.Count == 0)
        {
            return;
        }

        // Cancela alguns clientes PF ao longo dos últimos ~3 meses.
        var alvos = await _db.Clientes
            .Where(c => c.Ativo)
            .OrderBy(c => c.Id)
            .Take(12)
            .ToListAsync(ct);

        var i = 0;
        foreach (var cliente in alvos)
        {
            var quando = new DateTimeOffset(hoje.AddDays(-(7 + i * 7)).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var motivoId = motivoIds[i % motivoIds.Count];
            cliente.Cancelar("Cancelamento de demonstração", quando, motivoId, "Observação demo", null);
            i++;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogWarning("DemoDataSeeder: {N} cancelamentos demo registrados.", i);
    }

    private static DadosSnapshotEntrega SnapshotDe(Entrega e)
        => new(e.ClienteNome, e.Telefone, e.Rua, e.Numero, e.Complemento, e.Cep, e.Bairro, e.Cidade, e.Estado,
            e.FrequenciaNome, e.DiasCiclo, e.PreferenciaHorario);

    private static List<EntregaPet> ClonarPets(Entrega original)
    {
        var lista = new List<EntregaPet>();
        foreach (var op in original.Pets)
        {
            var itens = new List<EntregaItem>();
            foreach (var oi in op.Itens)
            {
                if (oi.Tipo == TipoReceita.Casa)
                {
                    var pacotes = oi.Pacotes.Select(p => EntregaItemPacote.Criar(p.TamanhoLabel, p.PesoGramas, p.Quantidade));
                    itens.Add(EntregaItem.CriarCasa(oi.ReceitaId, oi.ReceitaCodigo, oi.ReceitaNome, oi.QuantidadeCicloGramas ?? 0, pacotes));
                }
                else
                {
                    var ingr = oi.Ingredientes.Select(g =>
                        EntregaItemIngrediente.Criar(g.IngredienteId, g.IngredienteNome, g.Categoria, g.GramasCozidas, g.CustoKgCru, g.Coeficiente));
                    itens.Add(EntregaItem.CriarPersonalizada(oi.ReceitaId, oi.ReceitaCodigo, oi.ReceitaNome, oi.TamanhoPacoteGramas ?? 0, oi.QuantidadePacotes ?? 0, ingr));
                }
            }
            lista.Add(EntregaPet.Criar(op.PetId, op.PetNome, op.TipoAlimentacao, op.GramasDia, op.QuantidadeTotalGramas, itens));
        }
        return lista;
    }
}
