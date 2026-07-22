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
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Favorites;

public sealed class AddFavoriteUseCase : IAddFavoriteUseCase
{
    private readonly IFavoritePropertyRepository _favoriteRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<AddFavoriteRequest> _validator;

    public AddFavoriteUseCase(
        IFavoritePropertyRepository favoriteRepository,
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<AddFavoriteRequest> validator
    )
    {
        _favoriteRepository = favoriteRepository;
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<FavoriteResponse>> ExecuteAsync(
        AddFavoriteRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<FavoriteResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<FavoriteResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Client)))
            return Result<FavoriteResponse>.Failure(
                Error.Forbidden("Auth.ClientOnly", "Solo los clientes pueden agregar favoritos.")
            );

        var property = await _propertyRepository.GetByIdAsync(
            request.PropertyId,
            cancellationToken
        );
        if (property is null)
            return Result<FavoriteResponse>.Failure(
                Error.NotFound("Property.NotFound", "La propiedad no existe.")
            );

        if (property.Status != PropertyStatus.Available)
            return Result<FavoriteResponse>.Failure(
                Error.Conflict(
                    "Property.NotAvailable",
                    "La propiedad debe estar disponible para agregarla a favoritos."
                )
            );

        var agent = await _userRepository.GetByIdAsync(property.AgentId, cancellationToken);
        if (agent is null || !agent.IsActive)
            return Result<FavoriteResponse>.Failure(
                Error.NotFound("Property.NotFound", "La propiedad no existe o no está disponible.")
            );

        var clientId = _currentUser.UserId;
        var isFavorited = await _favoriteRepository.IsFavoritedAsync(
            clientId,
            request.PropertyId,
            cancellationToken
        );
        if (isFavorited)
        {
            var existing = await _favoriteRepository.GetAsync(
                clientId,
                request.PropertyId,
                cancellationToken
            );
            var existingResponse = _mapper.Map<FavoriteResponse>(property);
            existingResponse.Id = existing?.Id ?? 0;
            existingResponse.FavoritedAt = existing?.CreatedAt ?? DateTimeOffset.UtcNow;
            return Result<FavoriteResponse>.Success(existingResponse);
        }

        var createResult = FavoriteProperty.Create(clientId, request.PropertyId);
        if (createResult.IsFailure)
            return Result<FavoriteResponse>.Failure(createResult.GetError());

        var favorite = createResult.GetValue();
        await _favoriteRepository.AddAsync(favorite, cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            var concurrentFavorite = await _favoriteRepository.GetAsync(
                clientId,
                request.PropertyId,
                cancellationToken
            );
            if (concurrentFavorite is null)
                throw;
            favorite = concurrentFavorite;
        }

        var response = _mapper.Map<FavoriteResponse>(property);
        response.Id = favorite.Id;
        response.FavoritedAt = favorite.CreatedAt;

        return Result<FavoriteResponse>.Success(response);
    }
}
