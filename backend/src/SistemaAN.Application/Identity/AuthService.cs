using Microsoft.EntityFrameworkCore;
using SistemaAN.Application.Common.Exceptions;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Identity.Models;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Application.Identity;

public sealed class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IDateTimeProvider _clock;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IDateTimeProvider clock,
        JwtSettings jwtSettings)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _clock = clock;
        _jwtSettings = jwtSettings;
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = Usuario.Normalizar(request.Email);

        var usuario = await _db.Usuarios
            .Include(u => u.Papeis)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        // Verifica a senha mesmo quando o usuário não existe não é trivial sem o hash;
        // a mensagem genérica evita enumeração de usuários.
        if (usuario is null || !_passwordHasher.Verify(request.Senha, usuario.SenhaHash))
        {
            throw new UnauthorizedException();
        }

        if (!usuario.Ativo)
        {
            throw new ForbiddenAccessException("Usuário inativo.");
        }

        return await EmitirTokensAsync(usuario, cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var agora = _clock.UtcNow;

        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (token is null || !token.Ativo(agora))
        {
            throw new UnauthorizedException("Refresh token inválido ou expirado.");
        }

        var usuario = await _db.Usuarios
            .Include(u => u.Papeis)
            .FirstOrDefaultAsync(u => u.Id == token.UsuarioId, cancellationToken);

        if (usuario is null || !usuario.Ativo)
        {
            throw new UnauthorizedException("Usuário indisponível.");
        }

        // Rotação: o refresh token usado é revogado e novos tokens são emitidos.
        token.Revogar(agora);

        return await EmitirTokensAsync(usuario, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);

        if (token is not null && !token.Revogado)
        {
            token.Revogar(_clock.UtcNow);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AuthResult> EmitirTokensAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        var agora = _clock.UtcNow;
        var papeis = usuario.Papeis.Select(p => p.Nome).ToArray();

        var accessToken = _tokenGenerator.GerarAccessToken(usuario, papeis);
        var refreshValue = _tokenGenerator.GerarRefreshToken();
        var refreshExpira = agora.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        usuario.AdicionarRefreshToken(RefreshToken.Criar(refreshValue, usuario.Id, refreshExpira, agora));

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            accessToken.Value,
            accessToken.ExpiresAt,
            refreshValue,
            refreshExpira,
            new UsuarioAutenticado(usuario.Id, usuario.Nome, usuario.Email, papeis));
    }
}
