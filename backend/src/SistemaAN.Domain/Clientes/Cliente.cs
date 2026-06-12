using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Clientes;

/// <summary>
/// Cliente da empresa, com dados cadastrais e financeiros básicos embutidos
/// (sem módulo financeiro completo nesta fase).
/// </summary>
public class Cliente : AuditableEntity
{
    private Cliente() { } // EF Core

    public string Nome { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }
    public string? Endereco { get; private set; }
    public string? Bairro { get; private set; }
    public string? Cidade { get; private set; }
    public string? Observacoes { get; private set; }
    public bool Ativo { get; private set; }

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

    public void Atualizar(DadosCliente dados, bool ativo)
    {
        Aplicar(dados);
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    private void Aplicar(DadosCliente d)
    {
        Nome = d.Nome.Trim();
        Telefone = d.Telefone?.Trim();
        Email = d.Email?.Trim();
        Endereco = d.Endereco?.Trim();
        Bairro = d.Bairro?.Trim();
        Cidade = d.Cidade?.Trim();
        Observacoes = d.Observacoes?.Trim();
        TipoCliente = d.TipoCliente;
        FormaPagamento = d.FormaPagamento;
        DiaCobranca = d.DiaCobranca;
        ValorRecorrenteMensal = d.ValorRecorrenteMensal;
        StatusFinanceiro = d.StatusFinanceiro;
        ObservacoesFinanceiras = d.ObservacoesFinanceiras?.Trim();
    }
}

/// <summary>Dados de criação/atualização do cliente (cadastrais + financeiro básico).</summary>
public sealed record DadosCliente(
    string Nome,
    string? Telefone,
    string? Email,
    string? Endereco,
    string? Bairro,
    string? Cidade,
    string? Observacoes,
    TipoCliente TipoCliente,
    FormaPagamento? FormaPagamento,
    int? DiaCobranca,
    decimal ValorRecorrenteMensal,
    StatusFinanceiro StatusFinanceiro,
    string? ObservacoesFinanceiras);
