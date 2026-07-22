namespace RealEstateApp.Application.Dtos.Favorites.Requests;

public sealed record GetMyFavoritesRequest(int Page = 1, int PageSize = 20);
