using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Estoque;

/// <summary>
/// Entrada de estoque (compra simplificada por item/lote). Registra os dados
/// comerciais da compra e gera um <see cref="LoteEstoque"/> + uma
/// <see cref="MovimentacaoEstoque"/>. Preparada para evoluir para um cabeçalho de
/// compra com vários itens (Compras/Financeiro) no futuro.
/// </summary>
public class EntradaEstoque : AuditableEntity
{
    private EntradaEstoque() { } // EF Core

    private EntradaEstoque(
        ItemEstoque item,
        LoteEstoque lote,
        long? fornecedorId,
        decimal quantidade,
        UnidadeMedida unidadeMedida,
        decimal valorUnitario,
        decimal valorTotal,
        DateOnly dataCompra,
        DateOnly dataEntrada,
        DateOnly? validade,
        string loteCodigo,
        string? localArmazenamento,
        string usuario,
        string? observacoes)
    {
        Item = item;
        Lote = lote;
        FornecedorId = fornecedorId;
        Quantidade = quantidade;
        UnidadeMedida = unidadeMedida;
        ValorUnitario = valorUnitario;
        ValorTotal = valorTotal;
        DataCompra = dataCompra;
        DataEntrada = dataEntrada;
        Validade = validade;
        LoteCodigo = loteCodigo;
        LocalArmazenamento = localArmazenamento;
        Usuario = usuario;
        Observacoes = observacoes;
    }

    public long ItemEstoqueId { get; private set; }

    public ItemEstoque? Item { get; private set; }

    public long LoteEstoqueId { get; private set; }

    public LoteEstoque? Lote { get; private set; }

    public long? FornecedorId { get; private set; }

    public decimal Quantidade { get; private set; }

    public UnidadeMedida UnidadeMedida { get; private set; }

    public decimal ValorUnitario { get; private set; }

    public decimal ValorTotal { get; private set; }

    public DateOnly DataCompra { get; private set; }

    public DateOnly DataEntrada { get; private set; }

    public DateOnly? Validade { get; private set; }

    public string LoteCodigo { get; private set; } = string.Empty;

    public string? LocalArmazenamento { get; private set; }

    public string Usuario { get; private set; } = string.Empty;

    public string? Observacoes { get; private set; }

    public static EntradaEstoque Criar(
        ItemEstoque item,
        LoteEstoque lote,
        long? fornecedorId,
        decimal quantidade,
        UnidadeMedida unidadeMedida,
        decimal valorUnitario,
        decimal valorTotal,
        DateOnly dataCompra,
        DateOnly dataEntrada,
        DateOnly? validade,
        string loteCodigo,
        string? localArmazenamento,
        string usuario,
        string? observacoes)
        => new(item, lote, fornecedorId, quantidade, unidadeMedida, valorUnitario, valorTotal,
            dataCompra, dataEntrada, validade, loteCodigo.Trim(), localArmazenamento, usuario, observacoes);
}
