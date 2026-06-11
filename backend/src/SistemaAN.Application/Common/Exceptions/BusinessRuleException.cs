namespace SistemaAN.Application.Common.Exceptions;

/// <summary>Violação de regra de negócio (mapeado para HTTP 409).</summary>
public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
