using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Pedidos;

/// <summary>
/// Pedido PJ — a venda de produto acabado (Receitas da Casa) para um Cliente PJ.
/// Pedido = o que foi vendido. A Entrega vinculada cuida da logística.
/// </summary>
public class Pedido : AuditableEntity
{
    private readonly List<PedidoItem> _itens = [];

    private Pedido() { } // EF Core

    private Pedido(long clienteId, string clienteNome, DateOnly dataPedido, DateOnly dataEntrega, string? observacoes)
    {
        ClienteId = clienteId;
        ClienteNome = clienteNome;
        DataPedido = dataPedido;
        DataEntrega = dataEntrega;
        Observacoes = observacoes;
        Status = StatusPedido.Rascunho;
    }

    public long ClienteId { get; private set; }
    public string ClienteNome { get; private set; } = string.Empty; // snapshot (razão/fantasia)
    public DateOnly DataPedido { get; private set; }
    public DateOnly DataEntrega { get; private set; }
    public StatusPedido Status { get; private set; }
    public string? Observacoes { get; private set; }
    public decimal? ValorTotal { get; private set; } // futuro (preço)

    public long? EntregaId { get; private set; }

    public IReadOnlyCollection<PedidoItem> Itens => _itens.AsReadOnly();

    public bool EhRascunho => Status == StatusPedido.Rascunho;
    public bool EhConfirmado => Status == StatusPedido.Confirmado;
    public bool EhCancelado => Status == StatusPedido.Cancelado;

    public static Pedido Criar(long clienteId, string clienteNome, DateOnly dataPedido, DateOnly dataEntrega, string? observacoes)
        => new(clienteId, clienteNome, dataPedido, dataEntrega, observacoes?.Trim());

    public void Atualizar(DateOnly dataPedido, DateOnly dataEntrega, string? observacoes)
    {
        DataPedido = dataPedido;
        DataEntrega = dataEntrega;
        Observacoes = observacoes?.Trim();
    }

    public void LimparItens() => _itens.Clear();

    public void AdicionarItem(PedidoItem item) => _itens.Add(item);

    /// <summary>Recalcula o valor total do pedido pela soma dos itens (preço × quantidade).</summary>
    public void RecalcularTotal()
    {
        var temPreco = _itens.Any(i => i.PrecoUnitario.HasValue);
        ValorTotal = temPreco ? _itens.Sum(i => (i.PrecoUnitario ?? 0m) * i.Quantidade) : null;
    }

    public void Confirmar() => Status = StatusPedido.Confirmado;

    public void Cancelar() => Status = StatusPedido.Cancelado;

    public void VincularEntrega(long entregaId) => EntregaId = entregaId;

    public void DesvincularEntrega() => EntregaId = null;
}
