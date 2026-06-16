using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>Cria as origens de venda padrão na primeira execução. Idempotente.</summary>
public sealed class OrigensVendaDataSeeder
{
    private static readonly string[] Padrao =
        ["Indicação", "Instagram", "Facebook", "Google", "WhatsApp", "Loja física", "Outro"];

    private readonly ApplicationDbContext _db;
    private readonly ILogger<OrigensVendaDataSeeder> _logger;

    public OrigensVendaDataSeeder(ApplicationDbContext db, ILogger<OrigensVendaDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existentes = await _db.OrigensVenda.Select(o => o.Nome).ToListAsync(cancellationToken);

        var novas = new List<OrigemVenda>();
        for (var i = 0; i < Padrao.Length; i++)
        {
            if (!existentes.Contains(Padrao[i]))
            {
                novas.Add(OrigemVenda.Criar(Padrao[i], i));
            }
        }

        if (novas.Count > 0)
        {
            _db.OrigensVenda.AddRange(novas);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("{Qtd} origens de venda padrão criadas.", novas.Count);
        }
    }
}
