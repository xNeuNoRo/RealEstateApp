using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Adapters;
using RealEstateApp.Application.Dtos.Client.Requests;
using RealEstateApp.Application.Dtos.Favorites.Requests;
using RealEstateApp.Application.Dtos.Favorites.Responses;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Application.ViewModels.Client;
using RealEstateApp.Application.ViewModels.Property;
using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Settings;
using RealEstateApp.WebApp.Extensions;
using RealEstateApp.WebApp.Filters;

namespace RealEstateApp.WebApp.Controllers;

[SessionAuthorize]
[RoleAuthorize(nameof(Roles.Client))]
public sealed class ClientController : BaseController
{
    private const int DashboardPropertyCount = 3;
    private const int PageSize = 12;
    private const long ProfileUploadLimit = FileConstants.MaxImageFileSizeBytes + (1024 * 1024);

    private readonly IClientService _clientService;
    private readonly IGetPropertyListUseCase _getPropertyList;
    private readonly IViewModelBuilder<BaseViewModel> _viewModelBuilder;
    private readonly IMapper _mapper;

    public ClientController(
        ICurrentUserService currentUser,
        IClientService clientService,
        IGetPropertyListUseCase getPropertyList,
        IViewModelBuilder<BaseViewModel> viewModelBuilder,
        IMapper mapper
    )
        : base(currentUser)
    {
        _clientService = clientService;
        _getPropertyList = getPropertyList;
        _viewModelBuilder = viewModelBuilder;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var dashboardResult = await _clientService.GetDashboardAsync(cancellationToken);
        var propertiesResult = await _getPropertyList.ExecuteAsync(
            new GetPropertyListRequest(PageSize: DashboardPropertyCount),
            cancellationToken
        );

        var viewModel = dashboardResult.IsSuccess
            ? _mapper.Map<ClientDashboardViewModel>(dashboardResult.GetValue())
            : new ClientDashboardViewModel();

        if (dashboardResult.IsFailure)
            ModelState.AddModelError(string.Empty, dashboardResult.GetError().Message);

        if (propertiesResult.IsSuccess)
        {
            viewModel = new ClientDashboardViewModel
            {
                FavoritesCount = viewModel.FavoritesCount,
                TotalOffers = viewModel.TotalOffers,
                ActiveOffers = viewModel.ActiveOffers,
                ConversationsCount = viewModel.ConversationsCount,
                TotalMessages = viewModel.TotalMessages,
                AvailableProperties = _mapper.Map<IReadOnlyList<PropertyListItemViewModel>>(
                    propertiesResult.GetValue().Items
                ),
            };
        }
        else
        {
            ModelState.AddModelError(
                string.Empty,
                "No se pudieron cargar propiedades destacadas. Intenta nuevamente."
            );
        }

        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Favorites(int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var result = await _clientService.GetFavoritesAsync(
            new GetMyFavoritesRequest(page, PageSize),
            cancellationToken
        );

        if (result.IsSuccess && result.GetValue().TotalPages > 0 && page > result.GetValue().TotalPages)
            return RedirectToAction(nameof(Favorites), new { page = result.GetValue().TotalPages });

        var favorites = result.IsSuccess
            ? MapPage(result.GetValue())
            : new PagedResult<FavoriteListItemViewModel>([], 0, page, PageSize);
        if (result.IsFailure)
            ModelState.AddModelError(string.Empty, result.GetError().Message);

        var viewModel = new MyFavoritesViewModel { Favorites = favorites };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFavorite(
        int propertyId,
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _clientService.RemoveFavoriteAsync(propertyId, cancellationToken);
        if (result.IsSuccess)
            this.SetSuccessMessage("Propiedad eliminada de tus favoritos.");
        else
            this.SetErrorMessage(result.GetError().Message);

        return RedirectToAction(nameof(Favorites), new { page = Math.Max(1, page) });
    }

    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken = default)
    {
        var result = await _clientService.GetProfileAsync(cancellationToken);
        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            return RedirectToAction(nameof(Index));
        }

        var viewModel = _mapper.Map<ClientProfileViewModel>(result.GetValue());
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(CancellationToken cancellationToken = default)
    {
        var result = await _clientService.GetProfileAsync(cancellationToken);
        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            return RedirectToAction(nameof(Profile));
        }

        var profile = result.GetValue();
        var viewModel = new UpdateClientProfileViewModel
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
        UpdateClientProfileViewModel model,
        IFormFile? photoFile,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
            return View(model);
        }

        var result = await _clientService.UpdateProfileAsync(
            new UpdateClientProfileRequest(
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

    private PagedResult<FavoriteListItemViewModel> MapPage(PagedResult<FavoriteResponse> page) =>
        new(
            _mapper.Map<IReadOnlyList<FavoriteListItemViewModel>>(page.Items),
            page.TotalCount,
            page.Page,
            page.PageSize
        );
}
