namespace SistemaAN.Domain.Pedidos;

/// <summary>
/// Status do Pedido PJ (comercial). A logística é controlada pela Entrega vinculada,
/// não aqui — por isso o ciclo é enxuto.
/// </summary>
public enum StatusPedido
{
    Rascunho,
    Confirmado,
    Cancelado,
}
