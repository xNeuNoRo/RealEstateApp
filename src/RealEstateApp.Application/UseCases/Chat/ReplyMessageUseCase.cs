using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Chat.Requests;
using RealEstateApp.Application.Dtos.Chat.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Chat;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Chat;

public sealed class ReplyMessageUseCase : IReplyMessageUseCase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<ReplyMessageRequest> _validator;
    private readonly ILogger<ReplyMessageUseCase> _logger;

    public ReplyMessageUseCase(
        IMessageRepository messageRepository,
        IPropertyRepository propertyRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<ReplyMessageRequest> validator,
        ILogger<ReplyMessageUseCase> logger)
    {
        _messageRepository = messageRepository;
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<MessageResponse>> ExecuteAsync(
        ReplyMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<MessageResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<MessageResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión para enviar un mensaje.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<MessageResponse>.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden responder mensajes.")
            );

        var originalMessage = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);
        if (originalMessage is null)
            return Result<MessageResponse>.Failure(
                Error.NotFound("Message.NotFound", "No se encontró el mensaje original para responder.")
            );

        var property = await _propertyRepository.GetByIdAsync(originalMessage.PropertyId, cancellationToken);
        if (property is null)
            return Result<MessageResponse>.Failure(
                Error.NotFound("Property.NotFound", "No se encontró la propiedad asociada.")
            );

        if (property.AgentId != _currentUser.UserId)
            return Result<MessageResponse>.Failure(
                Error.Forbidden("Property.NotOwner", "Solo el agente propietario puede responder mensajes de esta propiedad.")
            );

        var conversation = await _messageRepository.GetConversationAsync(
            originalMessage.PropertyId,
            originalMessage.ClientId,
            _currentUser.UserId,
            ct: cancellationToken
        );

        if (conversation.Count == 0)
            return Result<MessageResponse>.Failure(
                Error.NotFound("Conversation.NotFound", "No se encontró la conversación para responder.")
            );

        if (string.IsNullOrWhiteSpace(request.Content))
            return Result<MessageResponse>.Failure(
                Error.Validation("Message.ContentEmpty", "Debe escribir un mensaje antes de enviarlo.")
            );

        var createResult = Message.Create(
            originalMessage.PropertyId,
            originalMessage.ClientId,
            _currentUser.UserId,
            SenderType.Agent,
            request.Content
        );

        if (createResult.IsFailure)
            return Result<MessageResponse>.Failure(createResult.GetError());

        var message = createResult.GetValue();

        await _messageRepository.AddAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Mensaje respondido por agente {AgentId} al cliente {ClientId} en propiedad {PropertyId}.",
            _currentUser.UserId,
            originalMessage.ClientId,
            originalMessage.PropertyId
        );

        var response = _mapper.Map<MessageResponse>(message);
        response.SenderName = _currentUser.FullName ?? string.Empty;

        return Result<MessageResponse>.Success(response);
    }
}

        