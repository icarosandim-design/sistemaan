using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Producao;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Relatorios;

/// <summary>
/// Implementação dos relatórios gerenciais. As agregações são feitas no backend a
/// partir dos dados operacionais reais. Por causa do volume modesto (~150 clientes),
/// os recortes do período são carregados com projeções enxutas e consolidados em
/// memória — evitando N+1 e mantendo a tradução de consultas simples.
/// </summary>
public sealed class RelatorioService : IRelatorioService
{
    private readonly IApplicationDbContext _db;

    public RelatorioService(IApplicationDbContext db) => _db = db;

    private const string TipoAssinatura = "Assinatura PF";
    private const string TipoAvulsa = "Venda Avulsa PF";
    private const string TipoPj = "Pedido PJ";

    private static string Mes(DateOnly d) => $"{d.Year:0000}-{d.Month:00}";

    // ----- Modelos internos de carregamento -----
    private sealed record VendaInfo(
        DateOnly Data, string Tipo, string Cliente, long ClienteId, string? Pet, string? Raca,
        string Receitas, decimal Kg, string Status, string? Origem, string? Cidade, string? Bairro, string? Observacoes,
        decimal? Valor, decimal Custo);

    private sealed record ConsumoInfo(
        DateOnly Data, long IngredienteId, string Ingrediente, decimal Coeficiente,
        int PlanejadoCozido, int PlanejadoCru, decimal RealCru, decimal RealCozido,
        decimal Sobra, decimal Perda, decimal CustoKg, decimal? FatorCadastrado);

    private sealed record EntradaInfo(
        DateOnly Data, long IngredienteId, string Ingrediente, string? Fornecedor,
        decimal Quantidade, string Unidade, decimal ValorUnitario, decimal ValorTotal,
        decimal CustoMedioAtual);

    // =======================================================================
    // Carregadores compartilhados
    // =======================================================================
    private async Task<List<VendaInfo>> CarregarVendasAsync(DateOnly inicio, DateOnly fim, CancellationToken ct)
    {
        var entregas = await _db.Entregas
            .Where(e => e.DataPrevista >= inicio && e.DataPrevista <= fim
                && e.Status != EntregaStatus.Cancelada && e.Status != EntregaStatus.Reagendada)
            .Select(e => new
            {
                e.DataPrevista,
                e.Status,
                e.ClienteId,
                e.PedidoId,
                e.ClienteNome,
                e.Bairro,
                e.Cidade,
                e.ObservacoesInternas,
                Pets = e.Pets.Select(p => new
                {
                    p.PetId,
                    p.PetNome,
                    p.QuantidadeTotalGramas,
                    Itens = p.Itens.Select(i => new
                    {
                        i.Tipo,
                        i.ReceitaId,
                        i.ReceitaNome,
                        i.QuantidadeCicloGramas,
                        Pacotes = i.Pacotes.Select(pk => new { pk.PesoGramas, pk.Quantidade }).ToList(),
                        Ingredientes = i.Ingredientes.Select(ig => new { ig.GramasCozidas, ig.CustoKgCru, ig.Coeficiente }).ToList(),
                    }).ToList(),
                }).ToList(),
            })
            .ToListAsync(ct);

        var clientes = await _db.Clientes
            .Select(c => new { c.Id, c.TipoCliente, c.OrigemVenda, c.ValorRecorrenteMensal })
            .ToListAsync(ct);
        var mapaCli = clientes.ToDictionary(c => c.Id);

        var pedidoValor = await _db.Pedidos
            .Select(p => new { p.Id, p.ValorTotal })
            .ToListAsync(ct);
        var mapaPedido = pedidoValor.ToDictionary(p => p.Id, p => p.ValorTotal);

        var custoPorKgReceita = await CarregarCustoReceitasCasaAsync(ct);

        var petRacas = await _db.Pets.Select(p => new { p.Id, p.Raca }).ToListAsync(ct);
        var mapaRaca = petRacas.ToDictionary(p => p.Id, p => p.Raca);

        var lista = new List<VendaInfo>(entregas.Count);
        foreach (var e in entregas)
        {
            mapaCli.TryGetValue(e.ClienteId, out var cli);
            var tipo = e.PedidoId != null
                ? TipoPj
                : (cli is not null && cli.TipoCliente == TipoCliente.Assinante ? TipoAssinatura : TipoAvulsa);

            var kg = e.Pets.Sum(p => p.QuantidadeTotalGramas) / 1000m;
            var pets = string.Join(", ", e.Pets.Select(p => p.PetNome).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct());
            var racas = string.Join(", ", e.Pets
                .Where(p => p.PetId != null)
                .Select(p => mapaRaca.TryGetValue(p.PetId!.Value, out var rc) ? rc : null)
                .Where(rc => !string.IsNullOrWhiteSpace(rc))
                .Distinct());
            var receitas = string.Join(", ", e.Pets.SelectMany(p => p.Itens).Select(i => i.ReceitaNome).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct());

            // Custo somado das receitas de todos os cães da entrega (mesma fórmula da Receita da Casa).
            decimal custo = 0m;
            foreach (var p in e.Pets)
            {
                foreach (var it in p.Itens)
                {
                    if (it.Tipo == TipoReceita.Personalizada)
                    {
                        custo += it.Ingredientes.Sum(ig => ig.Coeficiente > 0
                            ? ig.GramasCozidas / 1000m * (ig.CustoKgCru / ig.Coeficiente)
                            : 0m);
                    }
                    else
                    {
                        var cozidoG = it.QuantidadeCicloGramas ?? it.Pacotes.Sum(pk => pk.PesoGramas * pk.Quantidade);
                        if (custoPorKgReceita.TryGetValue(it.ReceitaId, out var custoKg))
                        {
                            custo += cozidoG / 1000m * custoKg;
                        }
                    }
                }
            }

            // Valor da venda (onde há fonte real).
            decimal? valor = e.PedidoId is { } pid && mapaPedido.TryGetValue(pid, out var vt)
                ? vt
                : tipo == TipoAssinatura
                    ? (cli is { ValorRecorrenteMensal: > 0m } ? cli.ValorRecorrenteMensal : null)
                    : ParseValorAvulsa(e.ObservacoesInternas);

            lista.Add(new VendaInfo(
                e.DataPrevista, tipo, e.ClienteNome, e.ClienteId,
                string.IsNullOrWhiteSpace(pets) ? null : pets,
                string.IsNullOrWhiteSpace(racas) ? null : racas,
                receitas, kg, e.Status.ToString(), cli?.OrigemVenda, e.Cidade, e.Bairro, e.ObservacoesInternas,
                valor, Round(custo)));
        }

        return lista;
    }

    /// <summary>Custo por kg cozido de cada Receita da Casa (mesma fórmula da tela de Receitas).</summary>
    private async Task<Dictionary<long, decimal>> CarregarCustoReceitasCasaAsync(CancellationToken ct)
    {
        var receitas = await _db.Receitas
            .Where(r => r.Tipo == TipoReceita.Casa)
            .Select(r => new { r.Id, Itens = r.Itens.Select(it => new { it.IngredienteId, it.Gramas }).ToList() })
            .ToListAsync(ct);

        var ingredientes = await _db.Ingredientes
            .Select(i => new { i.Id, i.CustoAtualKg, i.CoeficienteConversao })
            .ToListAsync(ct);
        var ingMap = ingredientes.ToDictionary(i => i.Id);

        var mapa = new Dictionary<long, decimal>();
        foreach (var r in receitas)
        {
            decimal custo = 0m;
            foreach (var it in r.Itens)
            {
                if (ingMap.TryGetValue(it.IngredienteId, out var ing) && ing.CoeficienteConversao > 0)
                {
                    custo += it.Gramas / 1000m * (ing.CustoAtualKg / ing.CoeficienteConversao);
                }
            }

            var rendimento = r.Itens.Sum(it => it.Gramas);
            mapa[r.Id] = rendimento > 0 ? custo / (rendimento / 1000m) : custo;
        }

        return mapa;
    }

    private async Task<List<ConsumoInfo>> CarregarConsumosAsync(DateOnly inicio, DateOnly fim, CancellationToken ct)
    {
        var consumos = await (from c in _db.ConsumosProducao
                              join o in _db.OrdensProducao on c.OrdemProducaoId equals o.Id
                              where o.Data >= inicio && o.Data <= fim && o.Status == StatusOrdemProducao.Finalizada
                              select new
                              {
                                  o.Data,
                                  c.IngredienteId,
                                  c.IngredienteNome,
                                  c.Coeficiente,
                                  c.PlanejadoCozidoGramas,
                                  c.PlanejadoCruGramas,
                                  c.RealCruGramas,
                                  c.RealCozidoGramas,
                                  c.SobraGramas,
                                  c.PerdaGramas,
                              })
            .ToListAsync(ct);

        var custos = await _db.Ingredientes
            .Select(i => new { i.Id, i.CustoAtualKg, i.CoeficienteConversao })
            .ToListAsync(ct);
        var mapaCusto = custos.ToDictionary(i => i.Id);

        return consumos.Select(c =>
        {
            mapaCusto.TryGetValue(c.IngredienteId, out var ci);
            return new ConsumoInfo(
                c.Data, c.IngredienteId, c.IngredienteNome, c.Coeficiente,
                c.PlanejadoCozidoGramas, c.PlanejadoCruGramas,
                c.RealCruGramas ?? 0m, c.RealCozidoGramas ?? 0m,
                c.SobraGramas ?? 0m, c.PerdaGramas ?? 0m,
                ci?.CustoAtualKg ?? 0m, ci?.CoeficienteConversao);
        }).ToList();
    }

    private async Task<List<EntradaInfo>> CarregarEntradasAsync(DateOnly inicio, DateOnly fim, CancellationToken ct)
    {
        var entradas = await _db.EntradasEstoque
            .Where(e => e.DataCompra >= inicio && e.DataCompra <= fim)
            .Select(e => new { e.DataCompra, e.ItemEstoqueId, e.FornecedorId, e.Quantidade, e.UnidadeMedida, e.ValorUnitario, e.ValorTotal })
            .ToListAsync(ct);

        var itensRaw = await _db.ItensEstoque
            .Where(i => i.IngredienteId != null)
            .Select(i => new { i.Id, i.IngredienteId, i.Nome, i.CustoMedio })
            .ToListAsync(ct);
        var mapaItem = itensRaw.ToDictionary(
            i => i.Id,
            i => new { IngredienteId = i.IngredienteId!.Value, i.Nome, i.CustoMedio });

        var fornecedores = await _db.Fornecedores.Select(f => new { f.Id, f.Nome }).ToListAsync(ct);
        var mapaForn = fornecedores.ToDictionary(f => f.Id, f => f.Nome);

        var lista = new List<EntradaInfo>();
        foreach (var e in entradas)
        {
            if (!mapaItem.TryGetValue(e.ItemEstoqueId, out var item))
            {
                continue; // entrada de produto acabado ou item sem ingrediente
            }

            string? forn = e.FornecedorId is { } fid && mapaForn.TryGetValue(fid, out var fn) ? fn : null;
            lista.Add(new EntradaInfo(
                e.DataCompra, item.IngredienteId, item.Nome, forn,
                e.Quantidade, e.UnidadeMedida.ToString(), e.ValorUnitario, e.ValorTotal, item.CustoMedio));
        }

        return lista;
    }

    private async Task<decimal> ReceitaRecorrenteAtivaAsync(CancellationToken ct) =>
        await _db.Clientes
            .Where(c => c.Ativo && c.TipoCliente == TipoCliente.Assinante)
            .SumAsync(c => c.ValorRecorrenteMensal, ct);

    // =======================================================================
    // Dashboard
    // =======================================================================
    public async Task<DashboardDto> ObterDashboardAsync(DateOnly inicio, DateOnly fim, long? ingredienteId, CancellationToken ct = default)
    {
        var vendas = await CarregarVendasAsync(inicio, fim, ct);
        var consumos = await CarregarConsumosAsync(inicio, fim, ct);
        var entradas = await CarregarEntradasAsync(inicio, fim, ct);
        var receitaRecorrente = await ReceitaRecorrenteAtivaAsync(ct);
        var cancelamentos = await CarregarCancelamentosAsync(inicio, fim, null, null, null, ct);

        // ----- Charts -----
        var vendasPorMes = vendas
            .GroupBy(v => Mes(v.Data))
            .OrderBy(g => g.Key)
            .Select(g => new VendasPorMesDto(g.Key, Round(g.Sum(x => x.Kg)), g.Count(), 0m))
            .ToList();

        var vendasPorTipo = vendas
            .GroupBy(v => v.Tipo)
            .Select(g => new VendasPorTipoDto(g.Key, Round(g.Sum(x => x.Kg)), g.Count()))
            .OrderByDescending(x => x.Kg)
            .ToList();

        var cancelPorMotivo = cancelamentos
            .GroupBy(c => c.Motivo)
            .Select(g => new CancelamentoPorMotivoDto(g.Key, g.Count(), Round(g.Sum(x => x.ValorMensalPerdido)), Round(g.Sum(x => x.KgMensalPerdido))))
            .OrderByDescending(x => x.Quantidade)
            .ToList();

        var producaoMes = consumos
            .GroupBy(c => Mes(c.Data))
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var plan = g.Sum(x => x.PlanejadoCru) / 1000m;
                var real = g.Sum(x => x.RealCru) / 1000m;
                return new ProducaoPlanRealDto(g.Key, Round(plan), Round(real), Round(real - plan));
            })
            .ToList();

        var custoPerdasTop = consumos
            .GroupBy(c => new { c.IngredienteId, c.Ingrediente })
            .Select(g => new CustoPerdaIngredienteDto(
                g.Key.Ingrediente,
                Round(g.Sum(x => x.Perda) / 1000m),
                Round(g.Sum(x => x.Perda / 1000m * x.CustoKg)),
                Round(g.Sum(x => x.Sobra) / 1000m),
                Round(g.Sum(x => x.Sobra / 1000m * x.CustoKg)),
                true))
            .OrderByDescending(x => x.PerdaValor)
            .Take(10)
            .ToList();

        // ----- Evolução de custo (insumo selecionado ou o de maior nº de entradas) -----
        long? ingEvolId = ingredienteId;
        if (ingEvolId is null && entradas.Count > 0)
        {
            ingEvolId = entradas
                .GroupBy(e => e.IngredienteId)
                .OrderByDescending(g => g.Count())
                .First().Key;
        }

        string? ingEvolNome = null;
        var evolucao = new List<EvolucaoCustoPontoDto>();
        if (ingEvolId is { } iid)
        {
            var doIng = entradas.Where(e => e.IngredienteId == iid).OrderBy(e => e.Data).ToList();
            ingEvolNome = doIng.FirstOrDefault()?.Ingrediente;
            decimal? anterior = null;
            foreach (var g in doIng.GroupBy(e => Mes(e.Data)).OrderBy(g => g.Key))
            {
                var media = g.Average(x => x.ValorUnitario);
                var ultimo = g.OrderBy(x => x.Data).Last().ValorUnitario;
                var varPct = anterior is { } a && a > 0 ? Round((media - a) / a * 100m) : 0m;
                evolucao.Add(new EvolucaoCustoPontoDto(g.Key, Round(media), Round(ultimo), varPct));
                anterior = media;
            }
        }

        // ----- Ticket médio por tipo (apenas onde há valor real) -----
        var valoresAssinatura = await _db.Clientes
            .Where(c => c.Ativo && c.TipoCliente == TipoCliente.Assinante && c.ValorRecorrenteMensal > 0)
            .Select(c => c.ValorRecorrenteMensal)
            .ToListAsync(ct);
        decimal? ticketAssinatura = valoresAssinatura.Count > 0 ? valoresAssinatura.Average() : null;

        var valoresAvulsa = vendas
            .Where(v => v.Tipo == TipoAvulsa)
            .Select(v => ParseValorAvulsa(v.Observacoes))
            .Where(x => x is > 0m)
            .Select(x => x!.Value)
            .ToList();
        decimal? ticketAvulsa = valoresAvulsa.Count > 0 ? valoresAvulsa.Average() : null;

        var valoresPj = await _db.Pedidos
            .Where(p => p.DataPedido >= inicio && p.DataPedido <= fim && p.ValorTotal != null && p.ValorTotal > 0)
            .Select(p => p.ValorTotal!.Value)
            .ToListAsync(ct);
        decimal? ticketPj = valoresPj.Count > 0 ? valoresPj.Average() : null;

        // ----- Cards -----
        var kgPeriodo = vendas.Sum(v => v.Kg);
        var difCru = consumos.Sum(c => c.RealCru - c.PlanejadoCru) / 1000m;
        var custoPerdasTotal = consumos.Sum(c => c.Perda / 1000m * c.CustoKg);
        var insumoMaiorAumento = ResumoCustosInsumos(entradas)
            .OrderByDescending(x => x.VariacaoPercentual)
            .FirstOrDefault();

        var cards = new List<DashboardCardDto>
        {
            new("kg_vendidos", "Kg vendidos no período", FmtKg(kgPeriodo), null, false, false),
            new("vendas", "Vendas no período", vendas.Count.ToString(), "entregas (exceto canceladas)", false, false),
            new("ticket_assinatura", "Ticket médio assinatura", ticketAssinatura is { } ta ? FmtMoeda(ta) : "—", "R$/mês por assinante", false, ticketAssinatura is null),
            new("ticket_avulsa", "Ticket médio avulsa", ticketAvulsa is { } tav ? FmtMoeda(tav) : "—", "por venda (quando informado)", false, ticketAvulsa is null),
            new("ticket_pj", "Ticket médio PJ", ticketPj is { } tp ? FmtMoeda(tp) : "—", ticketPj is null ? "Valor do pedido ainda não registrado" : "por pedido", false, ticketPj is null),
            new("receita_recorrente", "Receita recorrente ativa", FmtMoeda(receitaRecorrente), "por mês", false, false),
            new("cancelamentos", "Cancelamentos", cancelamentos.Count.ToString(), null, false, false),
            new("receita_perdida", "Receita perdida (cancelamentos)", FmtMoeda(cancelamentos.Sum(c => c.ValorMensalPerdido)), "por mês", false, false),
            new("kg_produzidos", "Kg produzidos (real)", FmtKg(consumos.Sum(c => c.RealCru) / 1000m), "cru real finalizado", false, false),
            new("dif_plan_real", "Diferença planejado × real", FmtKg(difCru), "cru (real − previsto)", false, false),
            new("custo_perdas", "Custo das perdas", FmtMoeda(custoPerdasTotal), "estimado (custo médio)", true, false),
            new("insumo_aumento", "Insumo que mais subiu", insumoMaiorAumento?.Ingrediente ?? "—",
                insumoMaiorAumento is null ? "Sem compras no período" : $"+{insumoMaiorAumento.VariacaoPercentual:0.#}%", false, insumoMaiorAumento is null),
        };

        return new DashboardDto(
            new RelatorioPeriodo(inicio, fim),
            cards, vendasPorMes, vendasPorTipo, cancelPorMotivo, producaoMes, custoPerdasTop,
            ingEvolId, ingEvolNome, evolucao, ReceitaPorVendaIndisponivel: true);
    }

    // =======================================================================
    // Vendas
    // =======================================================================
    public async Task<RelatorioVendasDto> ObterVendasAsync(RelatorioFiltro f, CancellationToken ct = default)
    {
        var (inicio, fim) = Periodo(f);
        var vendas = await CarregarVendasAsync(inicio, fim, ct);

        if (!string.IsNullOrWhiteSpace(f.Tipo)) vendas = vendas.Where(v => v.Tipo == f.Tipo).ToList();
        if (f.ClienteId is { } cid) vendas = vendas.Where(v => v.ClienteId == cid).ToList();
        if (!string.IsNullOrWhiteSpace(f.Cidade)) vendas = vendas.Where(v => string.Equals(v.Cidade, f.Cidade, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(f.Status)) vendas = vendas.Where(v => string.Equals(v.Status, f.Status, StringComparison.OrdinalIgnoreCase)).ToList();

        var receitaRecorrente = await ReceitaRecorrenteAtivaAsync(ct);

        var porTipo = vendas.GroupBy(v => v.Tipo)
            .Select(g => new VendasPorTipoDto(g.Key, Round(g.Sum(x => x.Kg)), g.Count()))
            .OrderByDescending(x => x.Kg).ToList();
        var porReceita = vendas.Where(v => !string.IsNullOrWhiteSpace(v.Receitas))
            .SelectMany(v => v.Receitas.Split(", ", StringSplitOptions.RemoveEmptyEntries).Select(r => new { r, v.Kg }))
            .GroupBy(x => x.r)
            .Select(g => new ChaveValorDto(g.Key, Round(g.Sum(x => x.Kg)), g.Count()))
            .OrderByDescending(x => x.Kg).Take(20).ToList();
        var porCidade = vendas.GroupBy(v => string.IsNullOrWhiteSpace(v.Cidade) ? "—" : v.Cidade!)
            .Select(g => new ChaveValorDto(g.Key, Round(g.Sum(x => x.Kg)), g.Count()))
            .OrderByDescending(x => x.Kg).ToList();

        var resumo = new RelatorioVendasResumoDto(
            vendas.Count, Round(vendas.Sum(v => v.Kg)), Round(receitaRecorrente), Round(vendas.Sum(v => v.Custo)),
            porTipo, porReceita, porCidade, ReceitaPorVendaIndisponivel: true);

        var ordenadas = vendas.OrderByDescending(v => v.Data).ToList();
        var (pagina, tam, pagic) = Paginar(ordenadas, f);
        var linhas = pagic.Select(v => new VendaLinhaDto(v.Data, v.Tipo, v.Cliente, v.Pet, v.Raca, v.Valor, v.Custo, v.Origem)).ToList();

        return new RelatorioVendasDto(new RelatorioPeriodo(inicio, fim), resumo, linhas, ordenadas.Count, pagina, tam);
    }

    // =======================================================================
    // Cancelamentos
    // =======================================================================
    private sealed record CancelInfo(
        DateOnly? Data, string Cliente, string? Pets, string Motivo, string? Observacao,
        decimal ValorMensalPerdido, decimal KgMensalPerdido, int? DiasComoCliente, string? Usuario, string? Cidade);

    private async Task<List<CancelInfo>> CarregarCancelamentosAsync(
        DateOnly inicio, DateOnly fim, long? motivoId, string? cidade, long? clienteId, CancellationToken ct)
    {
        var clientes = await _db.Clientes
            .Where(c => c.DataCancelamento != null)
            .Select(c => new
            {
                c.Id,
                c.Nome,
                c.DataCancelamento,
                c.MotivoCancelamento,
                c.MotivoCancelamentoId,
                c.ObservacaoCancelamento,
                c.UsuarioCancelamentoId,
                c.ValorRecorrenteMensal,
                c.CreatedAt,
                c.Cidade,
            })
            .ToListAsync(ct);

        var motivos = await _db.MotivosCancelamento.Select(m => new { m.Id, m.Nome }).ToListAsync(ct);
        var mapaMotivo = motivos.ToDictionary(m => m.Id, m => m.Nome);
        var usuarios = await _db.Usuarios.Select(u => new { u.Id, u.Nome }).ToListAsync(ct);
        var mapaUsuario = usuarios.ToDictionary(u => u.Id, u => u.Nome);

        var pets = await _db.Pets.Where(p => p.Ativo)
            .Select(p => new { p.ClienteId, p.Nome, p.GramasDiaAjustadas })
            .ToListAsync(ct);
        var petsPorCliente = pets.GroupBy(p => p.ClienteId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var lista = new List<CancelInfo>();
        foreach (var c in clientes)
        {
            var dataUtc = c.DataCancelamento!.Value.UtcDateTime;
            var data = DateOnly.FromDateTime(dataUtc);
            if (data < inicio || data > fim) continue;
            if (clienteId is { } cli && c.Id != cli) continue;
            if (motivoId is { } mid && c.MotivoCancelamentoId != mid) continue;
            if (!string.IsNullOrWhiteSpace(cidade) && !string.Equals(c.Cidade, cidade, StringComparison.OrdinalIgnoreCase)) continue;

            string motivo = c.MotivoCancelamentoId is { } id && mapaMotivo.TryGetValue(id, out var mn)
                ? mn
                : (string.IsNullOrWhiteSpace(c.MotivoCancelamento) ? "Não informado" : c.MotivoCancelamento!);

            petsPorCliente.TryGetValue(c.Id, out var ps);
            var nomesPets = ps is null ? null : string.Join(", ", ps.Select(p => p.Nome));
            var kgMes = ps is null ? 0m : ps.Sum(p => (p.GramasDiaAjustadas ?? 0) * 30m / 1000m);

            int? dias = (int)(dataUtc.Date - c.CreatedAt.UtcDateTime.Date).TotalDays;
            string? usuario = c.UsuarioCancelamentoId is { } uid && mapaUsuario.TryGetValue(uid, out var un) ? un : null;

            lista.Add(new CancelInfo(
                data, c.Nome, nomesPets, motivo, c.ObservacaoCancelamento,
                c.ValorRecorrenteMensal, Round(kgMes), dias, usuario, c.Cidade));
        }

        return lista;
    }

    public async Task<RelatorioCancelamentosDto> ObterCancelamentosAsync(RelatorioFiltro f, CancellationToken ct = default)
    {
        var (inicio, fim) = Periodo(f);
        var cancel = await CarregarCancelamentosAsync(inicio, fim, f.MotivoId, f.Cidade, f.ClienteId, ct);

        var porMotivo = cancel.GroupBy(c => c.Motivo)
            .Select(g => new CancelamentoPorMotivoDto(g.Key, g.Count(), Round(g.Sum(x => x.ValorMensalPerdido)), Round(g.Sum(x => x.KgMensalPerdido))))
            .OrderByDescending(x => x.Quantidade).ToList();
        var porCidade = cancel.GroupBy(c => string.IsNullOrWhiteSpace(c.Cidade) ? "—" : c.Cidade!)
            .Select(g => new ChaveValorDto(g.Key, Round(g.Sum(x => x.KgMensalPerdido)), g.Count()))
            .OrderByDescending(x => x.Quantidade).ToList();

        var total = cancel.Count;
        var receitaPerdida = cancel.Sum(c => c.ValorMensalPerdido);
        var resumo = new RelatorioCancelamentosResumoDto(
            total,
            Round(receitaPerdida),
            Round(cancel.Sum(c => c.KgMensalPerdido)),
            total > 0 ? Round(receitaPerdida / total) : 0m,
            cancel.Where(c => c.DiasComoCliente != null).Select(c => (double)c.DiasComoCliente!.Value).DefaultIfEmpty().Average(),
            porMotivo, porCidade);

        var ordenadas = cancel.OrderByDescending(c => c.Data).ToList();
        var (pagina, tam, pagic) = Paginar(ordenadas, f);
        var linhas = pagic.Select(c => new CancelamentoLinhaDto(
            c.Data, c.Cliente, c.Pets, c.Motivo, c.Observacao, c.ValorMensalPerdido, c.KgMensalPerdido, c.DiasComoCliente, c.Usuario)).ToList();

        return new RelatorioCancelamentosDto(new RelatorioPeriodo(inicio, fim), resumo, linhas, ordenadas.Count, pagina, tam);
    }

    // =======================================================================
    // Produção planejado x real
    // =======================================================================
    public async Task<RelatorioProducaoDto> ObterProducaoAsync(RelatorioFiltro f, CancellationToken ct = default)
    {
        var (inicio, fim) = Periodo(f);
        var consumos = await CarregarConsumosAsync(inicio, fim, ct);
        if (f.IngredienteId is { } ing) consumos = consumos.Where(c => c.IngredienteId == ing).ToList();

        var ordens = await _db.OrdensProducao
            .Where(o => o.Data >= inicio && o.Data <= fim)
            .Select(o => new { o.Status })
            .ToListAsync(ct);

        var custoPlan = consumos.Sum(c => c.PlanejadoCru / 1000m * c.CustoKg);
        var custoReal = consumos.Sum(c => c.RealCru / 1000m * c.CustoKg);

        var resumo = new RelatorioProducaoResumoDto(
            ordens.Count,
            ordens.Count(o => o.Status == StatusOrdemProducao.Finalizada),
            Round(consumos.Sum(c => c.PlanejadoCru) / 1000m),
            Round(consumos.Sum(c => c.RealCru) / 1000m),
            consumos.Sum(c => c.PlanejadoCru),
            Round(consumos.Sum(c => c.RealCru)),
            Round(consumos.Sum(c => c.RealCru - c.PlanejadoCru)),
            consumos.Sum(c => c.PlanejadoCozido),
            Round(consumos.Sum(c => c.RealCozido)),
            Round(consumos.Sum(c => c.RealCozido - c.PlanejadoCozido)),
            Round(custoPlan), Round(custoReal), Round(custoReal - custoPlan), true);

        var ordenadas = consumos.OrderByDescending(c => c.Data).ThenBy(c => c.Ingrediente).ToList();
        var (pagina, tam, pagic) = Paginar(ordenadas, f);
        var linhas = pagic.Select(c => new ProducaoLinhaDto(
            c.Data, c.Ingrediente, c.PlanejadoCru, Round(c.RealCru), c.PlanejadoCozido, Round(c.RealCozido),
            Round(c.RealCru - c.PlanejadoCru),
            Round(c.PlanejadoCru / 1000m * c.CustoKg), Round(c.RealCru / 1000m * c.CustoKg), "Finalizada")).ToList();

        return new RelatorioProducaoDto(new RelatorioPeriodo(inicio, fim), resumo, linhas, ordenadas.Count, pagina, tam);
    }

    // =======================================================================
    // Perdas de ingredientes
    // =======================================================================
    public async Task<RelatorioPerdasDto> ObterPerdasAsync(RelatorioFiltro f, CancellationToken ct = default)
    {
        var (inicio, fim) = Periodo(f);
        var consumos = await CarregarConsumosAsync(inicio, fim, ct);
        if (f.IngredienteId is { } ing) consumos = consumos.Where(c => c.IngredienteId == ing).ToList();

        var porIngrediente = consumos
            .GroupBy(c => new { c.IngredienteId, c.Ingrediente })
            .Select(g => new CustoPerdaIngredienteDto(
                g.Key.Ingrediente,
                Round(g.Sum(x => x.Perda) / 1000m),
                Round(g.Sum(x => x.Perda / 1000m * x.CustoKg)),
                Round(g.Sum(x => x.Sobra) / 1000m),
                Round(g.Sum(x => x.Sobra / 1000m * x.CustoKg)),
                true))
            .OrderByDescending(x => x.PerdaValor).ToList();

        var resumo = new RelatorioPerdasResumoDto(
            Round(consumos.Sum(c => c.Perda) / 1000m),
            Round(consumos.Sum(c => c.Perda / 1000m * c.CustoKg)),
            Round(consumos.Sum(c => c.Sobra) / 1000m),
            Round(consumos.Sum(c => c.Sobra / 1000m * c.CustoKg)),
            porIngrediente, true);

        var ordenadas = consumos.OrderByDescending(c => c.Perda).ToList();
        var (pagina, tam, pagic) = Paginar(ordenadas, f);
        var linhas = pagic.Select(c => new PerdaLinhaDto(
            c.Data, c.Ingrediente,
            Round(c.Perda / 1000m), Round(c.Perda / 1000m * c.CustoKg),
            Round(c.Sobra / 1000m), Round(c.Sobra / 1000m * c.CustoKg),
            c.CustoKg, c.FatorCadastrado,
            c.RealCru > 0 ? Round(c.RealCozido / c.RealCru) : (decimal?)null, true)).ToList();

        return new RelatorioPerdasDto(new RelatorioPeriodo(inicio, fim), resumo, linhas, ordenadas.Count, pagina, tam);
    }

    // =======================================================================
    // Evolução de custo dos insumos
    // =======================================================================
    private static List<CustoInsumoResumoItemDto> ResumoCustosInsumos(List<EntradaInfo> entradas) =>
        entradas
            .GroupBy(e => new { e.IngredienteId, e.Ingrediente })
            .Select(g =>
            {
                var ordenado = g.OrderBy(x => x.Data).ToList();
                var primeiro = ordenado.First().ValorUnitario;
                var ultimo = ordenado.Last().ValorUnitario;
                var varValor = ultimo - primeiro;
                var varPct = primeiro > 0 ? varValor / primeiro * 100m : 0m;
                return new CustoInsumoResumoItemDto(
                    g.Key.IngredienteId, g.Key.Ingrediente,
                    Round(ordenado.Last().CustoMedioAtual),
                    Round(ultimo),
                    Round(g.Min(x => x.ValorUnitario)),
                    Round(g.Max(x => x.ValorUnitario)),
                    Round(g.Sum(x => x.Quantidade)),
                    Round(g.Sum(x => x.ValorTotal)),
                    Round(varValor), Round(varPct));
            })
            .ToList();

    public async Task<RelatorioCustosInsumosDto> ObterCustosInsumosAsync(RelatorioFiltro f, CancellationToken ct = default)
    {
        var (inicio, fim) = Periodo(f);
        var entradas = await CarregarEntradasAsync(inicio, fim, ct);
        if (f.IngredienteId is { } ing) entradas = entradas.Where(e => e.IngredienteId == ing).ToList();
        if (f.FornecedorId is { } fid)
        {
            // Fornecedor é carregado por nome; refiltra por nome do fornecedor selecionado.
            var nome = await _db.Fornecedores.Where(x => x.Id == fid).Select(x => x.Nome).FirstOrDefaultAsync(ct);
            if (nome is not null) entradas = entradas.Where(e => e.Fornecedor == nome).ToList();
        }

        var resumo = ResumoCustosInsumos(entradas).OrderByDescending(x => x.VariacaoPercentual).ToList();

        var ordenadas = entradas.OrderByDescending(e => e.Data).ToList();
        var (pagina, tam, pagic) = Paginar(ordenadas, f);
        var linhas = pagic.Select(e => new CustoInsumoLinhaDto(
            e.Data, e.Ingrediente, e.Fornecedor, e.Quantidade, e.Unidade, e.ValorUnitario, e.ValorTotal, "Compra")).ToList();

        return new RelatorioCustosInsumosDto(new RelatorioPeriodo(inicio, fim), resumo, linhas, ordenadas.Count, pagina, tam, HistoricoLimitado: true);
    }

    // =======================================================================
    // Estoque e produto acabado
    // =======================================================================
    public async Task<RelatorioEstoqueDto> ObterEstoqueAsync(RelatorioFiltro f, CancellationToken ct = default)
    {
        var itens = await _db.ItensEstoque
            .Where(i => i.Ativo)
            .Select(i => new { i.Nome, i.Tipo, i.QuantidadeAtual, i.UnidadeMedida, i.QuantidadeMinima, i.CustoMedio })
            .ToListAsync(ct);

        var linhasAll = itens.Select(i =>
        {
            var tipo = i.Tipo == TipoItemEstoque.Insumo ? "Insumo" : "Produto acabado";
            var status = i.QuantidadeAtual <= 0 ? "Zerado" : (i.QuantidadeAtual < i.QuantidadeMinima ? "Baixo" : "Com saldo");
            return new EstoqueLinhaDto(
                i.Nome, tipo, Round(i.QuantidadeAtual), i.UnidadeMedida.ToString(),
                Round(i.QuantidadeMinima), Round(i.CustoMedio), Round(i.QuantidadeAtual * i.CustoMedio), status);
        }).ToList();

        if (!string.IsNullOrWhiteSpace(f.Tipo))
            linhasAll = linhasAll.Where(l => l.Tipo.Equals(f.Tipo, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(f.Status))
            linhasAll = linhasAll.Where(l => l.Status.Equals(f.Status, StringComparison.OrdinalIgnoreCase)).ToList();

        var resumo = new RelatorioEstoqueResumoDto(
            linhasAll.Count,
            linhasAll.Count(l => l.Status == "Baixo"),
            linhasAll.Count(l => l.Status == "Zerado"),
            Round(linhasAll.Sum(l => l.ValorEstimado)),
            linhasAll.Count(l => l.Tipo == "Produto acabado" && l.Status != "Zerado"),
            linhasAll.Count(l => l.Tipo == "Produto acabado" && l.Status == "Zerado"),
            ComprometidoIndisponivel: true);

        var ordenadas = linhasAll
            .OrderBy(l => l.Status == "Com saldo" ? 1 : 0)
            .ThenBy(l => l.Item)
            .ToList();

        return new RelatorioEstoqueDto(resumo, ordenadas);
    }

    // =======================================================================
    // Utilitários
    // =======================================================================
    private static (DateOnly inicio, DateOnly fim) Periodo(RelatorioFiltro f)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var fim = f.Fim ?? hoje;
        var inicio = f.Inicio ?? fim.AddDays(-29);
        return (inicio, fim);
    }

    private static (int pagina, int tamanho, List<T> itens) Paginar<T>(List<T> fonte, RelatorioFiltro f)
    {
        var tamanho = f.TamanhoPagina is > 0 and <= 500 ? f.TamanhoPagina : 50;
        var pagina = f.Pagina > 0 ? f.Pagina : 1;
        var itens = fonte.Skip((pagina - 1) * tamanho).Take(tamanho).ToList();
        return (pagina, tamanho, itens);
    }

    /// <summary>Extrai o valor da venda avulsa gravado como texto nas observações ("Valor: R$ 1.234,56").</summary>
    private static decimal? ParseValorAvulsa(string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(observacoes))
        {
            return null;
        }

        var m = System.Text.RegularExpressions.Regex.Match(observacoes, @"Valor:\s*R\$\s*([\d\.]*\d,\d{2})");
        if (!m.Success)
        {
            return null;
        }

        var bruto = m.Groups[1].Value.Replace(".", string.Empty).Replace(",", ".");
        return decimal.TryParse(bruto, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v)
            ? v
            : null;
    }

    private static decimal Round(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
    private static string FmtKg(decimal kg) => $"{Round(kg):0.##} kg";
    private static string FmtMoeda(decimal v) => $"R$ {Round(v):0.00}";
}
