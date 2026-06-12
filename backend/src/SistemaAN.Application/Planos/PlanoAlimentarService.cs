using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Planos;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Planos;

public sealed class PlanoAlimentarService : IPlanoAlimentarService
{
    private readonly IApplicationDbContext _db;
    private readonly Entregas.IEntregaService _entregas;

    public PlanoAlimentarService(IApplicationDbContext db, Entregas.IEntregaService entregas)
    {
        _db = db;
        _entregas = entregas;
    }

    public async Task<PlanoAlimentarDto?> ObterPorPetAsync(long petId, CancellationToken cancellationToken = default)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId, cancellationToken))
        {
            throw new NotFoundException("Pet", petId);
        }

        var plano = await CarregarAsync(petId, cancellationToken);
        return plano is null ? null : Map(plano);
    }

    public async Task<PlanoAlimentarDto> SalvarAsync(long petId, SalvarPlanoRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId, cancellationToken))
        {
            throw new NotFoundException("Pet", petId);
        }

        var (tipo, itens) = await ValidarAsync(petId, request, cancellationToken);
        var dados = new DadosPlano(request.GramasDiaSugeridas, request.GramasDiaAjustadas, tipo);

        var plano = await CarregarAsync(petId, cancellationToken);
        if (plano is null)
        {
            plano = PlanoAlimentar.Criar(petId, dados);
            plano.SubstituirItens(itens);
            _db.PlanosAlimentares.Add(plano);
        }
        else
        {
            plano.Atualizar(dados);
            plano.DefinirAtivo(true);
            plano.SubstituirItens(itens);
        }

        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _entregas.RegerarFuturasDoPetAsync(petId, "sistema", cancellationToken);
        }
        catch
        {
            // geração automática best-effort
        }

        var salvo = await CarregarAsync(petId, cancellationToken);
        return Map(salvo!);
    }

    public async Task AlternarStatusAsync(long petId, bool ativo, CancellationToken cancellationToken = default)
    {
        var plano = await _db.PlanosAlimentares.FirstOrDefaultAsync(p => p.PetId == petId, cancellationToken)
            ?? throw new NotFoundException("Plano alimentar do pet", petId);
        plano.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<PlanoAlimentar?> CarregarAsync(long petId, CancellationToken cancellationToken)
        => await _db.PlanosAlimentares
            .Include(p => p.Itens)
            .ThenInclude(i => i.Pacotes)
            .FirstOrDefaultAsync(p => p.PetId == petId, cancellationToken);

    private async Task<(TipoReceita Tipo, List<PlanoItemReceita> Itens)> ValidarAsync(
        long petId, SalvarPlanoRequest request, CancellationToken cancellationToken)
    {
        var erros = new Dictionary<string, string[]>();

        if (!Enum.TryParse<TipoReceita>(request.Tipo, true, out var tipo))
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["tipo"] = ["Tipo de alimentação inválido."] });
        }

        if (request.Itens is null || request.Itens.Count == 0)
        {
            erros["itens"] = ["Inclua ao menos uma receita no plano."];
            throw new ValidationException(erros);
        }

        // Carrega receitas referenciadas.
        var receitaIds = request.Itens.Select(i => i.ReceitaId).Distinct().ToList();
        var receitas = await _db.Receitas
            .Where(r => receitaIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, cancellationToken);

        // Tamanhos de pacote ativos (para Casa).
        var tamanhoIds = request.Itens
            .SelectMany(i => i.Pacotes ?? [])
            .Select(p => p.TamanhoPacoteId)
            .Distinct()
            .ToList();
        var tamanhosAtivos = await _db.TamanhosPacote
            .Where(t => tamanhoIds.Contains(t.Id) && t.Ativo)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var itens = new List<PlanoItemReceita>();

        foreach (var item in request.Itens)
        {
            if (!receitas.TryGetValue(item.ReceitaId, out var receita))
            {
                erros["itens"] = ["Há receita inexistente no plano."];
                continue;
            }

            if (receita.Tipo != tipo)
            {
                erros["itens"] = ["Todas as receitas devem ser do mesmo tipo do plano (Casa ou Personalizada)."];
                continue;
            }

            if (tipo == TipoReceita.Casa)
            {
                if (item.QuantidadeCicloGramas is null or <= 0)
                {
                    erros["itens"] = ["Informe a quantidade no ciclo (g) de cada receita da casa."];
                }

                var pacotes = new List<PlanoItemPacote>();
                foreach (var pac in item.Pacotes ?? [])
                {
                    if (pac.Quantidade <= 0)
                    {
                        continue; // ignora pacotes zerados
                    }
                    if (!tamanhosAtivos.Contains(pac.TamanhoPacoteId))
                    {
                        erros["pacotes"] = ["Há tamanho de pacote inexistente ou inativo."];
                        continue;
                    }
                    pacotes.Add(PlanoItemPacote.Criar(pac.TamanhoPacoteId, pac.Quantidade));
                }

                itens.Add(PlanoItemReceita.Criar(item.ReceitaId, item.QuantidadeCicloGramas, null, pacotes));
            }
            else // Personalizada
            {
                if (receita.PetId != petId)
                {
                    erros["itens"] = ["A receita personalizada deve pertencer a este pet."];
                }
                if (item.QuantidadePacotes is null or <= 0)
                {
                    erros["itens"] = ["Informe a quantidade de pacotes de cada receita personalizada."];
                }
                if (item.Pacotes is { Count: > 0 })
                {
                    erros["pacotes"] = ["Receita personalizada não usa tamanhos de pacote padrão."];
                }

                itens.Add(PlanoItemReceita.Criar(item.ReceitaId, null, item.QuantidadePacotes, []));
            }
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        return (tipo, itens);
    }

    private static PlanoAlimentarDto Map(PlanoAlimentar p) => new(
        p.Id,
        p.PetId,
        p.GramasDiaSugeridas,
        p.GramasDiaAjustadas,
        p.Tipo.ToString(),
        p.Ativo,
        p.Itens
            .OrderBy(i => i.Id)
            .Select(i => new PlanoItemDto(
                i.Id,
                i.ReceitaId,
                i.QuantidadeCicloGramas,
                i.QuantidadePacotes,
                i.Pacotes.OrderBy(pp => pp.Id).Select(pp => new PlanoItemPacoteDto(pp.TamanhoPacoteId, pp.Quantidade)).ToList()))
            .ToList());
}
