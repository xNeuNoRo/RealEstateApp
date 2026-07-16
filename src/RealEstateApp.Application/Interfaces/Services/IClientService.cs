using RealEstateApp.Application.Dtos.Chat.Requests;
using RealEstateApp.Application.Dtos.Chat.Responses;
using RealEstateApp.Application.Dtos.Client.Requests;
using RealEstateApp.Application.Dtos.Client.Responses;
using RealEstateApp.Application.Dtos.Favorites.Requests;
using RealEstateApp.Application.Dtos.Favorites.Responses;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.Services;

public interface IClientService
{
    Task<Result<ClientDashboardResponse>> GetDashboardAsync(CancellationToken ct = default);
    Task<Result<ClientProfileResponse>> GetProfileAsync(CancellationToken ct = default);
    Task<Result<ClientProfileResponse>> UpdateProfileAsync(
        UpdateClientProfileRequest request,
        CancellationToken ct = default
    );
    Task<Result<PagedResult<FavoriteResponse>>> GetFavoritesAsync(
        GetMyFavoritesRequest request,
        CancellationToken ct = default
    );
    Task<Result<FavoriteResponse>> AddFavoriteAsync(int propertyId, CancellationToken ct = default);
    Task<Result> RemoveFavoriteAsync(int propertyId, CancellationToken ct = default);
    Task<Result<MessageResponse>> SendMessageAsync(
        SendMessageRequest request,
        CancellationToken ct = default
    );
    Task<Result<OfferResponse>> CreateOfferAsync(
        CreateOfferRequest request,
        CancellationToken ct = default
    );
    Task<Result<PagedResult<OfferResponse>>> GetMyOffersAsync(
        GetMyOffersRequest request,
        CancellationToken ct = default
    );
}
