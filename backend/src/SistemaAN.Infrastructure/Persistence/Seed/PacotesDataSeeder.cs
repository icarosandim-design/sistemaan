using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaAN.Domain.Pacotes;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>Cria os tamanhos de pacote padrão na primeira execução. Idempotente.</summary>
public sealed class PacotesDataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PacotesDataSeeder> _logger;

    public PacotesDataSeeder(ApplicationDbContext db, ILogger<PacotesDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.TamanhosPacote.AnyAsync(cancellationToken))
        {
            return;
        }

        var tamanhos = new[]
        {
            TamanhoPacote.Criar("Pacote 250g", 250, null),
            TamanhoPacote.Criar("Pacote 500g", 500, null),
        };

        _db.TamanhosPacote.AddRange(tamanhos);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Qtd} tamanhos de pacote padrão criados.", tamanhos.Length);
    }
}
