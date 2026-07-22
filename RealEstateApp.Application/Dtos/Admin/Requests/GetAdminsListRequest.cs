namespace RealEstateApp.Application.Dtos.Admin.Requests;

public sealed record GetAdminsListRequest(
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
);
