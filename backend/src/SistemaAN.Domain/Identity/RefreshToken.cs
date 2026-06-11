using SistemaAN.Domain.Common;

namespace SistemaAN.Domain.Identity;

/// <summary>
/// Refresh token persistido por usuário. Permite renovar o acesso (rotação) e
/// suporta logout/invalidação por revogação.
/// </summary>
public class RefreshToken : Entity
{
    private RefreshToken() { } // EF Core

    private RefreshToken(string token, long usuarioId, DateTimeOffset expiresAt, DateTimeOffset createdAt)
    {
        Token = token;
        UsuarioId = usuarioId;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
    }

    public string Token { get; private set; } = string.Empty;

    public long UsuarioId { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public bool Revogado => RevokedAt is not null;

    public bool Expirado(DateTimeOffset agora) => agora >= ExpiresAt;

    public bool Ativo(DateTimeOffset agora) => !Revogado && !Expirado(agora);

    public static RefreshToken Criar(string token, long usuarioId, DateTimeOffset expiresAt, DateTimeOffset createdAt)
        => new(token, usuarioId, expiresAt, createdAt);

    public void Revogar(DateTimeOffset agora) => RevokedAt ??= agora;
}
