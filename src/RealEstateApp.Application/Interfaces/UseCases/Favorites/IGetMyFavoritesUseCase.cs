using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Favorites.Requests;
using RealEstateApp.Application.Dtos.Favorites.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.UseCases.Favorites;

public interface IGetMyFavoritesUseCase
    : IUseCase<GetMyFavoritesRequest, PagedResult<FavoriteResponse>>;
