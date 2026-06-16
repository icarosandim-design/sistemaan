using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Pedidos;

/// <summary>Item do Pedido PJ — produto acabado da Receita da Casa (receita + tamanho).</summary>
public class PedidoItem : Entity
{
    private PedidoItem() { } // EF Core

    private PedidoItem(
        long receitaId, string receitaCodigo, string receitaNome,
        long tamanhoPacoteId, string tamanhoNome, int pesoGramas,
        int quantidade, decimal? precoUnitario, string? observacao, long? itemEstoqueId, long? produtoId)
    {
        ReceitaId = receitaId;
        ReceitaCodigo = receitaCodigo;
        ReceitaNome = receitaNome;
        TamanhoPacoteId = tamanhoPacoteId;
        TamanhoNome = tamanhoNome;
        PesoGramas = pesoGramas;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        Observacao = observacao;
        ItemEstoqueId = itemEstoqueId;
        ProdutoId = produtoId;
    }

    public long PedidoId { get; private set; }
    public long ReceitaId { get; private set; }
    public string ReceitaCodigo { get; private set; } = string.Empty;
    public string ReceitaNome { get; private set; } = string.Empty;
    public long TamanhoPacoteId { get; private set; }
    public string TamanhoNome { get; private set; } = string.Empty;
    public int PesoGramas { get; private set; }
    public int Quantidade { get; private set; }
    public decimal? PrecoUnitario { get; private set; }
    public string? Observacao { get; private set; }

    /// <summary>Produto acabado de estoque correspondente (Receita+tamanho), se existir.</summary>
    public long? ItemEstoqueId { get; private set; }

    /// <summary>Produto comercial vendido (camada nova). Opcional para compatibilidade.</summary>
    public long? ProdutoId { get; private set; }

    /// <summary>Valor total do item = preço unitário × quantidade (quando há preço).</summary>
    public decimal? ValorTotalItem => PrecoUnitario.HasValue ? PrecoUnitario.Value * Quantidade : null;

    public static PedidoItem Criar(
        long receitaId, string receitaCodigo, string receitaNome,
        long tamanhoPacoteId, string tamanhoNome, int pesoGramas,
        int quantidade, decimal? precoUnitario, string? observacao, long? itemEstoqueId, long? produtoId = null)
        => new(receitaId, receitaCodigo, receitaNome, tamanhoPacoteId, tamanhoNome, pesoGramas,
            quantidade, precoUnitario, observacao, itemEstoqueId, produtoId);
}
