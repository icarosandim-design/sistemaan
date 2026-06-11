using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaAN.Domain.Consumo;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>
/// Popula faixas de consumo de exemplo na primeira execução. Idempotente.
/// </summary>
public sealed class ConsumoDataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ConsumoDataSeeder> _logger;

    public ConsumoDataSeeder(ApplicationDbContext db, ILogger<ConsumoDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.FaixasConsumo.AnyAsync(cancellationToken))
        {
            return;
        }

        // Intervalos fechados que não se encostam (a próxima começa após o final).
        var faixas = new[]
        {
            FaixaConsumo.Criar(3m, 5m, 200),
            FaixaConsumo.Criar(6m, 8m, 290),
            FaixaConsumo.Criar(9m, 11m, 370),
            FaixaConsumo.Criar(12m, 14m, 440),
        };

        _db.FaixasConsumo.AddRange(faixas);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Qtd} faixas de consumo de exemplo criadas.", faixas.Length);
    }
}
