namespace SistemaAN.Domain.Common;

/// <summary>
/// Base de todas as entidades persistidas. A chave primária é um <see cref="long"/>
/// (mapeado para <c>bigint generated always as identity</c> no PostgreSQL).
/// </summary>
public abstract class Entity
{
    public long Id { get; protected set; }
}
