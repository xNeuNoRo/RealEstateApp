namespace RealEstateApp.Application.Dtos.Admin.Requests;

public sealed record GetDevelopersListRequest(
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
);
