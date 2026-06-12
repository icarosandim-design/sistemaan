using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaAN.Domain.Catalog;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>
/// Garante as categorias iniciais de ingredientes e, na primeira execução,
/// popula ingredientes de exemplo. Idempotente.
/// </summary>
public sealed class CatalogDataSeeder
{
    private static readonly string[] CategoriasIniciais =
        ["Proteína", "Carboidrato", "Vegetal", "Óleo", "Suplemento", "Tempero"];

    private readonly ApplicationDbContext _db;
    private readonly ILogger<CatalogDataSeeder> _logger;

    public CatalogDataSeeder(ApplicationDbContext db, ILogger<CatalogDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existentes = await _db.CategoriasIngredientes
            .Select(c => c.Nome)
            .ToListAsync(cancellationToken);

        var novas = CategoriasIniciais
            .Where(nome => !existentes.Contains(nome))
            .Select(nome => CategoriaIngrediente.Criar(nome))
            .ToList();

        if (novas.Count > 0)
        {
            _db.CategoriasIngredientes.AddRange(novas);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("{Qtd} categorias de ingredientes criadas.", novas.Count);
        }

        if (!await _db.Ingredientes.AnyAsync(cancellationToken))
        {
            await SeedIngredientesExemploAsync(cancellationToken);
        }
    }

    private async Task SeedIngredientesExemploAsync(CancellationToken cancellationToken)
    {
        var cats = await _db.CategoriasIngredientes.ToDictionaryAsync(c => c.Nome, c => c.Id, cancellationToken);

        Ingrediente Novo(string nome, string cat, TipoConversao tipo, decimal coef, decimal custo, bool ativo = true)
        {
            var ing = Ingrediente.Criar(nome, cats[cat], tipo, coef, custo);
            ing.DefinirAtivo(ativo);
            return ing;
        }

        var exemplos = new[]
        {
            Novo("Batata-doce", "Carboidrato", TipoConversao.Perda, 0.55m, 6.50m),
            Novo("Arroz integral", "Carboidrato", TipoConversao.Ganho, 3.00m, 7.20m),
            Novo("Frango (peito)", "Proteína", TipoConversao.Perda, 0.70m, 18.90m),
            Novo("Carne bovina (patinho)", "Proteína", TipoConversao.Perda, 0.65m, 32.50m),
            Novo("Fígado bovino", "Proteína", TipoConversao.Perda, 0.72m, 19.00m),
            Novo("Abóbora", "Vegetal", TipoConversao.Perda, 0.80m, 4.80m),
            Novo("Cenoura", "Vegetal", TipoConversao.Perda, 0.88m, 5.50m),
            Novo("Aveia em flocos", "Carboidrato", TipoConversao.Ganho, 2.50m, 9.00m),
            Novo("Óleo de coco", "Óleo", TipoConversao.SemConversao, 1m, 39.90m),
            Novo("Sal", "Tempero", TipoConversao.SemConversao, 1m, 2.50m, ativo: false),
            Novo("Suplemento vitamínico", "Suplemento", TipoConversao.SemConversao, 1m, 120.00m),
        };

        _db.Ingredientes.AddRange(exemplos);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Qtd} ingredientes de exemplo criados.", exemplos.Length);
    }
}
