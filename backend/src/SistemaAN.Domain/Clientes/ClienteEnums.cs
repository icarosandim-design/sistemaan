namespace SistemaAN.Domain.Clientes;

public enum TipoCliente
{
    Assinante,
    Avulso,
}

public enum FormaPagamento
{
    Cartao,
    Pix,
    Dinheiro,
    Outro,
}

public enum StatusFinanceiro
{
    EmDia,
    Pendente,
    Inadimplente,
}
