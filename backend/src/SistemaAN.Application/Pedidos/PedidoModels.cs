namespace SistemaAN.Application.Pedidos;

public sealed record PedidoItemDto(
    long Id,
    long ReceitaId,
    string ReceitaCodigo,
    string ReceitaNome,
    long TamanhoPacoteId,
    string TamanhoNome,
    int PesoGramas,
    int Quantidade,
    string? Observacao,
    long? ProdutoId,
    decimal? PrecoUnitario,
    decimal? ValorTotalItem);

public sealed record PedidoDto(
    long Id,
    long ClienteId,
    string ClienteNome,
    DateOnly DataPedido,
    DateOnly DataEntrega,
    string Status,
    string? Observacoes,
    long? EntregaId,
    string? EntregaStatus,
    int TotalPacotes,
    decimal? ValorTotal,
    IReadOnlyList<PedidoItemDto> Itens);

public sealed record PedidoResumoDto(
    long Id,
    long ClienteId,
    DateOnly DataPedido,
    DateOnly DataEntrega,
    string Status,
    int TotalItens,
    int TotalPacotes,
    long? EntregaId,
    string? EntregaStatus);

public sealed record SalvarPedidoItemRequest(
    long ReceitaId,
    long TamanhoPacoteId,
    int Quantidade,
    string? Observacao,
    long? ProdutoId = null,
    decimal? PrecoUnitario = null);

public sealed record SalvarPedidoRequest(
    long ClienteId,
    DateOnly DataPedido,
    DateOnly DataEntrega,
    string? Observacoes,
    IReadOnlyList<SalvarPedidoItemRequest> Itens);

public sealed record CancelarPedidoRequest(string? Motivo);
