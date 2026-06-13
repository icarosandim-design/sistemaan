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

        // Valor unitário ORIGINAL do produto (sem frete).
        decimal valorUnitarioOriginal = 0m;
        if (request.ValorUnitario is decimal vu && vu > 0m)
        {
            valorUnitarioOriginal = vu;
        }
        else if (request.ValorTotal is decimal vt && vt > 0m && request.Quantidade > 0m)
        {
            valorUnitarioOriginal = vt / request.Quantidade;
        }
        else
        {
            erros["valor"] = ["Informe o valor unitário ou o valor total da compra."];
        }

        var frete = request.Frete is decimal f && f > 0m ? f : 0m;

        if (request.FornecedorId is long fornId && !await _db.Fornecedores.AnyAsync(x => x.Id == fornId, cancellationToken))
        {
            erros["fornecedorId"] = ["Fornecedor não encontrado."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        // Frete NÃO compõe o custo nesta fase: custo do estoque = valor do produto.
        // O frete fica registrado separadamente na entrada (informativo/relatório).
        var custoUnitarioEstoque = valorUnitarioOriginal;
        var valorProdutos = Math.Round(valorUnitarioOriginal * request.Quantidade, 2, MidpointRounding.AwayFromZero);
        var loteCodigo = string.IsNullOrWhiteSpace(request.LoteCodigo)
            ? $"L{DateTimeOffset.UtcNow:yyyyMMddHHmmss}"
            : request.LoteCodigo.Trim();
        var agora = DateTimeOffset.UtcNow;

        var saldoAnterior = item.QuantidadeAtual;
        var lote = LoteEstoque.Criar(item, loteCodigo, request.DataEntrada, request.Validade,
            request.Quantidade, custoUnitarioEstoque, request.FornecedorId, OrigemLote.Compra, null);
        item.RegistrarEntrada(request.Quantidade, custoUnitarioEstoque);

        var entrada = EntradaEstoque.Criar(item, lote, request.FornecedorId, request.Quantidade, item.UnidadeMedida,
            valorUnitarioOriginal, frete, request.FreteCompoeCusto, valorProdutos, request.DataCompra, request.DataEntrada,
            request.Validade, loteCodigo, request.LocalArmazenamento, usuario, request.Observacoes);

        var mov = MovimentacaoEstoque.CriarEntrada(item, lote, TipoMovimentacao.EntradaCompra, request.Quantidade,
            saldoAnterior, item.QuantidadeAtual, custoUnitarioEstoque, usuario, agora, request.Observacoes);
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

    public async Task<MovimentacaoPaginaDto> ListarMovimentacoesGeralAsync(FiltroMovimentacoesRequest filtro, CancellationToken cancellationToken = default)
    {
        var q = from m in _db.MovimentacoesEstoque
                join i in _db.ItensEstoque on m.ItemEstoqueId equals i.Id
                join lo in _db.LotesEstoque on m.LoteEstoqueId equals lo.Id into loj
                from lo in loj.DefaultIfEmpty()
                select new { m, i, lo };

        if (filtro.DataInicio is DateOnly di)
        {
            var inicio = new DateTimeOffset(di.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            q = q.Where(x => x.m.DataHora >= inicio);
        }
        if (filtro.DataFim is DateOnly df)
        {
            var fim = new DateTimeOffset(df.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            q = q.Where(x => x.m.DataHora < fim);
        }
        if (filtro.ItemEstoqueId is long iid)
        {
            q = q.Where(x => x.m.ItemEstoqueId == iid);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Categoria) && Enum.TryParse<CategoriaEstoque>(filtro.Categoria, out var cat))
        {
            q = q.Where(x => x.i.Categoria == cat);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Tipo) && Enum.TryParse<TipoMovimentacao>(filtro.Tipo, out var tp))
        {
            q = q.Where(x => x.m.Tipo == tp);
        }
        if (filtro.LoteEstoqueId is long lid)
        {
            q = q.Where(x => x.m.LoteEstoqueId == lid);
        }
        if (filtro.FornecedorId is long fid)
        {
            q = q.Where(x => x.lo != null && x.lo.FornecedorId == fid);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Usuario))
        {
            var u = filtro.Usuario.Trim();
            q = q.Where(x => x.m.Usuario.Contains(u));
        }
        if (!string.IsNullOrWhiteSpace(filtro.Motivo))
        {
            var mo = filtro.Motivo.Trim();
            q = q.Where(x => x.m.Motivo != null && x.m.Motivo.Contains(mo));
        }
        if (!string.IsNullOrWhiteSpace(filtro.Origem))
        {
            q = filtro.Origem switch
            {
                "Compra" => q.Where(x => x.m.EntradaEstoqueId != null),
                "Ajuste" => q.Where(x => x.m.AjusteEstoqueId != null),
                "Producao" => q.Where(x => x.m.OrdemProducaoId != null),
                "Entrega" => q.Where(x => x.m.EntregaId != null),
                _ => q,
            };
        }

        var total = await q.CountAsync(cancellationToken);
        var pagina = filtro.Pagina < 1 ? 1 : filtro.Pagina;
        var tam = filtro.TamanhoPagina < 1 ? 50 : Math.Min(filtro.TamanhoPagina, 200);

        var rows = await q
            .OrderByDescending(x => x.m.DataHora).ThenByDescending(x => x.m.Id)
            .Skip((pagina - 1) * tam).Take(tam)
            .Select(x => new
            {
                x.m.Id,
                x.m.DataHora,
                x.m.ItemEstoqueId,
                ItemNome = x.i.Nome,
                x.i.Categoria,
                Unidade = x.i.UnidadeMedida,
                x.m.Tipo,
                x.m.Sentido,
                x.m.Quantidade,
                LoteCodigo = x.lo != null ? x.lo.Codigo : null,
                FornecedorId = x.lo != null ? x.lo.FornecedorId : null,
                x.m.CustoUnitario,
                x.m.ValorTotal,
                x.m.SaldoAnteriorItem,
                x.m.SaldoPosteriorItem,
                x.m.Usuario,
                x.m.Motivo,
                x.m.Observacao,
                x.m.EntradaEstoqueId,
                x.m.AjusteEstoqueId,
                x.m.OrdemProducaoId,
                x.m.EntregaId,
            })
            .ToListAsync(cancellationToken);

        var fornIds = rows.Where(r => r.FornecedorId != null).Select(r => r.FornecedorId!.Value).Distinct().ToList();
        var fornNomes = await _db.Fornecedores.Where(f => fornIds.Contains(f.Id)).ToDictionaryAsync(f => f.Id, f => f.Nome, cancellationToken);

        var itens = rows.Select(r => new MovimentacaoGeralDto(
            r.Id, r.DataHora, r.ItemEstoqueId, r.ItemNome, r.Categoria.ToString(), r.Tipo.ToString(), r.Sentido.ToString(),
            r.Quantidade, r.Unidade.ToString(), r.LoteCodigo, r.CustoUnitario, r.ValorTotal, r.SaldoAnteriorItem, r.SaldoPosteriorItem,
            r.Usuario, r.Motivo, r.Observacao,
            r.FornecedorId != null && fornNomes.TryGetValue(r.FornecedorId.Value, out var fn) ? fn : null,
            OrigemMovimentacao(r.EntradaEstoqueId, r.AjusteEstoqueId, r.OrdemProducaoId, r.EntregaId, r.Tipo))).ToList();

        return new MovimentacaoPaginaDto(total, itens);
    }

    public async Task<IReadOnlyList<EntradaCompraDto>> ListarEntradasAsync(FiltroEntradasRequest filtro, CancellationToken cancellationToken = default)
    {
        var q = from e in _db.EntradasEstoque
                join i in _db.ItensEstoque on e.ItemEstoqueId equals i.Id
                join lo in _db.LotesEstoque on e.LoteEstoqueId equals lo.Id
                select new { e, i, lo };

        if (filtro.DataInicio is DateOnly di)
        {
            q = q.Where(x => x.e.DataCompra >= di);
        }
        if (filtro.DataFim is DateOnly df)
        {
            q = q.Where(x => x.e.DataCompra <= df);
        }
        if (filtro.FornecedorId is long fid)
        {
            q = q.Where(x => x.e.FornecedorId == fid);
        }
        if (filtro.ItemEstoqueId is long iid)
        {
            q = q.Where(x => x.e.ItemEstoqueId == iid);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Categoria) && Enum.TryParse<CategoriaEstoque>(filtro.Categoria, out var cat))
        {
            q = q.Where(x => x.i.Categoria == cat);
        }
        if (filtro.LoteEstoqueId is long lid)
        {
            q = q.Where(x => x.e.LoteEstoqueId == lid);
        }
        if (!string.IsNullOrWhiteSpace(filtro.Usuario))
        {
            var u = filtro.Usuario.Trim();
            q = q.Where(x => x.e.Usuario.Contains(u));
        }
        if (filtro.ValorMin is decimal vmin)
        {
            q = q.Where(x => x.e.ValorTotal >= vmin);
        }
        if (filtro.ValorMax is decimal vmax)
        {
            q = q.Where(x => x.e.ValorTotal <= vmax);
        }
        if (filtro.ComFrete is bool cf)
        {
            q = cf ? q.Where(x => x.e.Frete > 0m) : q.Where(x => x.e.Frete == 0m);
        }

        var rows = await q
            .OrderByDescending(x => x.e.DataCompra).ThenByDescending(x => x.e.Id)
            .Select(x => new
            {
                x.e.Id,
                x.e.DataCompra,
                x.e.DataEntrada,
                x.e.FornecedorId,
                x.e.ItemEstoqueId,
                ItemNome = x.i.Nome,
                x.i.Categoria,
                x.e.Quantidade,
                Unidade = x.e.UnidadeMedida,
                x.e.ValorUnitario,
                x.e.Frete,
                x.e.FreteCompoeCusto,
                CustoEfetivo = x.lo.CustoUnitario,
                x.e.ValorTotal,
                LoteCodigo = x.lo.Codigo,
                x.lo.Validade,
                x.e.Usuario,
                x.e.Observacoes,
            })
            .ToListAsync(cancellationToken);

        var fornIds = rows.Where(r => r.FornecedorId != null).Select(r => r.FornecedorId!.Value).Distinct().ToList();
        var fornNomes = await _db.Fornecedores.Where(f => fornIds.Contains(f.Id)).ToDictionaryAsync(f => f.Id, f => f.Nome, cancellationToken);

        return rows.Select(r => new EntradaCompraDto(
            r.Id, r.DataCompra, r.DataEntrada, r.FornecedorId,
            r.FornecedorId != null && fornNomes.TryGetValue(r.FornecedorId.Value, out var fn) ? fn : null,
            r.ItemEstoqueId, r.ItemNome, r.Categoria.ToString(), r.Quantidade, r.Unidade.ToString(),
            r.ValorUnitario, r.CustoEfetivo, r.ValorTotal, r.Frete, r.ValorTotal + r.Frete, r.LoteCodigo, r.Validade,
            r.Usuario, r.Observacoes)).ToList();
    }

    private static string OrigemMovimentacao(long? entrada, long? ajuste, long? producao, long? entrega, TipoMovimentacao tipo)
    {
        if (entrada != null)
        {
            return "Compra";
        }
        if (ajuste != null)
        {
            return "Ajuste";
        }
        if (producao != null)
        {
            return "Produção";
        }
        if (entrega != null)
        {
            return "Entrega";
        }
        return tipo.ToString();
    }
}
