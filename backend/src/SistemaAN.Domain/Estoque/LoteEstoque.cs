using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Estoque;

/// <summary>
/// Lote de estoque: unidade de controle de validade, custo de aquisição e PEPS/FIFO.
/// Cada compra (ou produção) gera um lote com saldo próprio.
/// </summary>
public class LoteEstoque : AuditableEntity
{
    private LoteEstoque() { } // EF Core

    private LoteEstoque(
        ItemEstoque item,
        string codigo,
        DateOnly dataEntrada,
        DateOnly? validade,
        decimal quantidadeInicial,
        decimal custoUnitario,
        long? fornecedorId,
        OrigemLote origem,
        long? origemId)
    {
        Item = item;
        Codigo = codigo;
        DataEntrada = dataEntrada;
        Validade = validade;
        QuantidadeInicial = quantidadeInicial;
        QuantidadeAtual = quantidadeInicial;
        CustoUnitario = custoUnitario;
        FornecedorId = fornecedorId;
        Origem = origem;
        OrigemId = origemId;
        Status = StatusLote.Ativo;
    }

    public long ItemEstoqueId { get; private set; }

    public ItemEstoque? Item { get; private set; }

    public string Codigo { get; private set; } = string.Empty;

    public DateOnly DataEntrada { get; private set; }

    public DateOnly? Validade { get; private set; }

    public decimal QuantidadeInicial { get; private set; }

    public decimal QuantidadeAtual { get; private set; }

    public decimal CustoUnitario { get; private set; }

    public long? FornecedorId { get; private set; }

    public OrigemLote Origem { get; private set; }

    /// <summary>Id da origem (EntradaEstoque/OrdemProducao). Sem FK nesta fase.</summary>
    public long? OrigemId { get; private set; }

    public StatusLote Status { get; private set; }

    public static LoteEstoque Criar(
        ItemEstoque item,
        string codigo,
        DateOnly dataEntrada,
        DateOnly? validade,
        decimal quantidadeInicial,
        decimal custoUnitario,
        long? fornecedorId,
        OrigemLote origem,
        long? origemId)
        => new(item, codigo.Trim(), dataEntrada, validade, quantidadeInicial, custoUnitario, fornecedorId, origem, origemId);

    /// <summary>Consome do lote (FIFO). Zera ⇒ Esgotado.</summary>
    public void Consumir(decimal quantidade)
    {
        if (quantidade <= 0m)
        {
            throw new InvalidOperationException("Quantidade consumida deve ser maior que zero.");
        }

        if (quantidade > QuantidadeAtual)
        {
            throw new InvalidOperationException("Consumo maior que o saldo do lote.");
        }

        QuantidadeAtual -= quantidade;
        if (QuantidadeAtual == 0m && Status == StatusLote.Ativo)
        {
            Status = StatusLote.Esgotado;
        }
    }

    /// <summary>Define o saldo do lote por ajuste manual.</summary>
    public void AjustarSaldo(decimal novaQuantidade)
    {
        if (novaQuantidade < 0m)
        {
            throw new InvalidOperationException("Saldo do lote não pode ser negativo.");
        }

        QuantidadeAtual = novaQuantidade;
        if (QuantidadeAtual == 0m && Status == StatusLote.Ativo)
        {
            Status = StatusLote.Esgotado;
        }
        else if (QuantidadeAtual > 0m && Status == StatusLote.Esgotado)
        {
            Status = StatusLote.Ativo;
        }
    }

    public void DefinirStatus(StatusLote status) => Status = status;
}
