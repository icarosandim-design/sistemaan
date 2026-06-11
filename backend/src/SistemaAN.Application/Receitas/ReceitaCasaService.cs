using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Catalog;
using SistemaAN.Domain.Receitas;

namespace SistemaAN.Application.Receitas;

public sealed class ReceitaCasaService : IReceitaCasaService
{
    private const int BaseGramas = 1000;

    private readonly IApplicationDbContext _db;

    public ReceitaCasaService(IApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<ReceitaCasaDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var receitas = await _db.Receitas
            .Where(r => r.Tipo == TipoReceita.Casa)
            .Include(r => r.Itens)
            .OrderBy(r => r.Nome)
            .ToListAsync(cancellationToken);

        var ingredientes = await CarregarIngredientesAsync(cancellationToken);
        return receitas.Select(r => Map(r, ingredientes)).ToList();
    }

    public async Task<ReceitaCasaDto> ObterAsync(long id, CancellationToken cancellationToken = default)
    {
        var receita = await BuscarAsync(id, cancellationToken);
        var ingredientes = await CarregarIngredientesAsync(cancellationToken);
        return Map(receita, ingredientes);
    }

    public async Task<ReceitaCasaDto> CriarAsync(SalvarReceitaCasaRequest request, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(request, null, cancellationToken);

        var receita = Receita.CriarCasa(request.Codigo, request.Nome, request.Observacoes);
        receita.DefinirAtivo(request.Ativo);
        receita.SubstituirItens(request.Itens.Select(i => ItemReceita.Criar(i.IngredienteId, i.Gramas)));

        _db.Receitas.Add(receita);
        await _db.SaveChangesAsync(cancellationToken);

        return await ObterAsync(receita.Id, cancellationToken);
    }

    public async Task<ReceitaCasaDto> AtualizarAsync(long id, SalvarReceitaCasaRequest request, CancellationToken cancellationToken = default)
    {
        var receita = await BuscarAsync(id, cancellationToken);
        await ValidarAsync(request, id, cancellationToken);

        receita.Atualizar(request.Codigo, request.Nome, request.Observacoes, request.Ativo);
        receita.SubstituirItens(request.Itens.Select(i => ItemReceita.Criar(i.IngredienteId, i.Gramas)));

        await _db.SaveChangesAsync(cancellationToken);
        return await ObterAsync(receita.Id, cancellationToken);
    }

    public async Task AlternarStatusAsync(long id, bool ativo, CancellationToken cancellationToken = default)
    {
        var receita = await BuscarAsync(id, cancellationToken);
        receita.DefinirAtivo(ativo);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Receita> BuscarAsync(long id, CancellationToken cancellationToken)
        => await _db.Receitas
            .Include(r => r.Itens)
            .FirstOrDefaultAsync(r => r.Id == id && r.Tipo == TipoReceita.Casa, cancellationToken)
            ?? throw new NotFoundException("Receita da casa", id);

    private async Task ValidarAsync(SalvarReceitaCasaRequest request, long? idAtual, CancellationToken cancellationToken)
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

        if (request.Itens.Any(i => i.Gramas <= 0))
        {
            erros["itens"] = ["As gramas de cada ingrediente devem ser maiores que zero."];
        }

        var total = request.Itens.Sum(i => i.Gramas);
        if (request.Itens.Count > 0 && total != BaseGramas)
        {
            erros["total"] = [$"A soma deve ser exatamente {BaseGramas} g (atual: {total} g)."];
        }

        if (erros.Count > 0)
        {
            throw new ValidationException(erros);
        }

        // Código único entre receitas da casa.
        var codigo = request.Codigo.Trim();
        var codigoEmUso = await _db.Receitas.AnyAsync(
            r => r.Tipo == TipoReceita.Casa && r.Codigo == codigo && (idAtual == null || r.Id != idAtual),
            cancellationToken);
        if (codigoEmUso)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["codigo"] = ["Já existe uma receita com este código."] });
        }

        // Ingredientes devem existir.
        var ids = request.Itens.Select(i => i.IngredienteId).Distinct().ToList();
        var existentes = await _db.Ingredientes.Where(i => ids.Contains(i.Id)).Select(i => i.Id).ToListAsync(cancellationToken);
        if (existentes.Count != ids.Count)
        {
            throw new ValidationException(new Dictionary<string, string[]> { ["itens"] = ["Há ingrediente inexistente na ficha técnica."] });
        }
    }

    private async Task<Dictionary<long, Ingrediente>> CarregarIngredientesAsync(CancellationToken cancellationToken)
        => await _db.Ingredientes.Include(i => i.Categoria).ToDictionaryAsync(i => i.Id, cancellationToken);

    private static ReceitaCasaDto Map(Receita r, IReadOnlyDictionary<long, Ingrediente> ingredientes)
    {
        var itens = new List<ItemReceitaDto>();
        var rendimento = 0;
        decimal custo = 0m;

        foreach (var it in r.Itens.OrderBy(i => i.Id))
        {
            ingredientes.TryGetValue(it.IngredienteId, out var ing);
            rendimento += it.Gramas;

            if (ing is not null && ing.CoeficienteConversao > 0)
            {
                // custo real/kg cozido = custo/kg cru ÷ rendimento do ingrediente
                custo += (it.Gramas / 1000m) * (ing.CustoAtualKg / ing.CoeficienteConversao);
            }

            itens.Add(new ItemReceitaDto(
                it.IngredienteId,
                ing?.Nome ?? "(removido)",
                ing?.Categoria?.Nome ?? string.Empty,
                it.Gramas));
        }

        var custoTotal = Math.Round(custo, 2);
        var custoPorKg = rendimento > 0 ? Math.Round(custo / (rendimento / 1000m), 2) : 0m;

        return new ReceitaCasaDto(r.Id, r.Codigo, r.Nome, r.Ativo, r.Observacoes, itens, rendimento, custoTotal, custoPorKg);
    }
}
