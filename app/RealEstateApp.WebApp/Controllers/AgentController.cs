using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Adapters;
using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Application.Dtos.Chat.Requests;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Application.ViewModels.Agent;
using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Settings;
using RealEstateApp.WebApp.Extensions;
using RealEstateApp.WebApp.Filters;

namespace RealEstateApp.WebApp.Controllers;

[SessionAuthorize]
[RoleAuthorize(nameof(Roles.Agent))]
public sealed class AgentController : BaseController
{
    private const int DashboardPropertyCount = 3;
    private const long ProfileUploadLimit = FileConstants.MaxImageFileSizeBytes + (1024 * 1024);

    private readonly IAgentService _agentService;
    private readonly IGetAgentPropertiesUseCase _getAgentProperties;
    private readonly IViewModelBuilder<BaseViewModel> _viewModelBuilder;
    private readonly IMapper _mapper;

    public AgentController(
        ICurrentUserService currentUser,
        IAgentService agentService,
        IGetAgentPropertiesUseCase getAgentProperties,
        IViewModelBuilder<BaseViewModel> viewModelBuilder,
        IMapper mapper
    )
        : base(currentUser)
    {
        _agentService = agentService;
        _getAgentProperties = getAgentProperties;
        _viewModelBuilder = viewModelBuilder;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        // Both use cases share scoped persistence dependencies; keep calls sequential.
        var availableResult = await _getAgentProperties.ExecuteAsync(
            new GetAgentPropertiesRequest(1, DashboardPropertyCount, PropertyStatus.Available),
            cancellationToken
        );
        var soldResult = await _getAgentProperties.ExecuteAsync(
            new GetAgentPropertiesRequest(1, DashboardPropertyCount, PropertyStatus.Sold),
            cancellationToken
        );
        var chatResult = await _agentService.GetChatListAsync(
            new GetMyConversationsRequest(1, 1),
            cancellationToken
        );

        var viewModel = new AgentHomeViewModel
        {
            AvailableProperties = availableResult.IsSuccess
                ? _mapper.Map<IReadOnlyList<AgentPropertyListItemViewModel>>(
                    availableResult.GetValue().Items
                )
                : [],
            SoldProperties = soldResult.IsSuccess
                ? _mapper.Map<IReadOnlyList<AgentPropertyListItemViewModel>>(soldResult.GetValue().Items)
                : [],
            AvailablePropertiesCount = availableResult.IsSuccess
                ? availableResult.GetValue().TotalCount
                : 0,
            SoldPropertiesCount = soldResult.IsSuccess ? soldResult.GetValue().TotalCount : 0,
            ConversationsCount = chatResult.IsSuccess ? chatResult.GetValue().TotalCount : 0,
        };

        if (availableResult.IsFailure)
            ModelState.AddModelError(string.Empty, availableResult.GetError().Message);
        if (soldResult.IsFailure)
            ModelState.AddModelError(string.Empty, soldResult.GetError().Message);

        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken = default)
    {
        var result = await _agentService.GetProfileAsync(cancellationToken);
        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            return RedirectToAction(nameof(Index));
        }

        var viewModel = _mapper.Map<AgentProfileViewModel>(result.GetValue());
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken = default)
    {
        var result = await _agentService.GetProfileAsync(cancellationToken);
        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            return RedirectToAction(nameof(Profile));
        }

        var profile = result.GetValue();
        var viewModel = new UpdateAgentProfileViewModel
        {
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Phone = profile.Phone ?? string.Empty,
        };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(ProfileUploadLimit)]
    public async Task<IActionResult> Edit(
        UpdateAgentProfileViewModel model,
        IFormFile? photoFile,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
            return View(model);
        }

        var result = await _agentService.UpdateProfileAsync(
            new UpdateAgentProfileRequest(
                model.FirstName,
                model.LastName,
                model.Phone,
                photoFile.ToAppFile()
            ),
            cancellationToken
        );
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.GetError().Message);
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
            return View(model);
        }

        this.SetSuccessMessage("Perfil actualizado correctamente.");
        return RedirectToAction(nameof(Profile));
    }
}
