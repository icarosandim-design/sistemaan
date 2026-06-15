namespace SistemaAN.Domain.Clientes;

/// <summary>Natureza do cliente: pessoa física (tutor/pets) ou jurídica (PJ/pedidos).</summary>
public enum NaturezaCliente
{
    PessoaFisica,
    PessoaJuridica,
}

/// <summary>Segmento do cliente PJ.</summary>
public enum TipoClientePJ
{
    Mercado,
    PetShop,
    ClinicaVeterinaria,
    Revendedor,
    Parceiro,
    Outro,
}

/// <summary>Preferência de horário de entrega (orienta a logística; sem "sem preferência").</summary>
public enum PreferenciaHorarioEntrega
{
    HorarioComercial,
    Manha,
    Tarde,
}

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
