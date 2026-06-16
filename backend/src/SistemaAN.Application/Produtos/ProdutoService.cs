using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Produtos;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Produtos;

public sealed class ProdutoService : IProdutoService
{
    private readonly IApplicationDbContext _db;

    public ProdutoService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProdutoDto>> ListarAsync(bool incluirInativos = false, string? tipo = null, long? receitaCasaId = null, long? tamanhoPacoteId = null, CancellationToken ct = default)
    {
        var query = _db.Produtos.AsQueryable();
        if (!incluirInativos) query = query.Where(p => p.Ativo);
        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoProduto>(tipo, true, out var t)) query = query.Where(p => p.Tipo == t);
        if (receitaCasaId is { } r) query = query.Where(p => p.ReceitaCasaId == r);
        if (tamanhoPacoteId is { } tp) query = query.Where(p => p.TamanhoPacoteId == tp);

        var produtos = await query.OrderBy(p => p.Nome).ToListAsync(ct);
        return await MapManyAsync(produtos, ct);
    }

    public async Task<ProdutoDto> ObterAsync(long id, CancellationToken ct = default)
    {
        var p = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Produto", id);
        return (await MapManyAsync([p], ct))[0];
    }

    public async Task<ProdutoDto> CriarAsync(SalvarProdutoRequest request, CancellationToken ct = default)
    {
        var dados = await ValidarAsync(request, null, ct);
        var produto = Produto.Criar(dados);
        produto.DefinirAtivo(request.Ativo);
        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync(ct);

        await GarantirItemEstoqueAsync(produto, ct);
        return await ObterAsync(produto.Id, ct);
    }

    public async Task<ProdutoDto> AtualizarAsync(long id, SalvarProdutoRequest request, CancellationToken ct = default)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Produto", id);
        var dados = await ValidarAsync(request, id, ct);
        produto.Atualizar(dados);
        produto.DefinirAtivo(request.Ativo);
        await _db.SaveChangesAsync(ct);

        await GarantirItemEstoqueAsync(produto, ct);
        return await ObterAsync(produto.Id, ct);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken ct = default)
    {
        var produto = await _db.Produtos.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Produto", id);
        produto.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ProdutoDto>> GerarParaReceitaAsync(long receitaCasaId, GerarProdutosReceitaRequest request, CancellationToken ct = default)
    {
        var receita = await _db.Receitas.FirstOrDefaultAsync(r => r.Id == receitaCasaId && r.Tipo == TipoReceita.Casa, ct)
            ?? throw new ValidationException(Erro("receitaCasaId", "Receita da Casa inválida."));

        var resultado = new List<long>();
        foreach (var t in request.Tamanhos ?? [])
        {
            var tamanho = await _db.TamanhosPacote.FirstOrDefaultAsync(x => x.Id == t.TamanhoPacoteId, ct);
            if (tamanho is null) continue;

            var existente = await _db.Produtos.FirstOrDefaultAsync(
                p => p.ReceitaCasaId == receitaCasaId && p.TamanhoPacoteId == t.TamanhoPacoteId, ct);

            var ativo = t.PrecoVendaAvulsaPF > 0m || t.PrecoVendaPJ > 0m; // sem preço → pendente (inativo)
            var dados = new DadosProduto(
                $"{receita.Nome} {tamanho.Nome}", null, TipoProduto.ReceitaDaCasa, receitaCasaId, tamanho.Id,
                UnidadeMedida.Pacote, tamanho.PesoGramas, t.PrecoVendaAvulsaPF, t.PrecoVendaPJ, true, true, null);

            if (existente is null)
            {
                var novo = Produto.Criar(dados);
                novo.DefinirAtivo(ativo);
                _db.Produtos.Add(novo);
                await _db.SaveChangesAsync(ct);
                await GarantirItemEstoqueAsync(novo, ct);
                resultado.Add(novo.Id);
            }
            else
            {
                existente.Atualizar(dados);
                existente.DefinirAtivo(ativo);
                await _db.SaveChangesAsync(ct);
                await GarantirItemEstoqueAsync(existente, ct);
                resultado.Add(existente.Id);
            }
        }

        var produtos = await _db.Produtos.Where(p => resultado.Contains(p.Id)).OrderBy(p => p.Nome).ToListAsync(ct);
        return await MapManyAsync(produtos, ct);
    }

    /// <summary>Garante o item de estoque correspondente ao Produto (quando controla estoque) e o vincula.</summary>
    private async Task GarantirItemEstoqueAsync(Produto produto, CancellationToken ct)
    {
        if (!produto.ControlaEstoque) return;

        if (produto.Tipo == TipoProduto.ReceitaDaCasa && produto.ReceitaCasaId is { } recId && produto.TamanhoPacoteId is { } tamId)
        {
            var item = await _db.ItensEstoque.FirstOrDefaultAsync(i => i.ReceitaId == recId && i.TamanhoPacoteId == tamId, ct);
            if (item is null)
            {
                item = ItemEstoque.CriarProdutoAcabadoCasa(produto.Nome, recId, tamId, 0m, null, true, null);
                item.DefinirProduto(produto.Id);
                _db.ItensEstoque.Add(item);
                await _db.SaveChangesAsync(ct);
            }
            else if (item.ProdutoId != produto.Id)
            {
                item.DefinirProduto(produto.Id);
                await _db.SaveChangesAsync(ct);
            }
            return;
        }

        // Produto comprado/petisco/brinde que controla estoque.
        var existente = await _db.ItensEstoque.FirstOrDefaultAsync(i => i.ProdutoId == produto.Id, ct);
        if (existente is null)
        {
            var item = ItemEstoque.CriarProdutoComprado(produto.Id, produto.Nome, produto.UnidadeMedida, 0m, null, null, false, null);
            _db.ItensEstoque.Add(item);
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<DadosProduto> ValidarAsync(SalvarProdutoRequest r, long? idAtual, CancellationToken ct)
    {
        var erros = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(r.Nome)) erros["nome"] = ["Informe o nome do produto."];
        if (!Enum.TryParse<TipoProduto>(r.Tipo, true, out var tipo)) erros["tipo"] = ["Tipo de produto inválido."];
        if (r.PrecoVendaAvulsaPF < 0m) erros["precoVendaAvulsaPF"] = ["O preço não pode ser negativo."];
        if (r.PrecoVendaPJ < 0m) erros["precoVendaPJ"] = ["O preço não pode ser negativo."];

        var unidade = UnidadeMedida.Unidade;
        if (tipo == TipoProduto.ReceitaDaCasa)
        {
            unidade = UnidadeMedida.Pacote;
            if (r.ReceitaCasaId is not { } recId)
            {
                erros["receitaCasaId"] = ["Produto de Receita da Casa exige uma receita."];
            }
            else if (!await _db.Receitas.AnyAsync(x => x.Id == recId && x.Tipo == TipoReceita.Casa, ct))
            {
                erros["receitaCasaId"] = ["Selecione uma Receita da Casa válida."];
            }
            if (r.TamanhoPacoteId is { } tamId && !await _db.TamanhosPacote.AnyAsync(x => x.Id == tamId, ct))
            {
                erros["tamanhoPacoteId"] = ["Selecione um tamanho de pacote válido."];
            }
        }
        else if (!string.IsNullOrWhiteSpace(r.UnidadeMedida) && Enum.TryParse<UnidadeMedida>(r.UnidadeMedida, true, out var u))
        {
            unidade = u;
        }

        if (erros.Count > 0) throw new ValidationException(erros);

        // Unicidade receita+tamanho (produto de receita).
        if (tipo == TipoProduto.ReceitaDaCasa && r.ReceitaCasaId is { } rid && r.TamanhoPacoteId is { } tid)
        {
            var dup = await _db.Produtos.AnyAsync(p => p.ReceitaCasaId == rid && p.TamanhoPacoteId == tid && (idAtual == null || p.Id != idAtual), ct);
            if (dup) throw new ValidationException(Erro("tamanhoPacoteId", "Já existe um produto para esta receita e tamanho."));
        }

        if (!string.IsNullOrWhiteSpace(r.Codigo))
        {
            var codigo = r.Codigo.Trim();
            var dup = await _db.Produtos.AnyAsync(p => p.Codigo == codigo && (idAtual == null || p.Id != idAtual), ct);
            if (dup) throw new ValidationException(Erro("codigo", "Já existe um produto com este código."));
        }

        var produzido = tipo == TipoProduto.ReceitaDaCasa || r.ProduzidoInternamente;
        return new DadosProduto(
            r.Nome, r.Codigo, tipo, tipo == TipoProduto.ReceitaDaCasa ? r.ReceitaCasaId : null,
            tipo == TipoProduto.ReceitaDaCasa ? r.TamanhoPacoteId : null, unidade, r.PesoGramas,
            r.PrecoVendaAvulsaPF, r.PrecoVendaPJ, r.ControlaEstoque, produzido, r.Observacoes);
    }

    private async Task<List<ProdutoDto>> MapManyAsync(List<Produto> produtos, CancellationToken ct)
    {
        var recIds = produtos.Where(p => p.ReceitaCasaId != null).Select(p => p.ReceitaCasaId!.Value).Distinct().ToList();
        var tamIds = produtos.Where(p => p.TamanhoPacoteId != null).Select(p => p.TamanhoPacoteId!.Value).Distinct().ToList();
        var receitas = await _db.Receitas.Where(r => recIds.Contains(r.Id)).Select(r => new { r.Id, r.Nome }).ToListAsync(ct);
        var tamanhos = await _db.TamanhosPacote.Where(t => tamIds.Contains(t.Id)).Select(t => new { t.Id, t.Nome }).ToListAsync(ct);
        var mapR = receitas.ToDictionary(r => r.Id, r => r.Nome);
        var mapT = tamanhos.ToDictionary(t => t.Id, t => t.Nome);

        return produtos.Select(p => new ProdutoDto(
            p.Id, p.Nome, p.Codigo, p.Tipo.ToString(),
            p.ReceitaCasaId, p.ReceitaCasaId is { } r && mapR.TryGetValue(r, out var rn) ? rn : null,
            p.TamanhoPacoteId, p.TamanhoPacoteId is { } t && mapT.TryGetValue(t, out var tn) ? tn : null,
            p.UnidadeMedida.ToString(), p.PesoGramas, p.PrecoVendaAvulsaPF, p.PrecoVendaPJ,
            p.ControlaEstoque, p.ProduzidoInternamente, p.Ativo, p.Observacoes)).ToList();
    }

    private static Dictionary<string, string[]> Erro(string campo, string msg) => new() { [campo] = [msg] };
}
