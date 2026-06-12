using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Catalog;

namespace SistemaAN.Application.Catalog;

public sealed class CategoriaIngredienteService : ICategoriaIngredienteService
{
    private readonly IApplicationDbContext _db;

    public CategoriaIngredienteService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoriaIngredienteDto>> ListarAsync(bool incluirInativas = false, CancellationToken cancellationToken = default)
    {
        var query = _db.CategoriasIngredientes.AsQueryable();
        if (!incluirInativas)
        {
            query = query.Where(c => c.Ativo);
        }

        return await query
            .OrderBy(c => c.Ordem)
            .ThenBy(c => c.Nome)
            .Select(c => new CategoriaIngredienteDto(c.Id, c.Nome, c.Descricao, c.Ordem, c.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoriaIngredienteDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var c = await _db.CategoriasIngredientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Categoria de ingrediente", id);
        return new CategoriaIngredienteDto(c.Id, c.Nome, c.Descricao, c.Ordem, c.Ativo);
    }

    public async Task<CategoriaIngredienteDto> CriarAsync(SalvarCategoriaIngredienteRequest request, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(request, null, cancellationToken);

        var categoria = CategoriaIngrediente.Criar(request.Nome, request.Descricao, request.Ordem);
        categoria.DefinirAtivo(request.Ativo);

        _db.CategoriasIngredientes.Add(categoria);
        await _db.SaveChangesAsync(cancellationToken);
        return new CategoriaIngredienteDto(categoria.Id, categoria.Nome, categoria.Descricao, categoria.Ordem, categoria.Ativo);
    }

    public async Task<CategoriaIngredienteDto> AtualizarAsync(long id, SalvarCategoriaIngredienteRequest request, CancellationToken cancellationToken = default)
    {
        var categoria = await _db.CategoriasIngredientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Categoria de ingrediente", id);

        await ValidarAsync(request, id, cancellationToken);

        if (!request.Ativo)
        {
            await GarantirQuePodeInativarAsync(id, cancellationToken);
        }

        categoria.Atualizar(request.Nome, request.Descricao, request.Ordem, request.Ativo);
        await _db.SaveChangesAsync(cancellationToken);
        return new CategoriaIngredienteDto(categoria.Id, categoria.Nome, categoria.Descricao, categoria.Ordem, categoria.Ativo);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var categoria = await _db.CategoriasIngredientes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Categoria de ingrediente", id);

        if (!ativo)
        {
            await GarantirQuePodeInativarAsync(id, cancellationToken);
        }

        categoria.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarAsync(SalvarCategoriaIngredienteRequest request, long? idAtual, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Informe o nome da categoria."] });
        }

        var nome = request.Nome.Trim();
        var nomeEmUso = await _db.CategoriasIngredientes.AnyAsync(
            c => c.Nome == nome && (idAtual == null || c.Id != idAtual),
            cancellationToken);
        if (nomeEmUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Já existe uma categoria com este nome."] });
        }
    }

    private async Task GarantirQuePodeInativarAsync(long id, CancellationToken cancellationToken)
    {
        if (await _db.Ingredientes.AnyAsync(i => i.CategoriaId == id && i.Ativo, cancellationToken))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["ativo"] = ["Categoria em uso por ingredientes ativos — não pode ser inativada."],
            });
        }
    }
}
