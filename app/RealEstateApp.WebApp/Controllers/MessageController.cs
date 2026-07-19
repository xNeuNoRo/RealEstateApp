using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Dtos.Chat.Requests;
using RealEstateApp.Application.Dtos.Chat.Responses;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Application.ViewModels.Chat;
using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.WebApp.Extensions;
using RealEstateApp.WebApp.Filters;

namespace RealEstateApp.WebApp.Controllers;

[SessionAuthorize]
public sealed class MessageController : BaseController
{
    private const int PageSize = 20;

    private readonly IClientService _clientService;
    private readonly IAgentService _agentService;
    private readonly IGetPropertyDetailUseCase _getPropertyDetail;
    private readonly IViewModelBuilder<BaseViewModel> _viewModelBuilder;
    private readonly IMapper _mapper;

    public MessageController(
        ICurrentUserService currentUser,
        IClientService clientService,
        IAgentService agentService,
        IGetPropertyDetailUseCase getPropertyDetail,
        IViewModelBuilder<BaseViewModel> viewModelBuilder,
        IMapper mapper
    )
        : base(currentUser)
    {
        _clientService = clientService;
        _agentService = agentService;
        _getPropertyDetail = getPropertyDetail;
        _viewModelBuilder = viewModelBuilder;
        _mapper = mapper;
    }

    private bool IsAgent => CurrentUser.IsInRole(nameof(Roles.Agent));

    [HttpGet]
    [RoleAuthorize(nameof(Roles.Client), nameof(Roles.Agent))]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var result = IsAgent
            ? await _agentService.GetChatListAsync(
                new GetMyConversationsRequest(page, PageSize),
                cancellationToken
            )
            : await _clientService.GetChatListAsync(
                new GetMyConversationsRequest(page, PageSize),
                cancellationToken
            );

        if (
            result.IsSuccess
            && result.GetValue().TotalPages > 0
            && page > result.GetValue().TotalPages
        )
            return RedirectToAction(nameof(Index), new { page = result.GetValue().TotalPages });

        var conversations = result.IsSuccess
            ? MapSummaryPage(result.GetValue())
            : EmptySummaryPage(page);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);

        var viewModel = new ChatListViewModel { Conversations = conversations };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    [RoleAuthorize(nameof(Roles.Client), nameof(Roles.Agent))]
    public async Task<IActionResult> Conversation(
        int propertyId,
        string? clientId = null,
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        if (propertyId <= 0)
            return RedirectToAction(nameof(Index));

        if (IsAgent && string.IsNullOrWhiteSpace(clientId))
            return RedirectToAction(nameof(Index));

        page = Math.Max(1, page);
        clientId = string.IsNullOrWhiteSpace(clientId) ? null : clientId.Trim();

        var propertyResult = await _getPropertyDetail.ExecuteAsync(
            new GetPropertyDetailRequest(propertyId),
            cancellationToken
        );
        if (propertyResult.IsFailure)
        {
            this.SetWarningMessage(propertyResult.GetError().Message);
            return RedirectToAction(nameof(Index));
        }

        var property = propertyResult.GetValue();
        var conversationResult = IsAgent
            ? await _agentService.GetConversationAsync(
                new GetConversationRequest(propertyId, clientId, page, PageSize),
                cancellationToken
            )
            : await _clientService.GetConversationAsync(
                new GetConversationRequest(propertyId, Page: page),
                cancellationToken
            );

        if (conversationResult.IsFailure)
        {
            if (conversationResult.GetError().Type == ErrorType.Forbidden)
            {
                this.SetErrorMessage(conversationResult.GetError().Message);
                return RedirectToAction(nameof(Index));
            }
            this.SetWarningMessage(conversationResult.GetError().Message);
            return RedirectToAction(nameof(Index));
        }

        var pageResult = conversationResult.GetValue();
        if (page <= 1 && pageResult.TotalPages > 1)
        {
            var lastPage = pageResult.TotalPages;
            conversationResult = IsAgent
                ? await _agentService.GetConversationAsync(
                    new GetConversationRequest(propertyId, clientId, lastPage, PageSize),
                    cancellationToken
                )
                : await _clientService.GetConversationAsync(
                    new GetConversationRequest(propertyId, Page: lastPage),
                    cancellationToken
                );
            pageResult = conversationResult.GetValue();
        }

        if (pageResult.TotalPages > 0 && page > pageResult.TotalPages)
            return RedirectToAction(
                nameof(Conversation),
                new { propertyId, clientId, page = pageResult.TotalPages }
            );

        var messages = MapMessagePage(pageResult);
        foreach (var m in messages.Items)
            m.IsFromCurrentUser = string.Equals(
                m.SenderId,
                CurrentUser.UserId,
                StringComparison.Ordinal
            );

        var otherRole = IsAgent ? nameof(Roles.Client) : nameof(Roles.Agent);
        var otherName = IsAgent
            ? messages.Items.FirstOrDefault(m => m.SenderType == SenderType.Client)?.SenderName
              ?? "Cliente"
            : property.AgentName ?? "Agente";

        var viewModel = new ConversationViewModel
        {
            PropertyId = propertyId,
            PropertyCode = property.Code,
            PropertyDescription = property.Description,
            OtherUserId = IsAgent ? (clientId ?? string.Empty) : (property.AgentId ?? string.Empty),
            OtherUserName = otherName,
            OtherUserRole = otherRole,
            Messages = messages,
            ReplyToMessageId = IsAgent ? messages.Items.LastOrDefault()?.Id : null,
        };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(nameof(Roles.Client))]
    public async Task<IActionResult> Send(
        int propertyId,
        string content,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _clientService.SendMessageAsync(
            new SendMessageRequest(propertyId, content),
            cancellationToken
        );

        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);
        else
            this.SetSuccessMessage("Mensaje enviado. El agente responderá pronto.");

        return RedirectToAction(nameof(Conversation), new { propertyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> Reply(
        int messageId,
        int propertyId,
        string clientId,
        string content,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _agentService.ReplyMessageAsync(
            new ReplyMessageRequest(messageId, content),
            cancellationToken
        );

        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);
        else
            this.SetSuccessMessage("Respuesta enviada al cliente.");

        return RedirectToAction(
            nameof(Conversation),
            new { propertyId, clientId }
        );
    }

    private PagedResult<ConversationSummaryViewModel> MapSummaryPage(
        PagedResult<ConversationSummaryResponse> page
    ) =>
        new(
            _mapper.Map<IReadOnlyList<ConversationSummaryViewModel>>(page.Items),
            page.TotalCount,
            page.Page,
            page.PageSize
        );

    private PagedResult<MessageViewModel> MapMessagePage(PagedResult<MessageResponse> page) =>
        new(
            _mapper.Map<IReadOnlyList<MessageViewModel>>(page.Items),
            page.TotalCount,
            page.Page,
            page.PageSize
        );

    private static PagedResult<ConversationSummaryViewModel> EmptySummaryPage(int page) =>
        new([], 0, page, PageSize);

    private static PagedResult<MessageViewModel> EmptyMessagePage(int page) =>
        new([], 0, page, PageSize);
}
