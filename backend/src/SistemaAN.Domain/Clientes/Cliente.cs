using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Clientes;

/// <summary>
/// Cliente da empresa: dados cadastrais (com endereço estruturado e CPF),
/// financeiros básicos embutidos e cancelamento de assinatura com motivo.
/// </summary>
public class Cliente : AuditableEntity
{
    private Cliente() { } // EF Core

    public string Nome { get; private set; } = string.Empty;
    public string? Cpf { get; private set; }
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? OrigemVenda { get; private set; }
    public string? Observacoes { get; private set; }

    // ----- Endereço -----
    public string? Rua { get; private set; }
    public string? Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string? Cep { get; private set; }
    public string? Bairro { get; private set; }
    public string? Cidade { get; private set; }
    public string? Estado { get; private set; }

    // ----- Situação / cancelamento -----
    public bool Ativo { get; private set; }
    public string? MotivoCancelamento { get; private set; }
    public DateTimeOffset? DataCancelamento { get; private set; }

    // ----- Financeiro básico -----
    public TipoCliente TipoCliente { get; private set; }
    public FormaPagamento? FormaPagamento { get; private set; }
    public int? DiaCobranca { get; private set; }
    public decimal ValorRecorrenteMensal { get; private set; }
    public StatusFinanceiro StatusFinanceiro { get; private set; }
    public string? ObservacoesFinanceiras { get; private set; }

    public static Cliente Criar(DadosCliente dados)
    {
        var c = new Cliente { Ativo = true };
        c.Aplicar(dados);
        return c;
    }

    public void Atualizar(DadosCliente dados) => Aplicar(dados);

    /// <summary>Cancela a assinatura/cliente, registrando o motivo e a data.</summary>
    public void Cancelar(string motivo, DateTimeOffset quando)
    {
        Ativo = false;
        MotivoCancelamento = motivo.Trim();
        DataCancelamento = quando;
    }

    public void Reativar()
    {
        Ativo = true;
        MotivoCancelamento = null;
        DataCancelamento = null;
    }

    private void Aplicar(DadosCliente d)
    {
        Nome = d.Nome.Trim();
        Cpf = Limpar(d.Cpf);
        Telefone = d.Telefone?.Trim();
        Email = d.Email?.Trim();
        OrigemVenda = d.OrigemVenda?.Trim();
        Observacoes = d.Observacoes?.Trim();
        Rua = d.Rua?.Trim();
        Numero = d.Numero?.Trim();
        Complemento = d.Complemento?.Trim();
        Cep = d.Cep?.Trim();
        Bairro = d.Bairro?.Trim();
        Cidade = d.Cidade?.Trim();
        Estado = d.Estado?.Trim();
        TipoCliente = d.TipoCliente;
        FormaPagamento = d.FormaPagamento;
        DiaCobranca = d.DiaCobranca;
        ValorRecorrenteMensal = d.ValorRecorrenteMensal;
        StatusFinanceiro = d.StatusFinanceiro;
        ObservacoesFinanceiras = d.ObservacoesFinanceiras?.Trim();
    }

    private static string? Limpar(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return null;
        }
        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        return digitos.Length == 0 ? null : digitos;
    }
}

public sealed record DadosCliente(
    string Nome,
    string? Cpf,
    string? Telefone,
    string? Email,
    string? OrigemVenda,
    string? Observacoes,
    string? Rua,
    string? Numero,
    string? Complemento,
    string? Cep,
    string? Bairro,
    string? Cidade,
    string? Estado,
    TipoCliente TipoCliente,
    FormaPagamento? FormaPagamento,
    int? DiaCobranca,
    decimal ValorRecorrenteMensal,
    StatusFinanceiro StatusFinanceiro,
    string? ObservacoesFinanceiras);
