using SistemaAN.Domain.Identity;

namespace SistemaAN.Application.Common.Interfaces;

/// <summary>Token de acesso emitido, com seu instante de expiração.</summary>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>Emissão de tokens JWT de acesso e de refresh.</summary>
public interface IJwtTokenGenerator
{
    AccessToken GerarAccessToken(Usuario usuario, IEnumerable<string> papeis);

    string GerarRefreshToken();
}
