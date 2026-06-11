namespace SistemaAN.Application.Common.Exceptions;

/// <summary>Falha de autenticação (mapeado para HTTP 401).</summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Credenciais inválidas.") : base(message) { }
}
