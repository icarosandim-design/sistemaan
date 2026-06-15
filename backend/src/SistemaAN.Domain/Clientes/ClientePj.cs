using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Clientes;

/// <summary>
/// Dados específicos de Cliente Pessoa Jurídica (1:1 com <see cref="Cliente"/>).
/// O endereço/telefone/e-mail comerciais ficam no próprio Cliente; aqui ficam os
/// dados PJ (razão social, CNPJ, contato), o endereço de entrega e os dados comerciais.
/// </summary>
public class ClientePj : AuditableEntity
{
    private ClientePj() { } // EF Core

    public long ClienteId { get; private set; }

    // ----- Cadastrais -----
    public string RazaoSocial { get; private set; } = string.Empty;
    public string NomeFantasia { get; private set; } = string.Empty;
    public string Cnpj { get; private set; } = string.Empty;
    public string? InscricaoEstadual { get; private set; }
    public string? Whatsapp { get; private set; }
    public string? PessoaContato { get; private set; }
    public string? CargoContato { get; private set; }

    // ----- Endereço de entrega -----
    public string? EntregaRua { get; private set; }
    public string? EntregaNumero { get; private set; }
    public string? EntregaComplemento { get; private set; }
    public string? EntregaBairro { get; private set; }
    public string? EntregaCidade { get; private set; }
    public string? EntregaEstado { get; private set; }
    public string? EntregaCep { get; private set; }

    // ----- Comerciais -----
    public TipoClientePJ TipoPJ { get; private set; }
    public string? CondicaoComercial { get; private set; }
    public string? PrazoPagamento { get; private set; }
    public int? DiaEntregaPreferencial { get; private set; }
    public string? FrequenciaCompra { get; private set; }
    public string? ObservacoesComerciais { get; private set; }

    public static ClientePj Criar(long clienteId, DadosClientePj d)
    {
        var pj = new ClientePj { ClienteId = clienteId };
        pj.Aplicar(d);
        return pj;
    }

    public void Atualizar(DadosClientePj d) => Aplicar(d);

    private void Aplicar(DadosClientePj d)
    {
        RazaoSocial = d.RazaoSocial.Trim();
        NomeFantasia = d.NomeFantasia.Trim();
        Cnpj = Limpar(d.Cnpj);
        InscricaoEstadual = d.InscricaoEstadual?.Trim();
        Whatsapp = d.Whatsapp?.Trim();
        PessoaContato = d.PessoaContato?.Trim();
        CargoContato = d.CargoContato?.Trim();
        EntregaRua = d.EntregaRua?.Trim();
        EntregaNumero = d.EntregaNumero?.Trim();
        EntregaComplemento = d.EntregaComplemento?.Trim();
        EntregaBairro = d.EntregaBairro?.Trim();
        EntregaCidade = d.EntregaCidade?.Trim();
        EntregaEstado = d.EntregaEstado?.Trim();
        EntregaCep = d.EntregaCep?.Trim();
        TipoPJ = d.TipoPJ;
        CondicaoComercial = d.CondicaoComercial?.Trim();
        PrazoPagamento = d.PrazoPagamento?.Trim();
        DiaEntregaPreferencial = d.DiaEntregaPreferencial;
        FrequenciaCompra = d.FrequenciaCompra?.Trim();
        ObservacoesComerciais = d.ObservacoesComerciais?.Trim();
    }

    private static string Limpar(string cnpj)
        => new(cnpj.Where(char.IsDigit).ToArray());
}

public sealed record DadosClientePj(
    string RazaoSocial,
    string NomeFantasia,
    string Cnpj,
    string? InscricaoEstadual,
    string? Whatsapp,
    string? PessoaContato,
    string? CargoContato,
    string? EntregaRua,
    string? EntregaNumero,
    string? EntregaComplemento,
    string? EntregaBairro,
    string? EntregaCidade,
    string? EntregaEstado,
    string? EntregaCep,
    TipoClientePJ TipoPJ,
    string? CondicaoComercial,
    string? PrazoPagamento,
    int? DiaEntregaPreferencial,
    string? FrequenciaCompra,
    string? ObservacoesComerciais);
