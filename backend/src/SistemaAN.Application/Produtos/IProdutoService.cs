namespace SistemaAN.Application.Produtos;

/// <summary>Produtos = itens comerciais vendidos/entregues e controlados em estoque.</summary>
public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoDto>> ListarAsync(bool incluirInativos = false, string? tipo = null, long? receitaCasaId = null, long? tamanhoPacoteId = null, CancellationToken ct = default);

    Task<ProdutoDto> ObterAsync(long id, CancellationToken ct = default);

    Task<ProdutoDto> CriarAsync(SalvarProdutoRequest request, CancellationToken ct = default);

    Task<ProdutoDto> AtualizarAsync(long id, SalvarProdutoRequest request, CancellationToken ct = default);

    Task AlternarStatusAsync(long id, bool ativo, CancellationToken ct = default);

    /// <summary>Gera/atualiza Produtos (ReceitaDaCasa) para os tamanhos informados. Idempotente por receita+tamanho.</summary>
    Task<IReadOnlyList<ProdutoDto>> GerarParaReceitaAsync(long receitaCasaId, GerarProdutosReceitaRequest request, CancellationToken ct = default);
}
