using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Estoque;

/// <summary>Situação de estoque de Produto Acabado da Casa (físico/comprometido/disponível/falta).</summary>
public sealed record SituacaoEstoqueItemDto(
    long ReceitaId,
    string ReceitaNome,
    int PesoGramas,
    string TamanhoNome,
    int Necessario,
    int Fisico,
    int Comprometido,
    int Disponivel,
    int Falta,
    bool TemFalta,
    bool SemItemEstoque);

public interface IProdutoAcabadoService
{
    Task<IReadOnlyList<SituacaoEstoqueItemDto>> SituacaoEntregaAsync(long entregaId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SituacaoEstoqueItemDto>> SituacaoPedidoAsync(long pedidoId, CancellationToken cancellationToken = default);
}

public sealed class ProdutoAcabadoService : IProdutoAcabadoService
{
    // Status que comprometem estoque (ativos, ainda não entregues/cancelados/reagendados).
    private static readonly EntregaStatus[] StatusAtivos =
    [
        EntregaStatus.Programada,
        EntregaStatus.ConfirmadaCliente,
        EntregaStatus.SaiuParaEntrega,
        EntregaStatus.NaoEntregue,
    ];

    private readonly IApplicationDbContext _db;

    public ProdutoAcabadoService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<SituacaoEstoqueItemDto>> SituacaoEntregaAsync(long entregaId, CancellationToken cancellationToken = default)
    {
        var entrega = await _db.Entregas
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == entregaId, cancellationToken)
            ?? throw new NotFoundException("Entrega", entregaId);

        var necessidade = new List<Linha>();
        foreach (var item in entrega.Pets.SelectMany(p => p.Itens).Where(i => i.Tipo == TipoReceita.Casa))
        {
            foreach (var pac in item.Pacotes)
            {
                necessidade.Add(new Linha(item.ReceitaId, pac.PesoGramas, item.ReceitaNome, pac.TamanhoLabel, pac.Quantidade));
            }
        }

        return await ComputarAsync(necessidade, entrega.DataPrevista, entregaId, cancellationToken);
    }

    public async Task<IReadOnlyList<SituacaoEstoqueItemDto>> SituacaoPedidoAsync(long pedidoId, CancellationToken cancellationToken = default)
    {
        var pedido = await _db.Pedidos.Include(p => p.Itens).AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pedidoId, cancellationToken)
            ?? throw new NotFoundException("Pedido", pedidoId);

        var necessidade = pedido.Itens
            .Select(i => new Linha(i.ReceitaId, i.PesoGramas, i.ReceitaNome, i.TamanhoNome, i.Quantidade))
            .ToList();

        return await ComputarAsync(necessidade, pedido.DataEntrega, pedido.EntregaId ?? 0, cancellationToken);
    }

    private async Task<IReadOnlyList<SituacaoEstoqueItemDto>> ComputarAsync(
        List<Linha> necessidade, DateOnly dataReferencia, long excluirEntregaId, CancellationToken ct)
    {
        // Agrega a necessidade por (receita, peso).
        var agregada = necessidade
            .GroupBy(l => (l.ReceitaId, l.Peso))
            .Select(g => new { g.Key.ReceitaId, g.Key.Peso, g.First().ReceitaNome, g.First().TamanhoNome, Necessario = g.Sum(x => x.Quantidade) })
            .ToList();
        if (agregada.Count == 0)
        {
            return [];
        }

        // Físico: produto acabado por (receita, peso).
        var pesoPorTamanho = await _db.TamanhosPacote.AsNoTracking()
            .Select(t => new { t.Id, t.PesoGramas })
            .ToDictionaryAsync(t => t.Id, t => t.PesoGramas, ct);
        var produtoAcabado = await _db.ItensEstoque.AsNoTracking()
            .Where(x => x.Tipo == TipoItemEstoque.ProdutoAcabadoCasa && x.ReceitaId != null && x.TamanhoPacoteId != null)
            .Select(x => new { ReceitaId = x.ReceitaId!.Value, TamanhoPacoteId = x.TamanhoPacoteId!.Value, x.QuantidadeAtual })
            .ToListAsync(ct);
        var fisicoPorChave = new Dictionary<(long, int), int>();
        foreach (var pa in produtoAcabado)
        {
            if (pesoPorTamanho.TryGetValue(pa.TamanhoPacoteId, out var peso))
            {
                fisicoPorChave[(pa.ReceitaId, peso)] = (int)Math.Floor(pa.QuantidadeAtual);
            }
        }

        // Comprometido (dinâmico): entregas ativas de Casa até a data de referência, exceto a própria.
        var entregasAtivas = await _db.Entregas
            .Where(e => StatusAtivos.Contains(e.Status) && e.DataPrevista <= dataReferencia && e.Id != excluirEntregaId)
            .Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
            .AsNoTracking()
            .ToListAsync(ct);
        var comprometidoPorChave = new Dictionary<(long, int), int>();
        foreach (var item in entregasAtivas.SelectMany(e => e.Pets).SelectMany(p => p.Itens).Where(i => i.Tipo == TipoReceita.Casa))
        {
            foreach (var pac in item.Pacotes)
            {
                var chave = (item.ReceitaId, pac.PesoGramas);
                comprometidoPorChave[chave] = (comprometidoPorChave.TryGetValue(chave, out var v) ? v : 0) + pac.Quantidade;
            }
        }

        return agregada.Select(a =>
        {
            var chave = (a.ReceitaId, a.Peso);
            var temItem = fisicoPorChave.TryGetValue(chave, out var fisico);
            var comprometido = comprometidoPorChave.TryGetValue(chave, out var c) ? c : 0;
            var disponivel = fisico - comprometido;
            var falta = Math.Max(0, a.Necessario - disponivel);
            return new SituacaoEstoqueItemDto(
                a.ReceitaId, a.ReceitaNome, a.Peso, a.TamanhoNome,
                a.Necessario, fisico, comprometido, disponivel, falta, falta > 0, !temItem);
        }).ToList();
    }

    private sealed record Linha(long ReceitaId, int Peso, string ReceitaNome, string TamanhoNome, int Quantidade);
}
