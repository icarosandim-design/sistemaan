namespace SistemaAN.Domain.Common;

/// <summary>
/// Entidade com campos de auditoria temporal. Os timestamps são preenchidos
/// automaticamente pelo interceptor de persistência (ver Infrastructure).
/// </summary>
public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
