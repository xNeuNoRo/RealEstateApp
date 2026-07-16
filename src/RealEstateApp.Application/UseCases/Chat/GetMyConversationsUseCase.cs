using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Chat.Requests;
using RealEstateApp.Application.Dtos.Chat.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Chat;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Chat;

public sealed class GetMyConversationsUseCase : IGetMyConversationsUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetMyConversationsRequest> _validator;

    public GetMyConversationsUseCase(
        IMessageRepository messageRepository,
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetMyConversationsRequest> validator
    )
    {
        _messageRepository = messageRepository;
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PagedResult<ConversationSummaryResponse>>> ExecuteAsync(
        GetMyConversationsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<ConversationSummaryResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<ConversationSummaryResponse>>.Failure(
                Error.Unauthorized(
                    "Auth.NotAuthenticated",
                    "Debe iniciar sesión para ver conversaciones."
                )
            );

        if (
            !_currentUser.IsInRole(nameof(Roles.Client))
            && !_currentUser.IsInRole(nameof(Roles.Agent))
        )
            return Result<PagedResult<ConversationSummaryResponse>>.Failure(
                Error.Forbidden("Auth.NotAllowed", "No tienes permiso para ver conversaciones.")
            );

        IReadOnlyList<Message> allMessages;
        int totalConversations;

        if (_currentUser.IsInRole(nameof(Roles.Client)))
        {
            allMessages = await _messageRepository.GetAllAsync(
                new QueryOptions<Message>
                {
                    Filter = m => m.ClientId == _currentUser.UserId,
                    Includes = [m => m.Property],
                    OrderBy = q => q.OrderByDescending(m => m.CreatedAt),
                },
                cancellationToken
            );

            var grouped = allMessages
                .GroupBy(m => (m.PropertyId, m.AgentId))
                .Select(g => g.First())
                .ToList(); // ponytail: in-memory grouping, migrate to DB GROUP BY if >1000 messages/client

            totalConversations = grouped.Count;
            var page = grouped
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var otherUserIds = page.Select(m => m.AgentId).Distinct().ToList();
            var otherUsers =
                otherUserIds.Count > 0
                    ? await _userRepository.GetByIdsAsync(otherUserIds, cancellationToken)
                    : [];
            var otherUserMap = otherUsers.ToDictionary(
                u => u.Id,
                u => $"{u.FirstName} {u.LastName}".Trim()
            );

            var items = page.Select(m =>
                {
                    var response = _mapper.Map<ConversationSummaryResponse>(m);
                    response.OtherUserId = m.AgentId;
                    response.OtherUserRole = nameof(Roles.Agent);
                    response.OtherUserName = otherUserMap.TryGetValue(m.AgentId, out var name)
                        ? name
                        : string.Empty;
                    return response;
                })
                .ToList();

            return Result<PagedResult<ConversationSummaryResponse>>.Success(
                new PagedResult<ConversationSummaryResponse>(
                    items,
                    totalConversations,
                    request.Page,
                    request.PageSize
                )
            );
        }
        else
        {
            var properties = await _propertyRepository.GetByAgentAsync(
                _currentUser.UserId,
                ct: cancellationToken
            );
            var propertyIds = properties.Select(p => p.Id).ToList();

            if (propertyIds.Count == 0)
                return Result<PagedResult<ConversationSummaryResponse>>.Success(
                    new PagedResult<ConversationSummaryResponse>(
                        [],
                        0,
                        request.Page,
                        request.PageSize
                    )
                );

            allMessages = await _messageRepository.GetAllAsync(
                new QueryOptions<Message>
                {
                    Filter = m => propertyIds.Contains(m.PropertyId),
                    Includes = [m => m.Property],
                    OrderBy = q => q.OrderByDescending(m => m.CreatedAt),
                },
                cancellationToken
            );

            var grouped = allMessages
                .GroupBy(m => (m.PropertyId, m.ClientId))
                .Select(g => g.First())
                .ToList();

            totalConversations = grouped.Count;
            var page = grouped
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var otherUserIds = page.Select(m => m.ClientId).Distinct().ToList();
            var otherUsers =
                otherUserIds.Count > 0
                    ? await _userRepository.GetByIdsAsync(otherUserIds, cancellationToken)
                    : [];
            var otherUserMap = otherUsers.ToDictionary(
                u => u.Id,
                u => $"{u.FirstName} {u.LastName}".Trim()
            );

            var items = page.Select(m =>
                {
                    var response = _mapper.Map<ConversationSummaryResponse>(m);
                    response.OtherUserId = m.ClientId;
                    response.OtherUserRole = nameof(Roles.Client);
                    response.OtherUserName = otherUserMap.TryGetValue(m.ClientId, out var name)
                        ? name
                        : string.Empty;
                    return response;
                })
                .ToList();

            return Result<PagedResult<ConversationSummaryResponse>>.Success(
                new PagedResult<ConversationSummaryResponse>(
                    items,
                    totalConversations,
                    request.Page,
                    request.PageSize
                )
            );
        }
    }
}
