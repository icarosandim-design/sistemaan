using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Infrastructure.Persistence.Seed;

/// <summary>
/// Garante a existência do papel "Administrador" e de um usuário administrador
/// inicial. Idempotente: pode rodar a cada inicialização sem duplicar dados.
/// Credenciais configuráveis por <c>Seed:AdminEmail</c> / <c>Seed:AdminPassword</c>.
/// </summary>
public sealed class IdentityDataSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<IdentityDataSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        // Garante os perfis fixos do sistema (Administrador, Operador, Cozinha).
        foreach (var (nome, descricao) in PapeisDoSistema.Todos)
        {
            if (!await _db.Papeis.AnyAsync(p => p.Nome == nome, cancellationToken))
            {
                _db.Papeis.Add(Papel.Criar(nome, descricao));
                _logger.LogInformation("Papel '{Papel}' criado.", nome);
            }
        }
        await _db.SaveChangesAsync(cancellationToken);

        var papelAdmin = await _db.Papeis
            .FirstAsync(p => p.Nome == PapeisDoSistema.Administrador, cancellationToken);

        var email = Usuario.Normalizar(_configuration["Seed:AdminEmail"] ?? "admin@sistemaan.local");

        if (!await _db.Usuarios.AnyAsync(u => u.Email == email, cancellationToken))
        {
            var senha = _configuration["Seed:AdminPassword"] ?? "Admin@123";

            var admin = Usuario.Criar("Administrador", email, _passwordHasher.Hash(senha));
            admin.AtribuirPapel(papelAdmin);

            _db.Usuarios.Add(admin);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Usuário administrador inicial criado: {Email}", email);
        }
    }
}
