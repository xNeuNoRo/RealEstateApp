namespace RealEstateApp.Application.Dtos.Admin.Requests;

public sealed record GetAgentsListRequest(
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
);
