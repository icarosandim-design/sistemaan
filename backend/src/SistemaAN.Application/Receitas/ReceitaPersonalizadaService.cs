using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Planos;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Receitas;

public sealed class ReceitaPersonalizadaService : IReceitaPersonalizadaService
{
    private readonly IApplicationDbContext _db;

    public ReceitaPersonalizadaService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ReceitaPersonalizadaDto>> ListarPorPetAsync(long petId, CancellationToken cancellationToken = default)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId, cancellationToken))
        {
            throw new NotFoundException("Pet", petId);
        }

        var receitas = await _db.Receitas
            .Where(r => r.Tipo == TipoReceita.Personalizada && r.PetId == petId)
            .Include(r => r.Itens)
            .OrderByDescending(r => r.Ativo)
            .ThenBy(r => r.Codigo)
            .ToListAsync(cancellationToken);

        var ingredientes = await CarregarIngredientesAsync(cancellationToken);
        return receitas.Select(r => Map(r, ingredientes)).ToList();
    }

    public async Task<ReceitaPersonalizadaDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var receita = await BuscarAsync(id, cancellationToken);
        var ingredientes = await CarregarIngredientesAsync(cancellationToken);
        return Map(receita, ingredientes);
    }

    public async Task<ReceitaPersonalizadaDto> CriarAsync(long petId, SalvarReceitaPersonalizadaRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == petId, cancellationToken))
        {
            throw new NotFoundException("Pet", petId);
        }

        await ValidarAsync(petId, request, null, cancellationToken);

        var receita = Receita.CriarPersonalizada(petId, request.Codigo, request.Nome, request.Observacoes);
        receita.DefinirAtivo(request.Ativo);
        receita.SubstituirItens(request.Itens.Select(i => ItemReceita.Criar(i.IngredienteId, i.Gramas)));

        _db.Receitas.Add(receita);
        await _db.SaveChangesAsync(cancellationToken);

        return await ObterAsync(receita.Id, cancellationToken);
    }

    public async Task<ReceitaPersonalizadaDto> AtualizarAsync(long id, SalvarReceitaPersonalizadaRequest request, CancellationToken cancellationToken = default)
    {
        var receita = await BuscarAsync(id, cancellationToken);
        await ValidarAsync(receita.PetId!.Value, request, id, cancellationToken);

        receita.Atualizar(request.Codigo, request.Nome, request.Observacoes, request.Ativo);
        receita.SubstituirItens(request.Itens.Select(i => ItemReceita.Criar(i.IngredienteId, i.Gramas)));

        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(receita.Id, cancellationToken);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var receita = await BuscarAsync(id, cancellationToken);

        if (!ativo && await PlanoUso.ReceitaEmPlanoAtivoAsync(_db, id, cancellationToken))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["ativo"] = ["Não é possível inativar: a receita está em um plano alimentar vigente."],
            });
        }

        receita.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Receita> BuscarAsync(long id, CancellationToken cancellationToken)
        => await _db.Receitas
            .Include(r => r.Itens)
            .FirstOrDefaultAsync(r => r.Id == id && r.Tipo == TipoReceita.Personalizada, cancellationToken)
            ?? throw new NotFoundException("Receita personalizada", id);

    private async Task ValidarAsync(long petId, SalvarReceitaPersonalizadaRequest request, long? idAtual, CancellationToken cancellationToken)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Codigo))
        {
            erros["codigo"] = ["Informe o código."];
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            erros["nome"] = ["Informe o nome."];
        }

        if (request.Itens.Count == 0)
        {
            erros["itens"] = ["Inclua ao menos um ingrediente."];
        }
        else if (request.Itens.Any(i => i.Gramas <= 0))
        {
            erros["itens"] = ["As gramas de cada ingrediente devem ser maiores que zero."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        // Código único por pet (entre as personalizadas do pet).
        var codigo = request.Codigo.Trim();
        var codigoEmUso = await _db.Receitas.AnyAsync(
            r => r.Tipo == TipoReceita.Personalizada && r.PetId == petId && r.Codigo == codigo
                 && (idAtual == null || r.Id != idAtual),
            cancellationToken);
        if (codigoEmUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["codigo"] = ["Já existe uma receita com este código para este pet."] });
        }

        // Ingredientes devem existir e estar ativos.
        var ids = request.Itens.Select(i => i.IngredienteId).Distinct().ToList();
        var ativos = await _db.Ingredientes.Where(i => ids.Contains(i.Id) && i.Ativo).Select(i => i.Id).ToListAsync(cancellationToken);
        if (ativos.Count != ids.Count)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["itens"] = ["Há ingrediente inexistente ou inativo na receita."] });
        }
    }

    private async Task<Dictionary<long, Ingrediente>> CarregarIngredientesAsync(CancellationToken cancellationToken)
        => await _db.Ingredientes.Include(i => i.Categoria).ToDictionaryAsync(i => i.Id, cancellationToken);

    private static ReceitaPersonalizadaDto Map(Receita r, IReadOnlyDictionary<long, Ingrediente> ingredientes)
    {
        var itens = new List<ItemReceitaDto>();
        var peso = 0;
        decimal custo = 0m;

        foreach (var it in r.Itens.OrderBy(i => i.Id))
        {
            ingredientes.TryGetValue(it.IngredienteId, out var ing);
            peso += it.Gramas;

            if (ing is not null && ing.CoeficienteConversao > 0)
            {
                custo += (it.Gramas / 1000m) * (ing.CustoAtualKg / ing.CoeficienteConversao);
            }

            itens.Add(new ItemReceitaDto(
                it.IngredienteId,
                ing?.Nome ?? "(removido)",
                ing?.Categoria?.Nome ?? string.Empty,
                it.Gramas));
        }

        var custoPacote = Math.Round(custo, 2);
        var custoPorKg = peso > 0 ? Math.Round(custo / (peso / 1000m), 2) : 0m;

        return new ReceitaPersonalizadaDto(
            r.Id, r.PetId ?? 0, r.Codigo, r.Nome, r.Ativo, r.Observacoes, itens, peso, custoPacote, custoPorKg);
    }
}
