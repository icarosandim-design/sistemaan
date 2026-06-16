using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaAN.Domain.Pets;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>Cria as raças padrão na primeira execução. Idempotente — não sobrescreve edições do usuário.</summary>
public sealed class RacasDataSeeder
{
    // SRD (sem raça definida) em primeiro lugar; demais em ordem alfabética.
    private static readonly string[] Padrao =
    [
        "SRD (Sem Raça Definida)",
        "Akita",
        "Beagle",
        "Bichon Frisé",
        "Border Collie",
        "Boxer",
        "Buldogue Francês",
        "Buldogue Inglês",
        "Cavalier King Charles Spaniel",
        "Chihuahua",
        "Chow Chow",
        "Cocker Spaniel",
        "Dachshund (Salsicha)",
        "Dálmata",
        "Doberman",
        "Fila Brasileiro",
        "Golden Retriever",
        "Husky Siberiano",
        "Labrador Retriever",
        "Lhasa Apso",
        "Maltês",
        "Pastor Alemão",
        "Pastor de Shetland",
        "Pequinês",
        "Pinscher",
        "Pitbull",
        "Poodle",
        "Pug",
        "Rottweiler",
        "Schnauzer",
        "Shih Tzu",
        "Spitz Alemão (Lulu da Pomerânia)",
        "Staffordshire Bull Terrier",
        "Weimaraner",
        "Yorkshire Terrier",
    ];

    private readonly ApplicationDbContext _db;
    private readonly ILogger<RacasDataSeeder> _logger;

    public RacasDataSeeder(ApplicationDbContext db, ILogger<RacasDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existentes = await _db.Racas.Select(r => r.Nome).ToListAsync(cancellationToken);

        var novas = new List<Raca>();
        for (var i = 0; i < Padrao.Length; i++)
        {
            if (!existentes.Contains(Padrao[i]))
            {
                novas.Add(Raca.Criar(Padrao[i], i));
            }
        }

        if (novas.Count > 0)
        {
            _db.Racas.AddRange(novas);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("{Qtd} raças padrão criadas.", novas.Count);
        }
    }
}
