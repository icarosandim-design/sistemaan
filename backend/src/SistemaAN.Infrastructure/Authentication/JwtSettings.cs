namespace SistemaAN.Infrastructure.Authentication;

/// <summary>
/// Configurações do JWT, vinculadas à seção <c>Jwt</c> da configuração.
/// A emissão de tokens e o módulo de autenticação/permissões serão construídos
/// sobre estas configurações na próxima etapa.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; init; } = 15;

    public int RefreshTokenExpirationDays { get; init; } = 7;
}
