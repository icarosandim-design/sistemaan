namespace SistemaAN.Domain.Common;

/// <summary>
/// Raiz de agregação. Acumula eventos de domínio para posterior despacho
/// (o despacho efetivo será implementado quando os módulos de negócio existirem).
/// </summary>
public abstract class AggregateRoot : AuditableEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
