using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Estoque;

/// <summary>
/// Cabeçalho de uma compra/nota: agrupa uma ou mais <see cref="EntradaEstoque"/> e
/// carrega os dados fiscais/financeiros da nota (NF, vencimento, forma de pagamento,
/// boleto, totais). A futura Conta a Pagar deve nascer 1 por NotaCompra (não por item).
/// </summary>
public class NotaCompra : AuditableEntity
{
    private NotaCompra() { } // EF Core

    private NotaCompra(DadosNotaCompra d)
    {
        Aplicar(d);
    }

    public long? FornecedorId { get; private set; }
    public DateOnly DataCompra { get; private set; }
    public DateOnly DataEntrada { get; private set; }

    // ----- Nota fiscal recebida (opcional) -----
    public string? NumeroNotaFiscal { get; private set; }
    public string? SerieNotaFiscal { get; private set; }
    public string? ChaveAcessoNotaFiscal { get; private set; }
    public DateOnly? DataEmissaoNotaFiscal { get; private set; }

    // ----- Pagamento (opcional; base para a futura Conta a Pagar) -----
    public DateOnly? DataVencimentoPagamento { get; private set; }
    public string? FormaPagamento { get; private set; }
    public string? CondicaoPagamento { get; private set; }
    public string? LinhaDigitavelBoleto { get; private set; }
    public string? CodigoBarrasBoleto { get; private set; }
    public string? BancoEmissorBoleto { get; private set; }
    public string? NumeroDocumento { get; private set; }

    // ----- Totais -----
    public decimal ValorProdutos { get; private set; }
    public decimal Frete { get; private set; }
    public decimal Desconto { get; private set; }
    public decimal Acrescimo { get; private set; }
    public decimal ValorTotal { get; private set; }

    public string? Observacoes { get; private set; }

    public static NotaCompra Criar(DadosNotaCompra dados) => new(dados);

    public void DefinirTotais(decimal valorProdutos, decimal frete, decimal desconto, decimal acrescimo)
    {
        ValorProdutos = valorProdutos;
        Frete = frete;
        Desconto = desconto;
        Acrescimo = acrescimo;
        ValorTotal = valorProdutos + frete + acrescimo - desconto;
    }

    private void Aplicar(DadosNotaCompra d)
    {
        FornecedorId = d.FornecedorId;
        DataCompra = d.DataCompra;
        DataEntrada = d.DataEntrada;
        NumeroNotaFiscal = Texto(d.NumeroNotaFiscal);
        SerieNotaFiscal = Texto(d.SerieNotaFiscal);
        ChaveAcessoNotaFiscal = Texto(d.ChaveAcessoNotaFiscal);
        DataEmissaoNotaFiscal = d.DataEmissaoNotaFiscal;
        DataVencimentoPagamento = d.DataVencimentoPagamento;
        FormaPagamento = Texto(d.FormaPagamento);
        CondicaoPagamento = Texto(d.CondicaoPagamento);
        LinhaDigitavelBoleto = Texto(d.LinhaDigitavelBoleto);
        CodigoBarrasBoleto = Texto(d.CodigoBarrasBoleto);
        BancoEmissorBoleto = Texto(d.BancoEmissorBoleto);
        NumeroDocumento = Texto(d.NumeroDocumento);
        Observacoes = Texto(d.Observacoes);
        DefinirTotais(d.ValorProdutos, d.Frete, d.Desconto, d.Acrescimo);
    }

    private static string? Texto(string? v) => string.IsNullOrWhiteSpace(v) ? null : v.Trim();
}

public sealed record DadosNotaCompra(
    long? FornecedorId,
    DateOnly DataCompra,
    DateOnly DataEntrada,
    string? NumeroNotaFiscal,
    string? SerieNotaFiscal,
    string? ChaveAcessoNotaFiscal,
    DateOnly? DataEmissaoNotaFiscal,
    DateOnly? DataVencimentoPagamento,
    string? FormaPagamento,
    string? CondicaoPagamento,
    string? LinhaDigitavelBoleto,
    string? CodigoBarrasBoleto,
    string? BancoEmissorBoleto,
    string? NumeroDocumento,
    decimal ValorProdutos,
    decimal Frete,
    decimal Desconto,
    decimal Acrescimo,
    string? Observacoes);
