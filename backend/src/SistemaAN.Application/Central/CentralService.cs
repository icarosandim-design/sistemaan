using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Producao;
using SistemaAN.Application.Rotas;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Central;

/// <summary>
/// Agrega o resumo da Central a partir dos dados reais. Reaproveita os serviços
/// donos (Produção, Rotas) para não duplicar regra; o restante são leituras
/// diretas e contagens. Sem persistência.
/// </summary>
public sealed class CentralService : ICentralService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private static readonly string[] StatusRotaAtiva = ["Rascunho", "Planejada", "Despachada"];

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;
    private readonly IProducaoService _producao;
    private readonly IRotaService _rotas;

    public CentralService(
        IApplicationDbContext db,
        IDateTimeProvider clock,
        IProducaoService producao,
        IRotaService rotas)
    {
        _db = db;
        _clock = clock;
        _producao = producao;
        _rotas = rotas;
    }

    public async Task<CentralResumoDto> ObterResumoAsync(DateOnly? data, CancellationToken cancellationToken = default)
    {
        var hoje = _clock.Today;
        var dia = data ?? hoje;

        var kpis = await MontarKpisAsync(dia, cancellationToken);
        var entregas7 = await MontarEntregas7Async(dia, hoje, cancellationToken);
        var (producao, casa, personalizada, ingredientes) = await MontarProducaoAsync(dia, cancellationToken);
        var estoque = await MontarEstoqueAsync(cancellationToken);
        var (rotas, entregasSemRota, rotasSemEntregador) = await MontarRotasAsync(dia, cancellationToken);
        var alertas = await MontarAlertasAsync(dia, hoje, producao, entregasSemRota, rotasSemEntregador, cancellationToken);

        return new CentralResumoDto(dia, kpis, entregas7, producao, casa, personalizada, ingredientes, estoque, rotas, alertas);
    }

    private async Task<IReadOnlyList<CentralKpiDto>> MontarKpisAsync(DateOnly dia, CancellationToken ct)
    {
        var pfAtivos = await _db.Clientes.CountAsync(c => c.Ativo && c.Natureza == NaturezaCliente.PessoaFisica, ct);
        var pjAtivos = await _db.Clientes.CountAsync(c => c.Ativo && c.Natureza == NaturezaCliente.PessoaJuridica, ct);
        var petsAtivos = await _db.Pets.CountAsync(p => p.Ativo, ct);
        var entregasDia = await _db.Entregas.CountAsync(e => e.DataPrevista == dia && e.Status != EntregaStatus.Cancelada, ct);
        var recorrente = await _db.Clientes.Where(c => c.Ativo).SumAsync(c => c.ValorRecorrenteMensal, ct);

        return new List<CentralKpiDto>
        {
            new("Clientes PF ativos", pfAtivos.ToString(PtBr), "group"),
            new("Clientes PJ ativos", pjAtivos.ToString(PtBr), "store"),
            new("Pets ativos", petsAtivos.ToString(PtBr), "pets"),
            new("Entregas hoje", entregasDia.ToString(PtBr), "local_shipping"),
            // Faturamento recorrente é a soma do valor mensal dos clientes ativos
            // (dado real). Só o Administrador enxerga este cartão.
            new("Recorrente / mês", recorrente.ToString("C0", PtBr), "payments", SomenteAdmin: true),
        };
    }

    private async Task<IReadOnlyList<CentralEntregaDiaDto>> MontarEntregas7Async(DateOnly dia, DateOnly hoje, CancellationToken ct)
    {
        var fim = dia.AddDays(6);
        var lista = await _db.Entregas
            .Where(e => e.DataPrevista >= dia && e.DataPrevista <= fim && e.Status != EntregaStatus.Cancelada)
            .Select(e => new { e.DataPrevista, e.PedidoId, e.Rua, e.Bairro, e.Cidade })
            .ToListAsync(ct);

        var dias = new List<CentralEntregaDiaDto>(7);
        for (var i = 0; i < 7; i++)
        {
            var d = dia.AddDays(i);
            var doDia = lista.Where(x => x.DataPrevista == d).ToList();
            var alertas = doDia.Count(x =>
                string.IsNullOrWhiteSpace(x.Rua) || string.IsNullOrWhiteSpace(x.Bairro) || string.IsNullOrWhiteSpace(x.Cidade));
            dias.Add(new CentralEntregaDiaDto(
                d,
                doDia.Count,
                doDia.Count(x => x.PedidoId == null),
                doDia.Count(x => x.PedidoId != null),
                alertas,
                d == hoje));
        }

        return dias;
    }

    private async Task<(CentralProducaoDto?, IReadOnlyList<CentralProducaoCasaDto>, IReadOnlyList<CentralProducaoPersonalizadaDto>, IReadOnlyList<CentralIngredienteDto>)>
        MontarProducaoAsync(DateOnly dia, CancellationToken ct)
    {
        var ordem = await _producao.ObterPorDataAsync(dia, ct);
        if (ordem is null)
        {
            return (null, [], [], []);
        }

        var ativas = ordem.Fichas.Where(f => f.Status != "NaoFeita").ToList();

        var casa = ativas
            .Where(f => f.Tipo == nameof(TipoReceita.Casa))
            .GroupBy(f => $"{f.ReceitaNome} {f.PesoPacoteGramas}g")
            .Select(g => new CentralProducaoCasaDto(g.Key, g.Sum(f => f.QuantidadePacotes)))
            .OrderBy(c => c.Produto)
            .ToList();

        var personalizada = ativas
            .Where(f => f.Tipo == nameof(TipoReceita.Personalizada))
            .Select(f => new CentralProducaoPersonalizadaDto(
                f.PetNome ?? "—",
                f.ReceitaCodigo,
                f.ClienteNome ?? "—",
                f.QuantidadePacotes))
            .ToList();

        // Estoque (cru) de cada insumo do consolidado, para a tabela de ingredientes.
        var itemIds = ordem.Consolidado.Where(c => c.ItemEstoqueId.HasValue).Select(c => c.ItemEstoqueId!.Value).Distinct().ToList();
        var saldos = await _db.ItensEstoque
            .Where(i => itemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.QuantidadeAtual })
            .ToDictionaryAsync(i => i.Id, i => i.QuantidadeAtual, ct);

        var ingredientes = ordem.Consolidado
            .Select(c => new CentralIngredienteDto(
                c.IngredienteNome,
                Math.Round(c.CozidoGramas / 1000m, 2),
                Math.Round(c.CruGramas / 1000m, 2),
                c.ItemEstoqueId.HasValue && saldos.TryGetValue(c.ItemEstoqueId.Value, out var s) ? s : 0m))
            .ToList();

        var pendencias = ordem.Fichas.Count(f => f.Status != "Conferida" && f.Status != "NaoFeita");
        var resumo = new CentralProducaoDto(
            ordem.Id,
            ordem.Status,
            ordem.Status == "Finalizada",
            ordem.Fichas.Count,
            casa.Count,
            personalizada.Count,
            pendencias);

        return (resumo, casa, personalizada, ingredientes);
    }

    private async Task<IReadOnlyList<CentralEstoqueItemDto>> MontarEstoqueAsync(CancellationToken ct)
    {
        var itens = await _db.ItensEstoque
            .Where(i => i.Ativo && i.Tipo == TipoItemEstoque.ProdutoAcabadoCasa)
            .Select(i => new { i.Nome, i.QuantidadeAtual, i.QuantidadeMinima })
            .ToListAsync(ct);

        return itens
            .OrderByDescending(i => i.QuantidadeAtual < i.QuantidadeMinima)
            .ThenBy(i => i.Nome)
            .Select(i => new CentralEstoqueItemDto(
                i.Nome,
                i.QuantidadeAtual,
                i.QuantidadeMinima,
                i.QuantidadeAtual < i.QuantidadeMinima ? "baixo" : "ok"))
            .ToList();
    }

    private async Task<(CentralRotasDto?, int EntregasSemRota, int SemEntregador)> MontarRotasAsync(DateOnly dia, CancellationToken ct)
    {
        var rotas = await _rotas.ListarPorDataAsync(dia, ct);
        var disponiveis = await _rotas.DisponiveisAsync(dia, ct);
        var entregasSemRota = disponiveis.Count;

        var semEntregador = rotas.Count(r => StatusRotaAtiva.Contains(r.Status) && string.IsNullOrWhiteSpace(r.Entregador));

        if (rotas.Count == 0 && entregasSemRota == 0)
        {
            return (null, 0, 0);
        }

        var resumo = new CentralRotasDto(
            rotas.Count(r => r.Status != "Cancelada"),
            rotas.Count(r => r.Status == "Planejada"),
            rotas.Count(r => r.Status == "Despachada"),
            rotas.Count(r => r.Status == "Concluida"),
            semEntregador,
            entregasSemRota);

        return (resumo, entregasSemRota, semEntregador);
    }

    private async Task<IReadOnlyList<CentralAlertaDto>> MontarAlertasAsync(
        DateOnly dia,
        DateOnly hoje,
        CentralProducaoDto? producao,
        int entregasSemRota,
        int rotasSemEntregador,
        CancellationToken ct)
    {
        var produtoAcabadoBaixo = await _db.ItensEstoque
            .CountAsync(i => i.Ativo && i.Tipo == TipoItemEstoque.ProdutoAcabadoCasa && i.QuantidadeAtual < i.QuantidadeMinima, ct);
        var insumoBaixo = await _db.ItensEstoque
            .CountAsync(i => i.Ativo && i.Tipo == TipoItemEstoque.Insumo && i.QuantidadeAtual < i.QuantidadeMinima, ct);

        // Materializa as entregas do dia (Include) e conta em memória — mesmo padrão
        // dos serviços de Produção/Rotas, evitando tradução de SelectMany aninhado.
        var entregasDoDia = await _db.Entregas
            .Where(e => e.DataPrevista == dia && e.Status != EntregaStatus.Cancelada)
            .Include(e => e.Pets).ThenInclude(p => p.Itens)
            .AsNoTracking()
            .ToListAsync(ct);

        var personalizadasPendentes = entregasDoDia
            .SelectMany(e => e.Pets).SelectMany(p => p.Itens)
            .Count(it => it.Tipo == TipoReceita.Personalizada && it.StatusPreparo != StatusPreparoPersonalizada.Pronta);

        var entregasDia = entregasDoDia.Count;

        var alertas = new List<CentralAlertaDto>();

        if (produtoAcabadoBaixo > 0)
        {
            alertas.Add(new("erro", "inventory_2",
                Plural(produtoAcabadoBaixo, "produto acabado abaixo do mínimo", "produtos acabados abaixo do mínimo"),
                "/estoque/itens"));
        }

        if (insumoBaixo > 0)
        {
            alertas.Add(new("erro", "eco",
                Plural(insumoBaixo, "ingrediente abaixo do mínimo", "ingredientes abaixo do mínimo"),
                "/estoque/itens"));
        }

        if (personalizadasPendentes > 0)
        {
            alertas.Add(new("aviso", "pending_actions",
                Plural(personalizadasPendentes, "receita personalizada aguardando produção", "receitas personalizadas aguardando produção"),
                "/producao/dia"));
        }

        if (producao is { Finalizada: false })
        {
            alertas.Add(new("aviso", "factory", "Produção do dia ainda não finalizada", "/producao/dia"));
        }

        if (entregasSemRota > 0)
        {
            alertas.Add(new("aviso", "alt_route",
                Plural(entregasSemRota, "entrega do dia sem rota", "entregas do dia sem rota"),
                "/rotas"));
        }

        if (rotasSemEntregador > 0)
        {
            alertas.Add(new("aviso", "person_off",
                Plural(rotasSemEntregador, "rota sem entregador", "rotas sem entregador"),
                "/rotas"));
        }

        if (entregasDia > 0)
        {
            var quando = dia == hoje ? "hoje" : dia.ToString("dd/MM", PtBr);
            alertas.Add(new("info", "local_shipping",
                dia == hoje
                    ? Plural(entregasDia, "entrega prevista para hoje", "entregas previstas para hoje")
                    : $"{entregasDia} {(entregasDia == 1 ? "entrega prevista" : "entregas previstas")} para {quando}",
                "/entregas"));
        }

        return alertas;
    }

    private static string Plural(int n, string singular, string plural) => n == 1 ? $"{n} {singular}" : $"{n} {plural}";
}
