using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Favorites.Requests;
using RealEstateApp.Application.Dtos.Favorites.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Favorites;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Favorites;

public sealed class GetMyFavoritesUseCase : IGetMyFavoritesUseCase
{
    private readonly IFavoritePropertyRepository _favoriteRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetMyFavoritesRequest> _validator;

    public GetMyFavoritesUseCase(
        IFavoritePropertyRepository favoriteRepository,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetMyFavoritesRequest> validator
    )
    {
        _favoriteRepository = favoriteRepository;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PagedResult<FavoriteResponse>>> ExecuteAsync(
        GetMyFavoritesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<FavoriteResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<FavoriteResponse>>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Client)))
            return Result<PagedResult<FavoriteResponse>>.Failure(
                Error.Forbidden("Auth.ClientOnly", "Solo los clientes pueden ver sus favoritos.")
            );

        var clientId = _currentUser.UserId;

        var options = new QueryOptions<FavoriteProperty>
        {
            Filter = fp => fp.Property != null && fp.Property.Status == PropertyStatus.Available,
            Includes = [fp => fp.Property],
            OrderBy = q => q.OrderByDescending(fp => fp.CreatedAt),
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
        };

        var favorites = await _favoriteRepository.GetByUserAsync(
            clientId,
            options,
            cancellationToken
        );

        var totalCount = await _favoriteRepository.CountAsync(
            fp =>
                fp.ClientId == clientId
                && fp.Property != null
                && fp.Property.Status == PropertyStatus.Available,
            cancellationToken
        );

        var items = favorites.Select(fp => _mapper.Map<FavoriteResponse>(fp)).ToList();

        return Result<PagedResult<FavoriteResponse>>.Success(
            new PagedResult<FavoriteResponse>(items, totalCount, request.Page, request.PageSize)
        );
    }
}
