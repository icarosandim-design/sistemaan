namespace SistemaAN.Application.Common.Interfaces;

/// <summary>Geração e verificação de hash de senha.</summary>
public interface IPasswordHasher
{
    string Hash(string senha);

    bool Verify(string senha, string hash);
}
