using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Estoque;

/// <summary>
/// Movimentação de estoque — **livro-razão append-only**. É a fonte de verdade do
/// saldo: toda entrada, saída ou ajuste gera uma movimentação. Nunca se edita/exclui.
/// </summary>
public class MovimentacaoEstoque : AuditableEntity
{
    private MovimentacaoEstoque() { } // EF Core

    private MovimentacaoEstoque(
        ItemEstoque item,
        LoteEstoque? lote,
        TipoMovimentacao tipo,
        SentidoMovimentacao sentido,
        decimal quantidade,
        decimal saldoAnteriorItem,
        decimal saldoPosteriorItem,
        decimal custoUnitario,
        string usuario,
        DateTimeOffset dataHora,
        MotivoSaida? motivoCodigo,
        string? motivo,
        string? observacao)
    {
        Item = item;
        Lote = lote;
        Tipo = tipo;
        Sentido = sentido;
        Quantidade = quantidade;
        SaldoAnteriorItem = saldoAnteriorItem;
        SaldoPosteriorItem = saldoPosteriorItem;
        CustoUnitario = custoUnitario;
        ValorTotal = Math.Round(quantidade * custoUnitario, 2, MidpointRounding.AwayFromZero);
        Usuario = usuario;
        DataHora = dataHora;
        MotivoCodigo = motivoCodigo;
        Motivo = motivo;
        Observacao = observacao;
    }

    public long ItemEstoqueId { get; private set; }

    public ItemEstoque? Item { get; private set; }

    public long? LoteEstoqueId { get; private set; }

    public LoteEstoque? Lote { get; private set; }

    public TipoMovimentacao Tipo { get; private set; }

    public SentidoMovimentacao Sentido { get; private set; }

    public decimal Quantidade { get; private set; }

    public decimal SaldoAnteriorItem { get; private set; }

    public decimal SaldoPosteriorItem { get; private set; }

    public decimal CustoUnitario { get; private set; }

    public decimal ValorTotal { get; private set; }

    public string Usuario { get; private set; } = string.Empty;

    public DateTimeOffset DataHora { get; private set; }

    public MotivoSaida? MotivoCodigo { get; private set; }

    public string? Motivo { get; private set; }

    public string? Observacao { get; private set; }

    // Origem (rastreabilidade). Entrada/Ajuste têm FK; Produção/Entrega são id-only até existirem.
    public long? EntradaEstoqueId { get; private set; }

    public EntradaEstoque? Entrada { get; private set; }

    public long? AjusteEstoqueId { get; private set; }

    public AjusteEstoque? Ajuste { get; private set; }

    public long? OrdemProducaoId { get; private set; }

    public long? EntregaId { get; private set; }

    public static MovimentacaoEstoque CriarEntrada(
        ItemEstoque item,
        LoteEstoque? lote,
        TipoMovimentacao tipo,
        decimal quantidade,
        decimal saldoAnterior,
        decimal saldoPosterior,
        decimal custoUnitario,
        string usuario,
        DateTimeOffset dataHora,
        string? observacao)
        => new(item, lote, tipo, SentidoMovimentacao.Entrada, quantidade, saldoAnterior, saldoPosterior,
            custoUnitario, usuario, dataHora, null, null, observacao);

    public static MovimentacaoEstoque CriarSaida(
        ItemEstoque item,
        LoteEstoque? lote,
        TipoMovimentacao tipo,
        decimal quantidade,
        decimal saldoAnterior,
        decimal saldoPosterior,
        decimal custoUnitario,
        string usuario,
        DateTimeOffset dataHora,
        MotivoSaida? motivoCodigo,
        string? motivo,
        string? observacao)
        => new(item, lote, tipo, SentidoMovimentacao.Saida, quantidade, saldoAnterior, saldoPosterior,
            custoUnitario, usuario, dataHora, motivoCodigo, motivo, observacao);

    public void Vincular(EntradaEstoque entrada) => Entrada = entrada;

    public void Vincular(AjusteEstoque ajuste) => Ajuste = ajuste;

    public void VincularOrdemProducao(long ordemProducaoId) => OrdemProducaoId = ordemProducaoId;

    public void VincularEntrega(long entregaId) => EntregaId = entregaId;
}
