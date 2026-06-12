using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Estoque;

namespace SistemaAN.Application.Estoque;

public sealed class EstoqueMovimentacaoService : IEstoqueMovimentacaoService
{
    private static readonly HashSet<TipoMovimentacao> SaidasPermitidas =
    [
        TipoMovimentacao.SaidaProducao,
        TipoMovimentacao.Descarte,
        TipoMovimentacao.Perda,
        TipoMovimentacao.Vencimento,
        TipoMovimentacao.TransferenciaSaida,
        TipoMovimentacao.ConsumoInterno,
    ];

    private readonly IApplicationDbContext _db;
    private readonly IItemEstoqueService _itens;

    public EstoqueMovimentacaoService(IApplicationDbContext db, IItemEstoqueService itens)
    {
        _db = db;
        _itens = itens;
    }

    public async Task<ItemEstoqueDto> RegistrarEntradaAsync(RegistrarEntradaRequest request, string usuario, CancellationToken cancellationToken = default)
    {
        var item = await _db.ItensEstoque.FirstOrDefaultAsync(x => x.Id == request.ItemEstoqueId, cancellationToken)
            ?? throw new NotFoundException("Item de estoque", request.ItemEstoqueId);

        var erros = new Dictionary<string, string[]>();
        if (request.Quantidade <= 0m)
        {
            erros["quantidade"] = ["A quantidade de entrada deve ser maior que zero."];
        }

        decimal custoUnitario = 0m;
        if (request.ValorUnitario is decimal vu && vu > 0m)
        {
            custoUnitario = vu;
        }
        else if (request.ValorTotal is decimal vt && vt > 0m && request.Quantidade > 0m)
        {
            custoUnitario = vt / request.Quantidade;
        }
        else
        {
            erros["valor"] = ["Informe o valor unitário ou o valor total da compra."];
        }

        if (request.FornecedorId is long fornId && !await _db.Fornecedores.AnyAsync(x => x.Id == fornId, cancellationToken))
        {
            erros["fornecedorId"] = ["Fornecedor não encontrado."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        var valorTotal = request.ValorTotal ?? Math.Round(request.Quantidade * custoUnitario, 2, MidpointRounding.AwayFromZero);
        var loteCodigo = string.IsNullOrWhiteSpace(request.LoteCodigo)
            ? $"L{DateTimeOffset.UtcNow:yyyyMMddHHmmss}"
            : request.LoteCodigo.Trim();
        var agora = DateTimeOffset.UtcNow;

        var saldoAnterior = item.QuantidadeAtual;
        var lote = LoteEstoque.Criar(item, loteCodigo, request.DataEntrada, request.Validade,
            request.Quantidade, custoUnitario, request.FornecedorId, OrigemLote.Compra, null);
        item.RegistrarEntrada(request.Quantidade, custoUnitario);

        var entrada = EntradaEstoque.Criar(item, lote, request.FornecedorId, request.Quantidade, item.UnidadeMedida,
            custoUnitario, valorTotal, request.DataCompra, request.DataEntrada, request.Validade, loteCodigo,
            request.LocalArmazenamento, usuario, request.Observacoes);

        var mov = MovimentacaoEstoque.CriarEntrada(item, lote, TipoMovimentacao.EntradaCompra, request.Quantidade,
            saldoAnterior, item.QuantidadeAtual, custoUnitario, usuario, agora, request.Observacoes);
        mov.Vincular(entrada);

        _db.LotesEstoque.Add(lote);
        _db.EntradasEstoque.Add(entrada);
        _db.MovimentacoesEstoque.Add(mov);
        await _db.SaveChangesAsync(cancellationToken);

        return await _itens.ObterAsync(item.Id, cancellationToken);
    }

    public async Task<ItemEstoqueDto> RegistrarSaidaAsync(RegistrarSaidaRequest request, string usuario, CancellationToken cancellationToken = default)
    {
        var item = await _db.ItensEstoque.FirstOrDefaultAsync(x => x.Id == request.ItemEstoqueId, cancellationToken)
            ?? throw new NotFoundException("Item de estoque", request.ItemEstoqueId);

        var erros = new Dictionary<string, string[]>();
        if (request.Quantidade <= 0m)
        {
            erros["quantidade"] = ["A quantidade de saída deve ser maior que zero."];
        }

        if (!Enum.TryParse<TipoMovimentacao>(request.Tipo, out var tipo) || !SaidasPermitidas.Contains(tipo))
        {
            erros["tipo"] = ["Tipo de saída inválido."];
        }

        MotivoSaida? motivoCodigo = null;
        if (!string.IsNullOrWhiteSpace(request.MotivoCodigo))
        {
            if (Enum.TryParse<MotivoSaida>(request.MotivoCodigo, out var m))
            {
                motivoCodigo = m;
            }
            else
            {
                erros["motivoCodigo"] = ["Motivo de saída inválido."];
            }
        }

        // Bloqueio de saldo negativo (decisão aprovada).
        if (request.Quantidade > item.QuantidadeAtual)
        {
            erros["quantidade"] = [$"Saldo insuficiente: disponível {item.QuantidadeAtual:0.###}, solicitado {request.Quantidade:0.###}."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        var agora = DateTimeOffset.UtcNow;
        var lotes = await _db.LotesEstoque
            .Where(l => l.ItemEstoqueId == item.Id && l.QuantidadeAtual > 0m && l.Status == StatusLote.Ativo)
            .ToListAsync(cancellationToken);
        var fifo = lotes
            .OrderBy(l => l.Validade ?? DateOnly.MaxValue)
            .ThenBy(l => l.DataEntrada)
            .ThenBy(l => l.Id)
            .ToList();

        var saldoCorrente = item.QuantidadeAtual;
        var restante = request.Quantidade;

        foreach (var lote in fifo)
        {
            if (restante <= 0m)
            {
                break;
            }

            var consumir = Math.Min(restante, lote.QuantidadeAtual);
            lote.Consumir(consumir);
            var mov = MovimentacaoEstoque.CriarSaida(item, lote, tipo, consumir, saldoCorrente, saldoCorrente - consumir,
                lote.CustoUnitario, usuario, agora, motivoCodigo, request.Motivo, request.Observacao);
            _db.MovimentacoesEstoque.Add(mov);
            saldoCorrente -= consumir;
            restante -= consumir;
        }

        // Remanescente sem lote rastreado (ex.: saldo originado de ajuste sem lote).
        if (restante > 0m)
        {
            var mov = MovimentacaoEstoque.CriarSaida(item, null, tipo, restante, saldoCorrente, saldoCorrente - restante,
                item.CustoMedio, usuario, agora, motivoCodigo, request.Motivo, request.Observacao);
            _db.MovimentacoesEstoque.Add(mov);
            saldoCorrente -= restante;
            restante = 0m;
        }

        item.RegistrarSaida(request.Quantidade);
        await _db.SaveChangesAsync(cancellationToken);

        return await _itens.ObterAsync(item.Id, cancellationToken);
    }

    public async Task<ItemEstoqueDto> RegistrarAjusteAsync(RegistrarAjusteRequest request, string usuario, CancellationToken cancellationToken = default)
    {
        var item = await _db.ItensEstoque.FirstOrDefaultAsync(x => x.Id == request.ItemEstoqueId, cancellationToken)
            ?? throw new NotFoundException("Item de estoque", request.ItemEstoqueId);

        var erros = new Dictionary<string, string[]>();
        if (request.NovaQuantidade < 0m)
        {
            erros["novaQuantidade"] = ["A nova quantidade não pode ser negativa."];
        }

        if (string.IsNullOrWhiteSpace(request.Motivo))
        {
            erros["motivo"] = ["Informe o motivo do ajuste."];
        }

        LoteEstoque? lote = null;
        if (request.LoteEstoqueId is long loteId)
        {
            lote = await _db.LotesEstoque.FirstOrDefaultAsync(l => l.Id == loteId, cancellationToken);
            if (lote is null || lote.ItemEstoqueId != item.Id)
            {
                erros["loteEstoqueId"] = ["Lote não encontrado para este item."];
            }
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        var agora = DateTimeOffset.UtcNow;
        decimal anteriorRegistro;
        decimal diferenca;
        var saldoItemAntes = item.QuantidadeAtual;

        if (lote is not null)
        {
            anteriorRegistro = lote.QuantidadeAtual;
            diferenca = request.NovaQuantidade - lote.QuantidadeAtual;
            lote.AjustarSaldo(request.NovaQuantidade);
            item.AjustarSaldo(item.QuantidadeAtual + diferenca);
        }
        else
        {
            anteriorRegistro = item.QuantidadeAtual;
            diferenca = request.NovaQuantidade - item.QuantidadeAtual;
            item.AjustarSaldo(request.NovaQuantidade);
        }

        if (diferenca == 0m)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["novaQuantidade"] = ["Informe uma quantidade diferente da atual."],
            });
        }

        var ajuste = AjusteEstoque.Criar(item, lote, anteriorRegistro, request.NovaQuantidade, request.Motivo, usuario, agora, request.Observacao);

        var quantidade = Math.Abs(diferenca);
        var mov = diferenca > 0m
            ? MovimentacaoEstoque.CriarEntrada(item, lote, TipoMovimentacao.AjustePositivo, quantidade,
                saldoItemAntes, item.QuantidadeAtual, item.CustoMedio, usuario, agora, request.Observacao)
            : MovimentacaoEstoque.CriarSaida(item, lote, TipoMovimentacao.AjusteNegativo, quantidade,
                saldoItemAntes, item.QuantidadeAtual, item.CustoMedio, usuario, agora, MotivoSaida.AjusteManual, request.Motivo, request.Observacao);
        mov.Vincular(ajuste);

        _db.AjustesEstoque.Add(ajuste);
        _db.MovimentacoesEstoque.Add(mov);
        await _db.SaveChangesAsync(cancellationToken);

        return await _itens.ObterAsync(item.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<LoteEstoqueDto>> ListarLotesAsync(long itemEstoqueId, CancellationToken cancellationToken = default)
    {
        var lotes = await _db.LotesEstoque
            .Where(l => l.ItemEstoqueId == itemEstoqueId)
            .OrderBy(l => l.DataEntrada)
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);

        var fornIds = lotes.Where(l => l.FornecedorId != null).Select(l => l.FornecedorId!.Value).Distinct().ToList();
        var fornNomes = await _db.Fornecedores.Where(f => fornIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, f => f.Nome, cancellationToken);

        return lotes.Select(l => new LoteEstoqueDto(
            l.Id, l.ItemEstoqueId, l.Codigo, l.DataEntrada, l.Validade, l.QuantidadeInicial, l.QuantidadeAtual,
            l.CustoUnitario, l.FornecedorId,
            l.FornecedorId != null && fornNomes.TryGetValue(l.FornecedorId.Value, out var n) ? n : null,
            l.Origem.ToString(), l.Status.ToString())).ToList();
    }

    public async Task<IReadOnlyList<MovimentacaoEstoqueDto>> ListarMovimentacoesAsync(long itemEstoqueId, CancellationToken cancellationToken = default)
    {
        var itemNome = await _db.ItensEstoque.Where(i => i.Id == itemEstoqueId).Select(i => i.Nome).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Item de estoque", itemEstoqueId);

        var movs = await _db.MovimentacoesEstoque
            .Where(m => m.ItemEstoqueId == itemEstoqueId)
            .OrderByDescending(m => m.DataHora)
            .ThenByDescending(m => m.Id)
            .ToListAsync(cancellationToken);

        var loteIds = movs.Where(m => m.LoteEstoqueId != null).Select(m => m.LoteEstoqueId!.Value).Distinct().ToList();
        var loteCodigos = await _db.LotesEstoque.Where(l => loteIds.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, l => l.Codigo, cancellationToken);

        return movs.Select(m => new MovimentacaoEstoqueDto(
            m.Id, m.ItemEstoqueId, itemNome, m.LoteEstoqueId,
            m.LoteEstoqueId != null && loteCodigos.TryGetValue(m.LoteEstoqueId.Value, out var c) ? c : null,
            m.Tipo.ToString(), m.Sentido.ToString(), m.Quantidade, m.SaldoAnteriorItem, m.SaldoPosteriorItem,
            m.CustoUnitario, m.ValorTotal, m.Usuario, m.DataHora,
            m.MotivoCodigo?.ToString(), m.Motivo, m.Observacao)).ToList();
    }
}
