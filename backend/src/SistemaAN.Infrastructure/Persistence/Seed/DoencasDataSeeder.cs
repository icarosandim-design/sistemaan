using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaAN.Domain.Pets;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>Cria as doenças padrão na primeira execução. Idempotente — não sobrescreve edições do usuário.</summary>
public sealed class DoencasDataSeeder
{
    private static readonly string[] Padrao =
    [
        "Alergia alimentar",
        "Artrose / Problemas articulares",
        "Cardiopatia",
        "Dermatite",
        "Diabetes",
        "Doença renal",
        "Epilepsia",
        "Gastrite",
        "Hipotireoidismo",
        "Obesidade",
        "Pancreatite",
        "Problemas hepáticos",
        "Cálculo / Problemas urinários",
        "Sensibilidade digestiva",
        "Outra",
    ];

    private readonly ApplicationDbContext _db;
    private readonly ILogger<DoencasDataSeeder> _logger;

    public DoencasDataSeeder(ApplicationDbContext db, ILogger<DoencasDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existentes = await _db.Doencas.Select(d => d.Nome).ToListAsync(cancellationToken);

        var novas = new List<Doenca>();
        for (var i = 0; i < Padrao.Length; i++)
        {
            if (!existentes.Contains(Padrao[i]))
            {
                novas.Add(Doenca.Criar(Padrao[i], i));
            }
        }

        if (novas.Count > 0)
        {
            _db.Doencas.AddRange(novas);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("{Qtd} doenças padrão criadas.", novas.Count);
        }
    }
}
