namespace SistemaAN.Application.Identity;

/// <summary>
/// Configurações do JWT, vinculadas à seção <c>Jwt</c> da configuração
/// (a vinculação ocorre na camada de Infraestrutura).
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
