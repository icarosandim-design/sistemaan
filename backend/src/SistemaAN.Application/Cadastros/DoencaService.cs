using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Pets;

namespace SistemaAN.Application.Cadastros;

public sealed class DoencaService : IDoencaService
{
    private readonly IApplicationDbContext _db;

    public DoencaService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<DoencaDto>> ListarAsync(bool incluirInativas = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Doencas.AsQueryable();
        if (!incluirInativas)
        {
            query = query.Where(d => d.Ativo);
        }

        return await query
            .OrderBy(d => d.Ordem)
            .ThenBy(d => d.Nome)
            .Select(d => new DoencaDto(d.Id, d.Nome, d.Ordem, d.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task<DoencaDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var d = await _db.Doencas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Doença", id);
        return new DoencaDto(d.Id, d.Nome, d.Ordem, d.Ativo);
    }

    public async Task<DoencaDto> CriarAsync(SalvarDoencaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(request, null, cancellationToken);

        var doenca = Doenca.Criar(request.Nome, request.Ordem);
        doenca.DefinirAtivo(request.Ativo);

        _db.Doencas.Add(doenca);
        await _db.SaveChangesAsync(cancellationToken);
        return new DoencaDto(doenca.Id, doenca.Nome, doenca.Ordem, doenca.Ativo);
    }

    public async Task<DoencaDto> AtualizarAsync(long id, SalvarDoencaRequest request, CancellationToken cancellationToken = default)
    {
        var doenca = await _db.Doencas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Doença", id);

        await ValidarAsync(request, id, cancellationToken);

        doenca.Atualizar(request.Nome, request.Ordem, request.Ativo);
        await _db.SaveChangesAsync(cancellationToken);
        return new DoencaDto(doenca.Id, doenca.Nome, doenca.Ordem, doenca.Ativo);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var doenca = await _db.Doencas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Doença", id);

        doenca.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarAsync(SalvarDoencaRequest request, long? idAtual, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Informe o nome da doença."] });
        }

        var nome = request.Nome.Trim();
        var emUso = await _db.Doencas.AnyAsync(d => d.Nome == nome && (idAtual == null || d.Id != idAtual), cancellationToken);
        if (emUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Já existe uma doença com este nome."] });
        }
    }
}
