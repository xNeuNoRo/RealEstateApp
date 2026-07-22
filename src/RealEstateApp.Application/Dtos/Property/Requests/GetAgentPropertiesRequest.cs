using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Dtos.Property.Requests;

public sealed record GetAgentPropertiesRequest(
    int Page = 1,
    int PageSize = 20,
    PropertyStatus? Status = null
);
