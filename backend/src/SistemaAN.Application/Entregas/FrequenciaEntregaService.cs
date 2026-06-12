using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Planos;
using SistemaAN.Domain.Entregas;

namespace SistemaAN.Application.Entregas;

public sealed class FrequenciaEntregaService : IFrequenciaEntregaService
{
    private readonly IApplicationDbContext _db;

    public FrequenciaEntregaService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<FrequenciaEntregaDto>> ListarAsync(CancellationToken cancellationToken = default)
        => await _db.FrequenciasEntrega
            .OrderBy(f => f.Personalizada)
            .ThenBy(f => f.DiasCiclo)
            .ThenBy(f => f.Nome)
            .Select(f => Map(f))
            .ToListAsync(cancellationToken);

    public async Task<FrequenciaEntregaDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var f = await _db.FrequenciasEntrega.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Frequência de entrega", id);
        return Map(f);
    }

    public async Task<FrequenciaEntregaDto> CriarAsync(SalvarFrequenciaEntregaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(request, null, cancellationToken);

        var f = FrequenciaEntrega.Criar(request.Nome, request.DiasCiclo, request.Descricao, request.Personalizada);
        f.DefinirAtivo(request.Ativo);

        _db.FrequenciasEntrega.Add(f);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(f);
    }

    public async Task<FrequenciaEntregaDto> AtualizarAsync(long id, SalvarFrequenciaEntregaRequest request, CancellationToken cancellationToken = default)
    {
        var f = await _db.FrequenciasEntrega.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Frequência de entrega", id);

        await ValidarAsync(request, id, cancellationToken);

        f.Atualizar(request.Nome, request.DiasCiclo, request.Descricao, request.Personalizada, request.Ativo);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(f);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var f = await _db.FrequenciasEntrega.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Frequência de entrega", id);

        if (!ativo && await PlanoUso.FrequenciaEmPlanoAtivoAsync(_db, id, cancellationToken))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["ativo"] = ["Não é possível inativar: a frequência está em um plano alimentar vigente."],
            });
        }

        f.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarAsync(SalvarFrequenciaEntregaRequest request, long? idAtual, CancellationToken cancellationToken)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            erros["nome"] = ["Informe o nome."];
        }

        if (!request.Personalizada && (request.DiasCiclo is null or <= 0))
        {
            erros["diasCiclo"] = ["A quantidade de dias deve ser maior que zero."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        var nome = request.Nome.Trim();
        var nomeEmUso = await _db.FrequenciasEntrega.AnyAsync(
            f => f.Nome == nome && (idAtual == null || f.Id != idAtual),
            cancellationToken);
        if (nomeEmUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Já existe uma frequência com este nome."] });
        }
    }

    private static FrequenciaEntregaDto Map(FrequenciaEntrega f)
        => new(f.Id, f.Nome, f.DiasCiclo, f.Descricao, f.Personalizada, f.Ativo);
}
