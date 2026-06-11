using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SistemaAN.Application.Common.Interfaces;
using SistemaAN.Application.Identity;
using SistemaAN.Domain.Identity;

namespace SistemaAN.Infrastructure.Identity;

/// <summary>Emissão de JWT (HMAC-SHA256) e geração de refresh tokens aleatórios.</summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    /// <summary>Tipo de claim usado para papéis (alinhado ao RoleClaimType configurado no JwtBearer).</summary>
    public const string RoleClaimType = "role";

    private readonly JwtSettings _settings;

    public JwtTokenGenerator(JwtSettings settings) => _settings = settings;

    public AccessToken GerarAccessToken(Usuario usuario, IEnumerable<string> papeis)
    {
        var agora = DateTimeOffset.UtcNow;
        var expiraEm = agora.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new("nome", usuario.Nome),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(papeis.Select(p => new Claim(RoleClaimType, p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = agora.UtcDateTime,
            IssuedAt = agora.UtcDateTime,
            Expires = expiraEm.UtcDateTime,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new AccessToken(token, expiraEm);
    }

    public string GerarRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
