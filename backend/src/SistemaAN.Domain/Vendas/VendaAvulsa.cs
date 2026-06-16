using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Vendas;

public enum StatusVendaAvulsa
{
    Rascunho,
    Confirmada,
    Cancelada,
}

/// <summary>
/// Venda avulsa PF: cabeçalho comercial da venda (fonte oficial do valor e da forma
/// de pagamento). A Entrega gerada a partir dela é apenas a operação/logística.
/// Os itens vendidos ficam em <see cref="VendaAvulsaItem"/>.
/// </summary>
public class VendaAvulsa : AuditableEntity
{
    private readonly List<VendaAvulsaItem> _itens = [];

    private VendaAvulsa() { } // EF Core

    private VendaAvulsa(long clienteId, long? petId, DateOnly dataVenda, string? formaPagamento, string? observacoes)
    {
        ClienteId = clienteId;
        PetId = petId;
        DataVenda = dataVenda;
        FormaPagamento = string.IsNullOrWhiteSpace(formaPagamento) ? null : formaPagamento.Trim();
        Observacoes = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
        Status = StatusVendaAvulsa.Confirmada;
        ValorTotal = 0m;
    }

    public long ClienteId { get; private set; }
    public long? PetId { get; private set; }
    public long? EntregaId { get; private set; }
    public DateOnly DataVenda { get; private set; }
    public decimal ValorTotal { get; private set; }
    public string? FormaPagamento { get; private set; }
    public StatusVendaAvulsa Status { get; private set; }
    public string? Observacoes { get; private set; }

    public IReadOnlyCollection<VendaAvulsaItem> Itens => _itens.AsReadOnly();

    public static VendaAvulsa Criar(long clienteId, long? petId, DateOnly dataVenda, string? formaPagamento, string? observacoes)
        => new(clienteId, petId, dataVenda, formaPagamento, observacoes);

    public void AdicionarItem(VendaAvulsaItem item) => _itens.Add(item);

    public void RecalcularTotal() => ValorTotal = _itens.Sum(i => i.ValorTotal);

    public void VincularEntrega(long entregaId) => EntregaId = entregaId;

    public void Cancelar() => Status = StatusVendaAvulsa.Cancelada;
}
