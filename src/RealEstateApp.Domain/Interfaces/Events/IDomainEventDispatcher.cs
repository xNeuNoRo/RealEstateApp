using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Interfaces.Events;

/// <summary>
/// Enruta eventos de dominio levantados por agregados hacia sus manejadores.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IDomainEvent;
}
