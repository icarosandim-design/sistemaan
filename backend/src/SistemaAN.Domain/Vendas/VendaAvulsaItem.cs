using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Vendas;

/// <summary>Item de uma venda avulsa PF: Produto + quantidade + preço aplicado.</summary>
public class VendaAvulsaItem : Entity
{
    private VendaAvulsaItem() { } // EF Core

    private VendaAvulsaItem(long produtoId, int quantidade, decimal precoUnitario, string? observacao)
    {
        ProdutoId = produtoId;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        ValorTotal = precoUnitario * quantidade;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
    }

    public long VendaAvulsaId { get; private set; }
    public long ProdutoId { get; private set; }
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal ValorTotal { get; private set; }
    public string? Observacao { get; private set; }

    public static VendaAvulsaItem Criar(long produtoId, int quantidade, decimal precoUnitario, string? observacao)
        => new(produtoId, quantidade, precoUnitario, observacao);
}
