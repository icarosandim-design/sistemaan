using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Pacotes;

namespace SistemaAN.Application.Pacotes;

public sealed class TamanhoPacoteService : ITamanhoPacoteService
{
    private readonly IApplicationDbContext _db;

    public TamanhoPacoteService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<TamanhoPacoteDto>> ListarAsync(CancellationToken cancellationToken = default)
        => await _db.TamanhosPacote
            .OrderBy(t => t.PesoGramas)
            .ThenBy(t => t.Nome)
            .Select(t => Map(t))
            .ToListAsync(cancellationToken);

    public async Task<TamanhoPacoteDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var t = await _db.TamanhosPacote.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Tamanho de pacote", id);
        return Map(t);
    }

    public async Task<TamanhoPacoteDto> CriarAsync(SalvarTamanhoPacoteRequest request, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(request, null, cancellationToken);

        var tamanho = TamanhoPacote.Criar(request.Nome, request.PesoGramas, request.Observacao);
        tamanho.DefinirAtivo(request.Ativo);

        _db.TamanhosPacote.Add(tamanho);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(tamanho);
    }

    public async Task<TamanhoPacoteDto> AtualizarAsync(long id, SalvarTamanhoPacoteRequest request, CancellationToken cancellationToken = default)
    {
        var tamanho = await _db.TamanhosPacote.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Tamanho de pacote", id);

        await ValidarAsync(request, id, cancellationToken);

        tamanho.Atualizar(request.Nome, request.PesoGramas, request.Observacao, request.Ativo);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(tamanho);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var tamanho = await _db.TamanhosPacote.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Tamanho de pacote", id);
        tamanho.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarAsync(SalvarTamanhoPacoteRequest request, long? idAtual, CancellationToken cancellationToken)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            erros["nome"] = ["Informe o nome do pacote."];
        }

        if (request.PesoGramas <= 0)
        {
            erros["pesoGramas"] = ["O peso em gramas deve ser maior que zero."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        var nome = request.Nome.Trim();
        var nomeEmUso = await _db.TamanhosPacote.AnyAsync(
            t => t.Nome == nome && (idAtual == null || t.Id != idAtual),
            cancellationToken);
        if (nomeEmUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["nome"] = ["Já existe um tamanho com este nome."] });
        }
    }

    private static TamanhoPacoteDto Map(TamanhoPacote t)
        => new(t.Id, t.Nome, t.PesoGramas, t.Ativo, t.Observacao);
}
