namespace RealEstateApp.Application.Dtos.Catalog.Requests;

public sealed record UpdatePropertyTypeRequest(int Id, string Name, string Description);
