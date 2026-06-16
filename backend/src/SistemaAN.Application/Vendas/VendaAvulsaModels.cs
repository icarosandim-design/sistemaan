namespace SistemaAN.Application.Vendas;

/// <summary>Item de uma venda avulsa PF: Produto + quantidade + preço aplicado.</summary>
public sealed record SalvarVendaAvulsaItemRequest(
    long ProdutoId,
    int Quantidade,
    decimal? PrecoUnitario,
    string? Observacao);

/// <summary>
/// Venda única para cliente PF, sem recorrência. Cria a VendaAvulsa (fonte comercial,
/// com valor estruturado) e gera uma Entrega Programada (operação/logística).
/// </summary>
public sealed record SalvarVendaAvulsaRequest(
    long ClienteId,
    long PetId,
    DateOnly DataEntrega,
    string? Observacoes,
    string? FormaPagamento,
    IReadOnlyList<SalvarVendaAvulsaItemRequest> Itens);

public sealed record VendaAvulsaResultadoDto(
    long VendaAvulsaId,
    long EntregaId,
    DateOnly DataEntrega,
    string Status,
    decimal ValorTotal,
    int TotalPacotes);
