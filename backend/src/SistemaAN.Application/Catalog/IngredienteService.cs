using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Catalog;

namespace SistemaAN.Application.Catalog;

public sealed class IngredienteService : IIngredienteService
{
    private readonly IApplicationDbContext _db;

    public IngredienteService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoriaDto>> ListarCategoriasAsync(CancellationToken cancellationToken = default)
    {
        return await _db.CategoriasIngredientes
            .Where(c => c.Ativo)
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaDto(c.Id, c.Nome))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IngredienteDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var itens = await _db.Ingredientes
            .Include(i => i.Categoria)
            .OrderBy(i => i.Nome)
            .ToListAsync(cancellationToken);

        return itens.Select(Map).ToList();
    }

    public async Task<IngredienteDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var ing = await _db.Ingredientes
            .Include(i => i.Categoria)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException("Ingrediente", id);

        return Map(ing);
    }

    public async Task<IngredienteDto> CriarAsync(SalvarIngredienteRequest request, CancellationToken cancellationToken = default)
    {
        var (tipo, categoria) = await ValidarAsync(request, cancellationToken);

        var ing = Ingrediente.Criar(request.Nome, request.CategoriaId, tipo, request.Coeficiente, request.CustoKg);
        ing.DefinirAtivo(request.Ativo);

        _db.Ingredientes.Add(ing);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(ing, categoria.Nome);
    }

    public async Task<IngredienteDto> AtualizarAsync(long id, SalvarIngredienteRequest request, CancellationToken cancellationToken = default)
    {
        var ing = await _db.Ingredientes.FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException("Ingrediente", id);

        var (tipo, categoria) = await ValidarAsync(request, cancellationToken);

        // Bloqueia inativar um ingrediente usado por receita em plano vigente.
        if (ing.Ativo && !request.Ativo
            && await Planos.PlanoUso.IngredienteEmPlanoAtivoAsync(_db, id, cancellationToken))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["ativo"] = ["Ingrediente em uso em um plano vigente — não pode ser inativado."],
            });
        }

        ing.Atualizar(request.Nome, request.CategoriaId, tipo, request.Coeficiente, request.CustoKg, request.Ativo);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(ing, categoria.Nome);
    }

    public async Task ExcluirAsync(long id, CancellationToken cancellationToken = default)
    {
        var ing = await _db.Ingredientes.FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException("Ingrediente", id);

        // Futuro: bloquear exclusão se o ingrediente estiver em uso por receitas.
        _db.Ingredientes.Remove(ing);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<(TipoConversao tipo, CategoriaIngrediente categoria)> ValidarAsync(
        SalvarIngredienteRequest request,
        CancellationToken cancellationToken)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            erros["nome"] = ["Informe o nome."];
        }

        if (request.Coeficiente <= 0)
        {
            erros["coeficiente"] = ["O coeficiente deve ser maior que zero."];
        }

        if (request.CustoKg < 0)
        {
            erros["custoKg"] = ["O custo não pode ser negativo."];
        }

        TipoConversao tipo;
        try
        {
            tipo = TipoConversaoMap.FromDbValue(request.TipoConversao);
        }
        catch
        {
            tipo = TipoConversao.Perda;
            erros["tipoConversao"] = ["Tipo de conversão inválido."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        var categoria = await _db.CategoriasIngredientes
            .FirstOrDefaultAsync(c => c.Id == request.CategoriaId, cancellationToken)
            ?? throw new ValidationException(new Dictionary<string, string[]>
            {
                ["categoriaId"] = ["Categoria inexistente."],
            });

        return (tipo, categoria);
    }

    private static IngredienteDto Map(Ingrediente i) => Map(i, i.Categoria?.Nome ?? string.Empty);

    private static IngredienteDto Map(Ingrediente i, string categoriaNome) => new(
        i.Id,
        i.Nome,
        i.CategoriaId,
        categoriaNome,
        i.TipoConversao.ToDbValue(),
        i.CoeficienteConversao,
        i.CustoAtualKg,
        i.Ativo);
}
