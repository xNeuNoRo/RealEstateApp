using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Client.Requests;
using RealEstateApp.Application.Dtos.Client.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Client;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Client;

public sealed class GetClientDashboardUseCase : IGetClientDashboardUseCase
{
    private readonly IFavoritePropertyRepository _favoriteRepo;
    private readonly IOfferRepository _offerRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetClientDashboardRequest> _validator;

    public GetClientDashboardUseCase(
        IFavoritePropertyRepository favoriteRepo,
        IOfferRepository offerRepo,
        IMessageRepository messageRepo,
        IUserRepository userRepository,
        ICurrentUserService currentUser,
        IValidator<GetClientDashboardRequest> validator
    )
    {
        _favoriteRepo = favoriteRepo;
        _offerRepo = offerRepo;
        _messageRepo = messageRepo;
        _userRepository = userRepository;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<ClientDashboardResponse>> ExecuteAsync(
        GetClientDashboardRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult<ClientDashboardResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<ClientDashboardResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Client)))
            return Result<ClientDashboardResponse>.Failure(
                Error.Forbidden(
                    "Auth.ClientOnly",
                    "Solo los clientes pueden acceder a este dashboard."
                )
            );

        var clientId = _currentUser.UserId;
        var activeAgentIds = await _userRepository.GetActiveIdsByRoleAsync(nameof(Roles.Agent), ct);

        var favoritesCount = await _favoriteRepo.CountAsync(
            fp =>
                fp.ClientId == clientId
                && fp.Property != null
                && fp.Property.Status == PropertyStatus.Available
                && activeAgentIds.Contains(fp.Property.AgentId),
            ct
        );

        var totalOffers = await _offerRepo.CountAsync(o => o.ClientId == clientId, ct);

        var activeOffers = await _offerRepo.CountAsync(
            o => o.ClientId == clientId && o.Status == OfferStatus.Pending,
            ct
        );

        var messages = await _messageRepo.GetByClientAsync(clientId, ct: ct);

        var distinctConversations = messages.GroupBy(m => new { m.PropertyId, m.AgentId }).Count();

        return Result<ClientDashboardResponse>.Success(
            new ClientDashboardResponse(
                favoritesCount,
                totalOffers,
                activeOffers,
                distinctConversations,
                messages.Count
            )
        );
    }
}
