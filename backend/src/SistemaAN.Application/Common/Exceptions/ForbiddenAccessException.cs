namespace SistemaAN.Application.Common.Exceptions;

/// <summary>Acesso negado por falta de permissão (mapeado para HTTP 403).</summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "Acesso negado.") : base(message) { }
}
