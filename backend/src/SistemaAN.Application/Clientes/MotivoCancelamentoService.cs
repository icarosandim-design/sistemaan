using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Application.Clientes;

public sealed class MotivoCancelamentoService : IMotivoCancelamentoService
{
    private readonly IApplicationDbContext _db;

    public MotivoCancelamentoService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<MotivoCancelamentoDto>> ListarAsync(bool incluirInativos = false, CancellationToken cancellationToken = default)
    {
        var query = _db.MotivosCancelamento.AsQueryable();
        if (!incluirInativos)
        {
            query = query.Where(m => m.Ativo);
        }

        return await query
            .OrderBy(m => m.Ordem)
            .ThenBy(m => m.Nome)
            .Select(m => new MotivoCancelamentoDto(m.Id, m.Nome, m.Ordem, m.Ativo, m.Observacoes))
            .ToListAsync(cancellationToken);
    }

    public async Task<MotivoCancelamentoDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var m = await _db.MotivosCancelamento.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Motivo de cancelamento", id);
        return new MotivoCancelamentoDto(m.Id, m.Nome, m.Ordem, m.Ativo, m.Observacoes);
    }

    public async Task<MotivoCancelamentoDto> CriarAsync(SalvarMotivoCancelamentoRequest request, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(request, null, cancellationToken);

        var motivo = MotivoCancelamento.Criar(request.Nome, request.Ordem, request.Observacoes);
        motivo.DefinirAtivo(request.Ativo);

        _db.MotivosCancelamento.Add(motivo);
        await _db.SaveChangesAsync(cancellationToken);
        return new MotivoCancelamentoDto(motivo.Id, motivo.Nome, motivo.Ordem, motivo.Ativo, motivo.Observacoes);
    }

    public async Task<MotivoCancelamentoDto> AtualizarAsync(long id, SalvarMotivoCancelamentoRequest request, CancellationToken cancellationToken = default)
    {
        var motivo = await _db.MotivosCancelamento.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Motivo de cancelamento", id);

        await ValidarAsync(request, id, cancellationToken);

        motivo.Atualizar(request.Nome, request.Ordem, request.Ativo, request.Observacoes);
        await _db.SaveChangesAsync(cancellationToken);
        return new MotivoCancelamentoDto(motivo.Id, motivo.Nome, motivo.Ordem, motivo.Ativo, motivo.Observacoes);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var motivo = await _db.MotivosCancelamento.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Motivo de cancelamento", id);

        motivo.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarAsync(SalvarMotivoCancelamentoRequest request, long? idAtual, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Informe o nome do motivo."] });
        }

        var nome = request.Nome.Trim();
        var emUso = await _db.MotivosCancelamento.AnyAsync(m => m.Nome == nome && (idAtual == null || m.Id != idAtual), cancellationToken);
        if (emUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Já existe um motivo com este nome."] });
        }
    }
}
