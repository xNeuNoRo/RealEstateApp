using Microsoft.Extensions.DependencyInjection;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Interfaces.Events;

namespace RealEstateApp.Infrastructure.Persistence.Services;

/// <summary>
/// Enruta eventos de dominio a sus manejadores registrados en DI.
/// Resuelve IEventHandler{TEvent} para cada evento y los ejecuta secuencialmente.
/// No crea scope propio para mantener la misma instancia de DbContext que el UnitOfWork.
/// </summary>
public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();

        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, ct);
        }
    }
}
