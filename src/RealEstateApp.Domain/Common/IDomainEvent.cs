namespace RealEstateApp.Domain.Common;

/// <summary>
/// Marcador para eventos de dominio levantados por un AggregateRoot.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
