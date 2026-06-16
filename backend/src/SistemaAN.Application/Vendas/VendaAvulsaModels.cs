namespace SistemaAN.Application.Vendas;

/// <summary>Item de uma venda avulsa PF (Receita da Casa / Produto Acabado).</summary>
public sealed record SalvarVendaAvulsaItemRequest(
    long ReceitaId,
    long TamanhoPacoteId,
    int Quantidade,
    string? Observacao);

/// <summary>
/// Venda única para cliente PF, sem recorrência. Gera uma Entrega Programada.
/// Valor e forma de pagamento são informativos (sem módulo financeiro).
/// </summary>
public sealed record SalvarVendaAvulsaRequest(
    long ClienteId,
    long PetId,
    DateOnly DataEntrega,
    string? Observacoes,
    decimal? Valor,
    string? FormaPagamento,
    IReadOnlyList<SalvarVendaAvulsaItemRequest> Itens);

public sealed record VendaAvulsaResultadoDto(
    long EntregaId,
    DateOnly DataEntrega,
    string Status,
    int TotalPacotes);
