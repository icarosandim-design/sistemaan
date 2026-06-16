using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Application.Clientes;

public sealed class OrigemVendaService : IOrigemVendaService
{
    private readonly IApplicationDbContext _db;

    public OrigemVendaService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<OrigemVendaDto>> ListarAsync(bool incluirInativas = false, CancellationToken cancellationToken = default)
    {
        var query = _db.OrigensVenda.AsQueryable();
        if (!incluirInativas)
        {
            query = query.Where(o => o.Ativo);
        }

        return await query
            .OrderBy(o => o.Ordem)
            .ThenBy(o => o.Nome)
            .Select(o => new OrigemVendaDto(o.Id, o.Nome, o.Ordem, o.Ativo))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrigemVendaDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var o = await _db.OrigensVenda.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Origem de venda", id);
        return new OrigemVendaDto(o.Id, o.Nome, o.Ordem, o.Ativo);
    }

    public async Task<OrigemVendaDto> CriarAsync(SalvarOrigemVendaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(request, null, cancellationToken);

        var origem = OrigemVenda.Criar(request.Nome, request.Ordem);
        origem.DefinirAtivo(request.Ativo);

        _db.OrigensVenda.Add(origem);
        await _db.SaveChangesAsync(cancellationToken);
        return new OrigemVendaDto(origem.Id, origem.Nome, origem.Ordem, origem.Ativo);
    }

    public async Task<OrigemVendaDto> AtualizarAsync(long id, SalvarOrigemVendaRequest request, CancellationToken cancellationToken = default)
    {
        var origem = await _db.OrigensVenda.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Origem de venda", id);

        await ValidarAsync(request, id, cancellationToken);

        origem.Atualizar(request.Nome, request.Ordem, request.Ativo);
        await _db.SaveChangesAsync(cancellationToken);
        return new OrigemVendaDto(origem.Id, origem.Nome, origem.Ordem, origem.Ativo);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var origem = await _db.OrigensVenda.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Origem de venda", id);

        origem.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarAsync(SalvarOrigemVendaRequest request, long? idAtual, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Informe o nome da origem."] });
        }

        var nome = request.Nome.Trim();
        var emUso = await _db.OrigensVenda.AnyAsync(o => o.Nome == nome && (idAtual == null || o.Id != idAtual), cancellationToken);
        if (emUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Já existe uma origem com este nome."] });
        }
    }
}
