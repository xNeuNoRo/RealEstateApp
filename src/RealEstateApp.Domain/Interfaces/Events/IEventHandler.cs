using RealEstateApp.Domain.Common;

namespace RealEstateApp.Domain.Interfaces.Events;

/// <summary>
/// Maneja un tipo de evento de dominio. Resuelto desde DI en la capa de Application.
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
