using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaAN.Domain.Entregas;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>Cria as frequências de entrega padrão na primeira execução. Idempotente.</summary>
public sealed class FrequenciasDataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<FrequenciasDataSeeder> _logger;

    public FrequenciasDataSeeder(ApplicationDbContext db, ILogger<FrequenciasDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.FrequenciasEntrega.AnyAsync(cancellationToken))
        {
            return;
        }

        var frequencias = new[]
        {
            FrequenciaEntrega.Criar("Semanal", 7, "Entrega a cada 7 dias.", false),
            FrequenciaEntrega.Criar("Quinzenal", 14, "Entrega a cada 14 dias.", false),
            FrequenciaEntrega.Criar("Mensal operacional", 28, "Entrega a cada 28 dias (4 semanas).", false),
            FrequenciaEntrega.Criar("Personalizada", null, "Ciclo definido manualmente por pet.", true),
        };

        _db.FrequenciasEntrega.AddRange(frequencias);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Qtd} frequências de entrega de exemplo criadas.", frequencias.Length);
    }
}
