namespace SistemaAN.Application.Common.Exceptions;

/// <summary>Falha de validação de entrada (mapeado para HTTP 400).</summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Uma ou mais validações falharam.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors) : this()
    {
        Errors = errors;
    }
}
