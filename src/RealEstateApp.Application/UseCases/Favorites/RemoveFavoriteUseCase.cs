using FluentValidation;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Favorites.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Favorites;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Favorites;

public sealed class RemoveFavoriteUseCase : IRemoveFavoriteUseCase
{
    private readonly IFavoritePropertyRepository _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<RemoveFavoriteRequest> _validator;
    private readonly ILogger<RemoveFavoriteUseCase> _logger;

    public RemoveFavoriteUseCase(
        IFavoritePropertyRepository favoriteRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IValidator<RemoveFavoriteRequest> validator,
        ILogger<RemoveFavoriteUseCase> logger
    )
    {
        _favoriteRepository = favoriteRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(
        RemoveFavoriteRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Client)))
            return Result.Failure(
                Error.Forbidden("Auth.ClientOnly", "Solo los clientes pueden eliminar favoritos.")
            );

        var clientId = _currentUser.UserId;

        var favorite = await _favoriteRepository.GetAsync(
            clientId,
            request.PropertyId,
            cancellationToken
        );
        if (favorite is null)
        {
            _logger.LogInformation(
                "Favorito {PropertyId} no existía al eliminar (idempotente), cliente {ClientId}.",
                request.PropertyId,
                clientId
            );
            return Result.Success();
        }

        _favoriteRepository.Delete(favorite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Favorito {PropertyId} eliminado por cliente {ClientId}.",
            request.PropertyId,
            clientId
        );

        return Result.Success();
    }
}
