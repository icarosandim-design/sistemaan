using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Entregas;
using SistemaAN.Domain.Estoque;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Estoque;

public sealed class ItemEstoqueService : IItemEstoqueService
{
    private readonly IApplicationDbContext _db;

    public ItemEstoqueService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ItemEstoqueDto>> ListarAsync(bool apenasAbaixoMinimo = false, CancellationToken cancellationToken = default)
    {
        var query = _db.ItensEstoque.AsQueryable();
        if (apenasAbaixoMinimo)
        {
            query = query.Where(i => i.QuantidadeAtual < i.QuantidadeMinima);
        }

        return await query
            .OrderBy(i => i.Nome)
            .Select(Projecao())
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PersonalizadaProntaDto>> ListarPersonalizadasProntasAsync(CancellationToken cancellationToken = default)
    {
        // Entregas ativas (ainda não entregues/canceladas/reagendadas) que reservam pacotes prontos.
        var ativos = new[]
        {
            EntregaStatus.Programada, EntregaStatus.ConfirmadaCliente,
            EntregaStatus.SaiuParaEntrega, EntregaStatus.NaoEntregue,
        };

        var entregas = await _db.Entregas
            .Where(e => ativos.Contains(e.Status))
            .Include(e => e.Pets).ThenInclude(p => p.Itens)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var lista = new List<PersonalizadaProntaDto>();
        foreach (var e in entregas)
        {
            foreach (var pet in e.Pets)
            {
                foreach (var item in pet.Itens.Where(i =>
                    i.Tipo == TipoReceita.Personalizada
                    && i.StatusPreparo != StatusPreparoPersonalizada.NaoPronta
                    && (i.PacotesProntos ?? 0) > 0))
                {
                    lista.Add(new PersonalizadaProntaDto(
                        e.Id, item.ReceitaCodigo, item.ReceitaNome, item.TamanhoPacoteGramas ?? 0,
                        pet.PetNome, e.ClienteNome, e.DataPrevista, item.PacotesProntos ?? 0, e.Status.ToString()));
                }
            }
        }

        return lista
            .OrderBy(x => x.DataPrevista)
            .ThenBy(x => x.ClienteNome)
            .ThenBy(x => x.ReceitaNome)
            .ToList();
    }

    public async Task<ItemEstoqueDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var dto = await _db.ItensEstoque
            .Where(i => i.Id == id)
            .Select(Projecao())
            .FirstOrDefaultAsync(cancellationToken);
        return dto ?? throw new NotFoundException("Item de estoque", id);
    }

    public async Task<ItemEstoqueDto> CriarInsumoAsync(CriarItemInsumoRequest request, CancellationToken cancellationToken = default)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            erros["nome"] = ["Informe o nome do item."];
        }

        var categoria = ParseCategoria(request.Categoria, erros);
        var unidade = ParseUnidade(request.UnidadeMedida, erros);

        if (request.QuantidadeMinima < 0m)
        {
            erros["quantidadeMinima"] = ["O estoque mínimo não pode ser negativo."];
        }

        if (request.IngredienteId is long ingId)
        {
            var existeIng = await _db.Ingredientes.AnyAsync(x => x.Id == ingId, cancellationToken);
            if (!existeIng)
            {
                erros["ingredienteId"] = ["Ingrediente não encontrado."];
            }
            else if (await _db.ItensEstoque.AnyAsync(x => x.IngredienteId == ingId, cancellationToken))
            {
                erros["ingredienteId"] = ["Este ingrediente já está vinculado a um item de estoque."];
            }
        }

        if (request.FornecedorPrincipalId is long fornId && !await _db.Fornecedores.AnyAsync(x => x.Id == fornId, cancellationToken))
        {
            erros["fornecedorPrincipalId"] = ["Fornecedor não encontrado."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        var item = ItemEstoque.CriarInsumo(
            request.Nome, categoria!.Value, unidade!.Value, request.IngredienteId, request.QuantidadeMinima,
            request.FornecedorPrincipalId, request.LocalArmazenamento, request.ControlaValidade, request.Observacoes);
        item.DefinirAtivo(request.Ativo);

        _db.ItensEstoque.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(item.Id, cancellationToken);
    }

    public async Task<ItemEstoqueDto> CriarProdutoAcabadoAsync(CriarItemProdutoAcabadoRequest request, CancellationToken cancellationToken = default)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            erros["nome"] = ["Informe o nome do item."];
        }

        var receita = await _db.Receitas.FirstOrDefaultAsync(r => r.Id == request.ReceitaId, cancellationToken);
        if (receita is null)
        {
            erros["receitaId"] = ["Receita não encontrada."];
        }
        else if (receita.Tipo != TipoReceita.Casa)
        {
            erros["receitaId"] = ["Produto acabado é controlado apenas para Receitas da Casa."];
        }

        if (!await _db.TamanhosPacote.AnyAsync(t => t.Id == request.TamanhoPacoteId, cancellationToken))
        {
            erros["tamanhoPacoteId"] = ["Tamanho de pacote não encontrado."];
        }

        if (request.QuantidadeMinima < 0m)
        {
            erros["quantidadeMinima"] = ["O estoque mínimo não pode ser negativo."];
        }

        if (await _db.ItensEstoque.AnyAsync(x => x.ReceitaId == request.ReceitaId && x.TamanhoPacoteId == request.TamanhoPacoteId, cancellationToken))
        {
            erros["receitaId"] = ["Já existe um item de produto acabado para esta receita e tamanho."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        var item = ItemEstoque.CriarProdutoAcabadoCasa(
            request.Nome, request.ReceitaId, request.TamanhoPacoteId, request.QuantidadeMinima,
            request.LocalArmazenamento, request.ControlaValidade, request.Observacoes);
        item.DefinirAtivo(request.Ativo);

        _db.ItensEstoque.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(item.Id, cancellationToken);
    }

    public async Task<ItemEstoqueDto> AtualizarAsync(long id, AtualizarItemEstoqueRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _db.ItensEstoque.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Item de estoque", id);

        var erros = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            erros["nome"] = ["Informe o nome do item."];
        }

        var categoria = ParseCategoria(request.Categoria, erros);
        var unidade = ParseUnidade(request.UnidadeMedida, erros);

        if (request.QuantidadeMinima < 0m)
        {
            erros["quantidadeMinima"] = ["O estoque mínimo não pode ser negativo."];
        }

        if (request.FornecedorPrincipalId is long fornId && !await _db.Fornecedores.AnyAsync(x => x.Id == fornId, cancellationToken))
        {
            erros["fornecedorPrincipalId"] = ["Fornecedor não encontrado."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        item.Atualizar(
            request.Nome, categoria!.Value, unidade!.Value, request.QuantidadeMinima, request.FornecedorPrincipalId,
            request.LocalArmazenamento, request.ControlaValidade, request.Observacoes, request.Ativo);

        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(item.Id, cancellationToken);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var item = await _db.ItensEstoque.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Item de estoque", id);

        item.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static CategoriaEstoque? ParseCategoria(string? valor, Dictionary<string, string[]> erros)
    {
        if (!string.IsNullOrWhiteSpace(valor) && Enum.TryParse<CategoriaEstoque>(valor, out var c))
        {
            return c;
        }

        erros["categoria"] = ["Categoria de estoque inválida."];
        return null;
    }

    private static UnidadeMedida? ParseUnidade(string? valor, Dictionary<string, string[]> erros)
    {
        if (!string.IsNullOrWhiteSpace(valor) && Enum.TryParse<UnidadeMedida>(valor, out var u))
        {
            return u;
        }

        erros["unidadeMedida"] = ["Unidade de medida inválida."];
        return null;
    }

    private Expression<Func<ItemEstoque, ItemEstoqueDto>> Projecao()
        => i => new ItemEstoqueDto(
            i.Id,
            i.Tipo.ToString(),
            i.Nome,
            i.Categoria.ToString(),
            i.UnidadeMedida.ToString(),
            i.IngredienteId,
            _db.Ingredientes.Where(x => x.Id == i.IngredienteId).Select(x => x.Nome).FirstOrDefault(),
            i.ReceitaId,
            _db.Receitas.Where(x => x.Id == i.ReceitaId).Select(x => x.Nome).FirstOrDefault(),
            i.TamanhoPacoteId,
            _db.TamanhosPacote.Where(x => x.Id == i.TamanhoPacoteId).Select(x => x.Nome).FirstOrDefault(),
            i.QuantidadeAtual,
            i.QuantidadeMinima,
            i.CustoMedio,
            i.FornecedorPrincipalId,
            _db.Fornecedores.Where(x => x.Id == i.FornecedorPrincipalId).Select(x => x.Nome).FirstOrDefault(),
            i.LocalArmazenamento,
            i.ControlaValidade,
            i.Ativo,
            i.Observacoes,
            i.QuantidadeAtual < i.QuantidadeMinima);
}
