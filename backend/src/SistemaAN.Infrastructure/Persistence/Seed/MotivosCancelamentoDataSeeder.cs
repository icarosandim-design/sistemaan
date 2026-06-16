using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaAN.Domain.Clientes;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>Cria os motivos de cancelamento padrão na primeira execução. Idempotente — não sobrescreve edições do usuário.</summary>
public sealed class MotivosCancelamentoDataSeeder
{
    private static readonly string[] Padrao =
    [
        "Preço",
        "Pet não se adaptou",
        "Problema financeiro",
        "Mudança de cidade",
        "Atendimento",
        "Entrega",
        "Pediu pausa",
        "Falecimento do pet",
        "Outro",
    ];

    private readonly ApplicationDbContext _db;
    private readonly ILogger<MotivosCancelamentoDataSeeder> _logger;

    public MotivosCancelamentoDataSeeder(ApplicationDbContext db, ILogger<MotivosCancelamentoDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existentes = await _db.MotivosCancelamento.Select(m => m.Nome).ToListAsync(cancellationToken);

        var novos = new List<MotivoCancelamento>();
        for (var i = 0; i < Padrao.Length; i++)
        {
            if (!existentes.Contains(Padrao[i]))
            {
                novos.Add(MotivoCancelamento.Criar(Padrao[i], i));
            }
        }

        if (novos.Count > 0)
        {
            _db.MotivosCancelamento.AddRange(novos);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("{Qtd} motivos de cancelamento padrão criados.", novos.Count);
        }
    }
}
