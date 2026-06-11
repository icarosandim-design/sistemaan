using System.Security.Cryptography;
using SistemaAN.Application.Common.Interfaces;

namespace SistemaAN.Infrastructure.Identity;

/// <summary>
/// Hash de senha com PBKDF2 (HMAC-SHA256), sem dependências externas.
/// Formato armazenado: <c>iteracoes:saltBase64:hashBase64</c>.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private const char Delimiter = ':';
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, Algorithm, KeySize);

        return string.Join(
            Delimiter,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    public bool Verify(string senha, string hash)
    {
        var parts = hash.Split(Delimiter);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var key = Convert.FromBase64String(parts[2]);
        var attempt = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iterations, Algorithm, key.Length);

        return CryptographicOperations.FixedTimeEquals(attempt, key);
    }
}
