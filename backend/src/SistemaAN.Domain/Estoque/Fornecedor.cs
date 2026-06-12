using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Estoque;

/// <summary>
/// Fornecedor: cadastro-base **global** e reutilizável. Aparece no menu Estoque
/// nesta fase (usado nas entradas), mas será compartilhado por Compras, Contas a
/// Pagar e Financeiro no futuro. Campos financeiros/bancários já previstos.
/// </summary>
public class Fornecedor : AuditableEntity
{
    private Fornecedor() { } // EF Core

    private Fornecedor(string nome)
    {
        Nome = nome;
        Ativo = true;
    }

    /// <summary>Nome / razão social.</summary>
    public string Nome { get; private set; } = string.Empty;

    public string? NomeFantasia { get; private set; }

    /// <summary>CPF/CNPJ (opcional).</summary>
    public string? Documento { get; private set; }

    public string? Telefone { get; private set; }

    public string? WhatsApp { get; private set; }

    public string? Email { get; private set; }

    public string? PessoaContato { get; private set; }

    public string? Endereco { get; private set; }

    public string? Cidade { get; private set; }

    public string? Estado { get; private set; }

    public CategoriaFornecedor? Categoria { get; private set; }

    public string? Observacoes { get; private set; }

    public bool Ativo { get; private set; }

    // ---- Campos preparados para Compras/Financeiro (todos opcionais) ----
    public int? PrazoPagamentoDias { get; private set; }

    public string? FormaPagamentoPreferida { get; private set; }

    public string? ChavePix { get; private set; }

    public string? DadosBancarios { get; private set; }

    public static Fornecedor Criar(
        string nome,
        string? nomeFantasia,
        string? documento,
        string? telefone,
        string? whatsApp,
        string? email,
        string? pessoaContato,
        string? endereco,
        string? cidade,
        string? estado,
        CategoriaFornecedor? categoria,
        string? observacoes,
        int? prazoPagamentoDias,
        string? formaPagamentoPreferida,
        string? chavePix,
        string? dadosBancarios)
    {
        var f = new Fornecedor(nome.Trim());
        f.Aplicar(nomeFantasia, documento, telefone, whatsApp, email, pessoaContato, endereco, cidade,
            estado, categoria, observacoes, prazoPagamentoDias, formaPagamentoPreferida, chavePix, dadosBancarios);
        return f;
    }

    public void Atualizar(
        string nome,
        string? nomeFantasia,
        string? documento,
        string? telefone,
        string? whatsApp,
        string? email,
        string? pessoaContato,
        string? endereco,
        string? cidade,
        string? estado,
        CategoriaFornecedor? categoria,
        string? observacoes,
        int? prazoPagamentoDias,
        string? formaPagamentoPreferida,
        string? chavePix,
        string? dadosBancarios,
        bool ativo)
    {
        Nome = nome.Trim();
        Aplicar(nomeFantasia, documento, telefone, whatsApp, email, pessoaContato, endereco, cidade,
            estado, categoria, observacoes, prazoPagamentoDias, formaPagamentoPreferida, chavePix, dadosBancarios);
        Ativo = ativo;
    }

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    private void Aplicar(
        string? nomeFantasia, string? documento, string? telefone, string? whatsApp, string? email,
        string? pessoaContato, string? endereco, string? cidade, string? estado, CategoriaFornecedor? categoria,
        string? observacoes, int? prazoPagamentoDias, string? formaPagamentoPreferida, string? chavePix, string? dadosBancarios)
    {
        NomeFantasia = Texto(nomeFantasia);
        Documento = Texto(documento);
        Telefone = Texto(telefone);
        WhatsApp = Texto(whatsApp);
        Email = Texto(email);
        PessoaContato = Texto(pessoaContato);
        Endereco = Texto(endereco);
        Cidade = Texto(cidade);
        Estado = Texto(estado)?.ToUpperInvariant();
        Categoria = categoria;
        Observacoes = Texto(observacoes);
        PrazoPagamentoDias = prazoPagamentoDias;
        FormaPagamentoPreferida = Texto(formaPagamentoPreferida);
        ChavePix = Texto(chavePix);
        DadosBancarios = Texto(dadosBancarios);
    }

    private static string? Texto(string? v)
    {
        var t = v?.Trim();
        return string.IsNullOrEmpty(t) ? null : t;
    }
}
