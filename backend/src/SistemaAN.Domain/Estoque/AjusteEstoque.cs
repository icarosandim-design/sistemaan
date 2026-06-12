using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Estoque;

/// <summary>
/// Ajuste manual de estoque. Toda divergência física é corrigida por aqui, com
/// motivo obrigatório e histórico. Gera uma <see cref="MovimentacaoEstoque"/>.
/// </summary>
public class AjusteEstoque : AuditableEntity
{
    private AjusteEstoque() { } // EF Core

    private AjusteEstoque(
        ItemEstoque item,
        LoteEstoque? lote,
        decimal quantidadeAnterior,
        decimal quantidadeNova,
        string motivo,
        string usuario,
        DateTimeOffset dataHora,
        string? observacao)
    {
        Item = item;
        Lote = lote;
        QuantidadeAnterior = quantidadeAnterior;
        QuantidadeNova = quantidadeNova;
        Diferenca = quantidadeNova - quantidadeAnterior;
        Motivo = motivo;
        Usuario = usuario;
        DataHora = dataHora;
        Observacao = observacao;
    }

    public long ItemEstoqueId { get; private set; }

    public ItemEstoque? Item { get; private set; }

    public long? LoteEstoqueId { get; private set; }

    public LoteEstoque? Lote { get; private set; }

    public decimal QuantidadeAnterior { get; private set; }

    public decimal QuantidadeNova { get; private set; }

    public decimal Diferenca { get; private set; }

    public string Motivo { get; private set; } = string.Empty;

    public string Usuario { get; private set; } = string.Empty;

    public DateTimeOffset DataHora { get; private set; }

    public string? Observacao { get; private set; }

    public static AjusteEstoque Criar(
        ItemEstoque item,
        LoteEstoque? lote,
        decimal quantidadeAnterior,
        decimal quantidadeNova,
        string motivo,
        string usuario,
        DateTimeOffset dataHora,
        string? observacao)
        => new(item, lote, quantidadeAnterior, quantidadeNova, motivo.Trim(), usuario, dataHora, observacao);
}
