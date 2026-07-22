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

public sealed class GetConversationUseCase : IGetConversationUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetConversationRequest> _validator;

    public GetConversationUseCase(
        IMessageRepository messageRepository,
        IPropertyRepository propertyRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetConversationRequest> validator
    )
    {
        _messageRepository = messageRepository;
        _propertyRepository = propertyRepository;
        _userRepository = userRepository;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PagedResult<MessageResponse>>> ExecuteAsync(
        GetConversationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PagedResult<MessageResponse>>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<PagedResult<MessageResponse>>.Failure(
                Error.Unauthorized(
                    "Auth.NotAuthenticated",
                    "Debe iniciar sesión para ver la conversación."
                )
            );

        var property = await _propertyRepository.GetByIdAsync(
            request.PropertyId,
            cancellationToken
        );
        if (property is null)
            return Result<PagedResult<MessageResponse>>.Failure(
                Error.NotFound("Property.NotFound", "La propiedad no existe.")
            );

        if (_currentUser.IsInRole(nameof(Roles.Agent)))
        {
            if (property.AgentId != _currentUser.UserId)
                return Result<PagedResult<MessageResponse>>.Failure(
                    Error.Forbidden("Auth.NotPropertyAgent", "No eres el agente de esta propiedad.")
                );
        }
        else if (!_currentUser.IsInRole(nameof(Roles.Client)))
        {
            return Result<PagedResult<MessageResponse>>.Failure(
                Error.Forbidden("Auth.NotAllowed", "No tienes permiso para ver esta conversación.")
            );
        }

        var options = new QueryOptions<Message>
        {
            Includes = [m => m.Property],
            OrderBy = q => q.OrderBy(m => m.CreatedAt),
            Skip = (request.Page - 1) * request.PageSize,
            Take = request.PageSize,
        };

        IReadOnlyList<Message> messages;
        int totalCount;

        if (_currentUser.IsInRole(nameof(Roles.Client)))
        {
            messages = await _messageRepository.GetConversationAsync(
                request.PropertyId,
                _currentUser.UserId,
                property.AgentId,
                options,
                cancellationToken
            );
            totalCount = await _messageRepository.CountAsync(
                m => m.PropertyId == request.PropertyId && m.ClientId == _currentUser.UserId,
                cancellationToken
            );
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(request.ClientId))
            {
                messages = await _messageRepository.GetConversationAsync(
                    request.PropertyId,
                    request.ClientId,
                    _currentUser.UserId,
                    options,
                    cancellationToken
                );
                totalCount = await _messageRepository.CountAsync(
                    m => m.PropertyId == request.PropertyId && m.ClientId == request.ClientId,
                    cancellationToken
                );
            }
            else
            {
                messages = await _messageRepository.GetByPropertyAsync(
                    request.PropertyId,
                    options,
                    cancellationToken
                );
                totalCount = await _messageRepository.CountAsync(
                    m => m.PropertyId == request.PropertyId,
                    cancellationToken
                );
            }
        }

        var userIds = messages
            .Select(m => m.ClientId)
            .Concat(messages.Select(m => m.AgentId))
            .Distinct()
            .ToList();

        var users =
            userIds.Count > 0
                ? await _userRepository.GetByIdsAsync(userIds, cancellationToken)
                : [];
        var userMap = users.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());

        var items = _mapper.Map<List<MessageResponse>>(messages);
        var msgById = messages.ToDictionary(m => m.Id);
        foreach (var item in items)
        {
            if (msgById.TryGetValue(item.Id, out var msg))
            {
                var senderId = msg.SenderType == SenderType.Client ? msg.ClientId : msg.AgentId;
                item.SenderId = senderId;
                if (userMap.TryGetValue(senderId, out var name))
                    item.SenderName = name;
            }
        }

        return Result<PagedResult<MessageResponse>>.Success(
            new PagedResult<MessageResponse>(items, totalCount, request.Page, request.PageSize)
        );
    }
}
