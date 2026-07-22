using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Domain.Common;

public sealed record OfferClientSummary(
    string ClientId,
    int OfferCount,
    decimal LastAmount,
    OfferStatus LastStatus,
    DateTimeOffset LastCreatedAt
);
