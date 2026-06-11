using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Consumo;

namespace SistemaAN.Application.Consumo;

public sealed class FaixaConsumoService : IFaixaConsumoService
{
    private readonly IApplicationDbContext _db;

    public FaixaConsumoService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<FaixaConsumoDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _db.FaixasConsumo
            .OrderBy(f => f.PesoInicial)
            .Select(f => Map(f))
            .ToListAsync(cancellationToken);
    }

    public async Task<FaixaConsumoDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var faixa = await _db.FaixasConsumo.FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            ?? throw new NotFoundException("Faixa de consumo", id);
        return Map(faixa);
    }

    public async Task<FaixaConsumoDto?> ConsultarPorPesoAsync(decimal peso, CancellationToken cancellationToken = default)
    {
        var faixa = await _db.FaixasConsumo
            .Where(f => f.Ativo && f.PesoInicial <= peso && peso <= f.PesoFinal)
            .OrderBy(f => f.PesoInicial)
            .FirstOrDefaultAsync(cancellationToken);

        return faixa is null ? null : Map(faixa);
    }

    public async Task<FaixaConsumoDto> CriarAsync(SalvarFaixaConsumoRequest request, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(request, null, cancellationToken);

        var faixa = FaixaConsumo.Criar(request.PesoInicial, request.PesoFinal, request.GramasPorDia);
        faixa.DefinirAtivo(request.Ativo);

        _db.FaixasConsumo.Add(faixa);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(faixa);
    }

    public async Task<FaixaConsumoDto> AtualizarAsync(long id, SalvarFaixaConsumoRequest request, CancellationToken cancellationToken = default)
    {
        var faixa = await _db.FaixasConsumo.FirstOrDefaultAsync(f => f.Id == id, cancellationToken)
            ?? throw new NotFoundException("Faixa de consumo", id);

        await ValidarAsync(request, id, cancellationToken);

        faixa.Atualizar(request.PesoInicial, request.PesoFinal, request.GramasPorDia, request.Ativo);
        await _db.SaveChangesAsync(cancellationToken);

        return Map(faixa);
    }

    private async Task ValidarAsync(SalvarFaixaConsumoRequest request, long? idAtual, CancellationToken cancellationToken)
    {
        var erros = new Dictionary<string, string[]>();

        if (request.PesoInicial <= 0)
        {
            erros["pesoInicial"] = ["O peso inicial deve ser maior que zero."];
        }

        if (request.PesoFinal <= request.PesoInicial)
        {
            erros["pesoFinal"] = ["O peso final deve ser maior que o peso inicial."];
        }

        if (request.GramasPorDia <= 0)
        {
            erros["gramasPorDia"] = ["As gramas por dia devem ser maiores que zero."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        // Sobreposição apenas entre faixas ATIVAS (regra de negócio).
        if (request.Ativo)
        {
            // Intervalo fechado: faixas ativas não podem se sobrepor NEM se encostar
            // (comparações com <=, então tocar no limite já é conflito).
            var conflito = await _db.FaixasConsumo.AnyAsync(
                f => f.Ativo
                    && (idAtual == null || f.Id != idAtual)
                    && f.PesoInicial <= request.PesoFinal
                    && request.PesoInicial <= f.PesoFinal,
                cancellationToken);

            if (conflito)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["pesoInicial"] = ["Esta faixa se sobrepõe ou encosta em outra faixa ativa."],
                });
            }
        }
    }

    private static FaixaConsumoDto Map(FaixaConsumo f) =>
        new(f.Id, f.PesoInicial, f.PesoFinal, f.GramasPorDia, f.Ativo);
}
