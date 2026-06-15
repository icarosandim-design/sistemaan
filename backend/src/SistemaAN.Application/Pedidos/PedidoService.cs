using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Clientes;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Pedidos;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Pedidos;

public sealed class PedidoService : IPedidoService
{
    private readonly IApplicationDbContext _db;

    public PedidoService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<PedidoResumoDto>> ListarPorClienteAsync(long clienteId, CancellationToken cancellationToken = default)
    {
        var pedidos = await _db.Pedidos.Include(p => p.Itens)
            .Where(p => p.ClienteId == clienteId)
            .OrderByDescending(p => p.DataPedido).ThenByDescending(p => p.Id)
            .ToListAsync(cancellationToken);

        var entregaStatus = await CarregarEntregaStatusAsync(pedidos, cancellationToken);

        return pedidos.Select(p => new PedidoResumoDto(
            p.Id, p.ClienteId, p.DataPedido, p.DataEntrega, p.Status.ToString(),
            p.Itens.Count, p.Itens.Sum(i => i.Quantidade), p.EntregaId,
            p.EntregaId is { } eid && entregaStatus.TryGetValue(eid, out var s) ? s : null)).ToList();
    }

    public async Task<PedidoDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var pedido = await _db.Pedidos.Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pedido", id);
        return await MapAsync(pedido, cancellationToken);
    }

    public async Task<PedidoDto> CriarAsync(SalvarPedidoRequest request, CancellationToken cancellationToken = default)
    {
        var (cliente, pj) = await CarregarClientePjAsync(request.ClienteId, cancellationToken);
        var itens = await MontarItensAsync(request.Itens, cancellationToken);

        var nome = NomeSnapshot(cliente, pj);
        var pedido = Pedido.Criar(cliente.Id, nome, request.DataPedido, request.DataEntrega, request.Observacoes);
        foreach (var it in itens)
        {
            pedido.AdicionarItem(it);
        }

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(pedido.Id, cancellationToken);
    }

    public async Task<PedidoDto> AtualizarAsync(long id, SalvarPedidoRequest request, CancellationToken cancellationToken = default)
    {
        var pedido = await _db.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pedido", id);

        if (pedido.EhCancelado)
        {
            throw Erro("pedido", "Pedido cancelado não pode ser editado.");
        }

        Entrega? entrega = null;
        if (pedido.EhConfirmado)
        {
            entrega = pedido.EntregaId is { } eid
                ? await _db.Entregas.Include(e => e.Pets).ThenInclude(p => p.Itens).ThenInclude(i => i.Pacotes)
                    .FirstOrDefaultAsync(e => e.Id == eid, cancellationToken)
                : null;
            if (entrega is null || entrega.Status != EntregaStatus.Programada)
            {
                throw Erro("pedido", "Só é possível editar enquanto a entrega estiver Programada.");
            }
        }

        var itens = await MontarItensAsync(request.Itens, cancellationToken);
        pedido.Atualizar(request.DataPedido, request.DataEntrega, request.Observacoes);
        pedido.LimparItens();
        foreach (var it in itens)
        {
            pedido.AdicionarItem(it);
        }

        if (entrega is not null)
        {
            AtualizarEntrega(entrega, pedido);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(id, cancellationToken);
    }

    public async Task<PedidoDto> ConfirmarAsync(long id, string usuario, CancellationToken cancellationToken = default)
    {
        var pedido = await _db.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pedido", id);

        if (pedido.EhCancelado)
        {
            throw Erro("pedido", "Pedido cancelado não pode ser confirmado.");
        }
        if (pedido.EhConfirmado)
        {
            return await ObterAsync(id, cancellationToken); // idempotente
        }
        if (pedido.Itens.Count == 0)
        {
            throw Erro("itens", "Inclua ao menos um item antes de confirmar.");
        }

        var (cliente, pj) = await CarregarClientePjAsync(pedido.ClienteId, cancellationToken);
        var entrega = ConstruirEntrega(pedido, cliente, pj);
        entrega.VincularPedido(pedido.Id);
        entrega.RegistrarHistorico(usuario, "Entrega gerada a partir do Pedido PJ", null, EntregaStatus.Programada);
        _db.Entregas.Add(entrega);
        await _db.SaveChangesAsync(cancellationToken);

        pedido.VincularEntrega(entrega.Id);
        pedido.Confirmar();
        await _db.SaveChangesAsync(cancellationToken);

        return await ObterAsync(id, cancellationToken);
    }

    public async Task<PedidoDto> CancelarAsync(long id, string? motivo, string usuario, CancellationToken cancellationToken = default)
    {
        var pedido = await _db.Pedidos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Pedido", id);

        if (pedido.EhCancelado)
        {
            return await ObterAsync(id, cancellationToken);
        }

        if (pedido.EntregaId is { } eid)
        {
            var entrega = await _db.Entregas.Include(e => e.Historico).FirstOrDefaultAsync(e => e.Id == eid, cancellationToken);
            if (entrega is not null && entrega.Status != EntregaStatus.Cancelada && entrega.Status != EntregaStatus.Entregue)
            {
                entrega.Cancelar(motivo?.Trim() is { Length: > 0 } m ? m : "Pedido PJ cancelado", usuario);
            }
        }

        pedido.Cancelar();
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(id, cancellationToken);
    }

    // ===================== Helpers =====================
    private async Task<(Cliente Cliente, ClientePj Pj)> CarregarClientePjAsync(long clienteId, CancellationToken ct)
    {
        var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Id == clienteId, ct)
            ?? throw new NotFoundException("Cliente", clienteId);
        if (cliente.Natureza != NaturezaCliente.PessoaJuridica)
        {
            throw Erro("clienteId", "O pedido deve ser de um Cliente Pessoa Jurídica.");
        }
        var pj = await _db.ClientesPj.FirstOrDefaultAsync(p => p.ClienteId == clienteId, ct)
            ?? throw Erro("clienteId", "Dados de Pessoa Jurídica não encontrados para este cliente.");
        return (cliente, pj);
    }

    private async Task<List<PedidoItem>> MontarItensAsync(IReadOnlyList<SalvarPedidoItemRequest> reqs, CancellationToken ct)
    {
        if (reqs is null || reqs.Count == 0)
        {
            throw Erro("itens", "Inclua ao menos um item no pedido.");
        }

        var receitaIds = reqs.Select(r => r.ReceitaId).Distinct().ToList();
        var tamanhoIds = reqs.Select(r => r.TamanhoPacoteId).Distinct().ToList();
        var receitas = await _db.Receitas.Where(r => receitaIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, ct);
        var tamanhos = await _db.TamanhosPacote.Where(t => tamanhoIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, ct);
        var produtoAcabado = await _db.ItensEstoque.AsNoTracking()
            .Where(x => x.Tipo == TipoItemEstoque.ProdutoAcabadoCasa)
            .Select(x => new { x.Id, x.ReceitaId, x.TamanhoPacoteId })
            .ToListAsync(ct);

        var itens = new List<PedidoItem>();
        var erros = new Dictionary<string, string[]>();
        for (var idx = 0; idx < reqs.Count; idx++)
        {
            var r = reqs[idx];
            if (!receitas.TryGetValue(r.ReceitaId, out var rec) || rec.Tipo != TipoReceita.Casa)
            {
                erros[$"itens[{idx}].receitaId"] = ["Selecione uma Receita da Casa válida."];
                continue;
            }
            if (!tamanhos.TryGetValue(r.TamanhoPacoteId, out var tam))
            {
                erros[$"itens[{idx}].tamanhoPacoteId"] = ["Selecione um tamanho de pacote válido."];
                continue;
            }
            if (r.Quantidade <= 0)
            {
                erros[$"itens[{idx}].quantidade"] = ["A quantidade deve ser maior que zero."];
                continue;
            }
            var itemEstoqueId = produtoAcabado.FirstOrDefault(x => x.ReceitaId == r.ReceitaId && x.TamanhoPacoteId == r.TamanhoPacoteId)?.Id;
            itens.Add(PedidoItem.Criar(rec.Id, rec.Codigo, rec.Nome, tam.Id, tam.Nome, tam.PesoGramas,
                r.Quantidade, null, string.IsNullOrWhiteSpace(r.Observacao) ? null : r.Observacao.Trim(), itemEstoqueId));
        }
        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }
        return itens;
    }

    private static Entrega ConstruirEntrega(Pedido pedido, Cliente cliente, ClientePj pj)
    {
        var nome = NomeSnapshot(cliente, pj);
        var snapshot = new DadosSnapshotEntrega(
            nome,
            cliente.Telefone,
            pj.EntregaRua ?? cliente.Rua,
            pj.EntregaNumero ?? cliente.Numero,
            pj.EntregaComplemento ?? cliente.Complemento,
            pj.EntregaCep ?? cliente.Cep,
            pj.EntregaBairro ?? cliente.Bairro,
            pj.EntregaCidade ?? cliente.Cidade,
            pj.EntregaEstado ?? cliente.Estado,
            "Pedido PJ",
            0,
            cliente.PreferenciaHorario);

        var entrega = Entrega.Criar(pedido.ClienteId, pedido.DataEntrega, snapshot);
        entrega.AdicionarPet(ContainerDoPedido(pedido, nome));
        return entrega;
    }

    private static void AtualizarEntrega(Entrega entrega, Pedido pedido)
    {
        entrega.AlterarDataPrevista(pedido.DataEntrega);
        entrega.LimparPets();
        entrega.AdicionarPet(ContainerDoPedido(pedido, entrega.ClienteNome));
    }

    /// <summary>Grupo "container" (EntregaPet sem pet) com os itens de Casa do pedido.</summary>
    private static EntregaPet ContainerDoPedido(Pedido pedido, string nome)
    {
        var entregaItens = pedido.Itens.Select(it =>
            EntregaItem.CriarCasa(
                it.ReceitaId, it.ReceitaCodigo, it.ReceitaNome,
                it.Quantidade * it.PesoGramas,
                new[] { EntregaItemPacote.Criar(it.TamanhoNome, it.PesoGramas, it.Quantidade) }))
            .ToList();
        var total = pedido.Itens.Sum(it => it.Quantidade * it.PesoGramas);
        return EntregaPet.Criar(null, nome, TipoReceita.Casa, null, total, entregaItens);
    }

    private async Task<Dictionary<long, string>> CarregarEntregaStatusAsync(List<Pedido> pedidos, CancellationToken ct)
    {
        var ids = pedidos.Where(p => p.EntregaId != null).Select(p => p.EntregaId!.Value).Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, string>();
        }
        return await _db.Entregas.Where(e => ids.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Status.ToString(), ct);
    }

    private async Task<PedidoDto> MapAsync(Pedido p, CancellationToken ct)
    {
        string? entregaStatus = null;
        if (p.EntregaId is { } eid)
        {
            entregaStatus = await _db.Entregas.Where(e => e.Id == eid).Select(e => e.Status.ToString()).FirstOrDefaultAsync(ct);
        }
        return new PedidoDto(
            p.Id, p.ClienteId, p.ClienteNome, p.DataPedido, p.DataEntrega, p.Status.ToString(), p.Observacoes,
            p.EntregaId, entregaStatus, p.Itens.Sum(i => i.Quantidade),
            p.Itens.OrderBy(i => i.Id).Select(i => new PedidoItemDto(
                i.Id, i.ReceitaId, i.ReceitaCodigo, i.ReceitaNome, i.TamanhoPacoteId, i.TamanhoNome, i.PesoGramas, i.Quantidade, i.Observacao)).ToList());
    }

    private static string NomeSnapshot(Cliente cliente, ClientePj pj)
        => string.IsNullOrWhiteSpace(pj.NomeFantasia) ? cliente.Nome : pj.NomeFantasia;

    private static ValidationException Erro(string campo, string msg)
        => new(new Dictionary<string, string[]> { [campo] = [msg] });
}
