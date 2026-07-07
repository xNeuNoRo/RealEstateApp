namespace RealEstateApp.Domain.Common;

/// <summary>
/// Base para Aggregate Roots. Mantiene eventos de dominio no confirmados
/// producidos por métodos de mutación.
/// </summary>
public abstract class AggregateRoot : BaseEntity<int>
{
    private readonly List<IDomainEvent> _events = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events.AsReadOnly();

    protected void RaiseEvent(IDomainEvent @event) => _events.Add(@event);

    public void ClearDomainEvents() => _events.Clear();
}
