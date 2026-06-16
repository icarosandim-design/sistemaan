using SistemaAN.Domain.Common;
using SistemaAN.Domain.Estoque;

namespace SistemaAN.Domain.Produtos;

/// <summary>Tipo comercial do produto.</summary>
public enum TipoProduto
{
    ReceitaDaCasa,
    Petisco,
    ProdutoComprado,
    Brinde,
    Outro,
}

/// <summary>
/// Produto = item comercial vendido/entregue e controlado em estoque.
/// Pode derivar de uma Receita da Casa (ficha técnica) + Tamanho de Pacote, ou
/// existir sozinho (petisco, comprado pronto, brinde). A Receita da Casa continua
/// sendo a ficha técnica/custo; o Produto guarda o preço de venda e dados comerciais.
/// </summary>
public class Produto : AuditableEntity
{
    private Produto() { } // EF Core

    private Produto(DadosProduto d)
    {
        Aplicar(d);
        Ativo = true;
    }

    public string Nome { get; private set; } = string.Empty;
    public string? Codigo { get; private set; }
    public TipoProduto Tipo { get; private set; }

    /// <summary>Receita da Casa de origem (quando produzido internamente).</summary>
    public long? ReceitaCasaId { get; private set; }
    public long? TamanhoPacoteId { get; private set; }

    public UnidadeMedida UnidadeMedida { get; private set; }
    public int? PesoGramas { get; private set; }

    public decimal PrecoVendaAvulsaPF { get; private set; }
    public decimal PrecoVendaPJ { get; private set; }

    public bool ControlaEstoque { get; private set; }
    public bool ProduzidoInternamente { get; private set; }
    public bool Ativo { get; private set; }
    public string? Observacoes { get; private set; }

    public static Produto Criar(DadosProduto dados) => new(dados);

    public void Atualizar(DadosProduto dados) => Aplicar(dados);

    public void DefinirAtivo(bool ativo) => Ativo = ativo;

    public void DefinirPrecos(decimal precoPf, decimal precoPj)
    {
        PrecoVendaAvulsaPF = precoPf;
        PrecoVendaPJ = precoPj;
    }

    private void Aplicar(DadosProduto d)
    {
        Nome = d.Nome.Trim();
        Codigo = string.IsNullOrWhiteSpace(d.Codigo) ? null : d.Codigo.Trim();
        Tipo = d.Tipo;
        ReceitaCasaId = d.ReceitaCasaId;
        TamanhoPacoteId = d.TamanhoPacoteId;
        UnidadeMedida = d.UnidadeMedida;
        PesoGramas = d.PesoGramas;
        PrecoVendaAvulsaPF = d.PrecoVendaAvulsaPF;
        PrecoVendaPJ = d.PrecoVendaPJ;
        ControlaEstoque = d.ControlaEstoque;
        ProduzidoInternamente = d.ProduzidoInternamente;
        Observacoes = string.IsNullOrWhiteSpace(d.Observacoes) ? null : d.Observacoes.Trim();
    }
}

public sealed record DadosProduto(
    string Nome,
    string? Codigo,
    TipoProduto Tipo,
    long? ReceitaCasaId,
    long? TamanhoPacoteId,
    UnidadeMedida UnidadeMedida,
    int? PesoGramas,
    decimal PrecoVendaAvulsaPF,
    decimal PrecoVendaPJ,
    bool ControlaEstoque,
    bool ProduzidoInternamente,
    string? Observacoes);
