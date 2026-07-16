using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Favorites.Requests;
using RealEstateApp.Application.Dtos.Favorites.Responses;

namespace RealEstateApp.Application.Interfaces.UseCases.Favorites;

public interface IAddFavoriteUseCase : IUseCase<AddFavoriteRequest, FavoriteResponse>;
