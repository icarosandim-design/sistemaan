using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Pets;

namespace SistemaAN.Application.Cadastros;

public sealed class RacaService : IRacaService
{
    private readonly IApplicationDbContext _db;

    public RacaService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<RacaDto>> ListarAsync(bool incluirInativas = false, CancellationToken cancellationToken = default)
    {
        var query = _db.Racas.AsQueryable();
        if (!incluirInativas)
        {
            query = query.Where(r => r.Ativo);
        }

        return await query
            .OrderBy(r => r.Ordem)
            .ThenBy(r => r.Nome)
            .Select(r => new RacaDto(r.Id, r.Nome, r.Ordem, r.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task<RacaDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var r = await _db.Racas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Raça", id);
        return new RacaDto(r.Id, r.Nome, r.Ordem, r.Ativo);
    }

    public async Task<RacaDto> CriarAsync(SalvarRacaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(request, null, cancellationToken);

        var raca = Raca.Criar(request.Nome, request.Ordem);
        raca.DefinirAtivo(request.Ativo);

        _db.Racas.Add(raca);
        await _db.SaveChangesAsync(cancellationToken);
        return new RacaDto(raca.Id, raca.Nome, raca.Ordem, raca.Ativo);
    }

    public async Task<RacaDto> AtualizarAsync(long id, SalvarRacaRequest request, CancellationToken cancellationToken = default)
    {
        var raca = await _db.Racas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Raça", id);

        await ValidarAsync(request, id, cancellationToken);

        raca.Atualizar(request.Nome, request.Ordem, request.Ativo);
        await _db.SaveChangesAsync(cancellationToken);
        return new RacaDto(raca.Id, raca.Nome, raca.Ordem, raca.Ativo);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var raca = await _db.Racas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Raça", id);

        raca.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarAsync(SalvarRacaRequest request, long? idAtual, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Informe o nome da raça."] });
        }

        var nome = request.Nome.Trim();
        var emUso = await _db.Racas.AnyAsync(r => r.Nome == nome && (idAtual == null || r.Id != idAtual), cancellationToken);
        if (emUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Já existe uma raça com este nome."] });
        }
    }
}
