namespace SistemaAN.Application.Produtos;

public sealed record ProdutoDto(
    long Id,
    string Nome,
    string? Codigo,
    string Tipo,
    long? ReceitaCasaId,
    string? ReceitaCasaNome,
    long? TamanhoPacoteId,
    string? TamanhoPacoteNome,
    string UnidadeMedida,
    int? PesoGramas,
    decimal PrecoVendaAvulsaPF,
    decimal PrecoVendaPJ,
    bool ControlaEstoque,
    bool ProduzidoInternamente,
    bool Ativo,
    string? Observacoes);

public sealed record SalvarProdutoRequest(
    string Nome,
    string? Codigo,
    string Tipo,
    long? ReceitaCasaId,
    long? TamanhoPacoteId,
    string? UnidadeMedida,
    int? PesoGramas,
    decimal PrecoVendaAvulsaPF,
    decimal PrecoVendaPJ,
    bool ControlaEstoque,
    bool ProduzidoInternamente,
    bool Ativo,
    string? Observacoes);

/// <summary>Tamanho a gerar como Produto a partir de uma Receita da Casa, com preços.</summary>
public sealed record GerarProdutoTamanhoRequest(
    long TamanhoPacoteId,
    decimal PrecoVendaAvulsaPF,
    decimal PrecoVendaPJ);

public sealed record GerarProdutosReceitaRequest(IReadOnlyList<GerarProdutoTamanhoRequest> Tamanhos);
