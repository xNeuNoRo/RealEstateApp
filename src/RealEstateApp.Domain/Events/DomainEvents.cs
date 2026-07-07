using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Domain.Events;

public sealed record OfferAcceptedEvent(
    int OfferId,
    int PropertyId,
    string ClientId,
    decimal Amount,
    DateTimeOffset OccurredOn
) : IDomainEvent;

public sealed record PropertyCreatedEvent(
    int PropertyId,
    string AgentId,
    PropertyCode Code,
    DateTimeOffset OccurredOn
) : IDomainEvent;

public sealed record MessageSentEvent(
    int PropertyId,
    int MessageId,
    string ClientId,
    string AgentId,
    SenderType SenderType,
    DateTimeOffset OccurredOn
) : IDomainEvent;

public sealed record FavoriteAddedEvent(
    int FavoriteId,
    string ClientId,
    int PropertyId,
    DateTimeOffset OccurredOn
) : IDomainEvent;
