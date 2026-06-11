namespace SistemaAN.Application.Common.Exceptions;

/// <summary>Recurso não encontrado (mapeado para HTTP 404).</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entity, object key)
        : base($"{entity} com identificador '{key}' não foi encontrado.") { }
}
