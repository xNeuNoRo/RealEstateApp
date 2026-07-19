using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Agent;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Application.ViewModels.Home;
using RealEstateApp.Application.ViewModels.Property;
using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.WebApp.Extensions;

namespace RealEstateApp.WebApp.Controllers;

[AllowAnonymous]
public sealed class HomeController : BaseController
{
    private const int PageSize = 12;

    private readonly IGetPropertyListUseCase _getPropertyList;
    private readonly IGetPropertyDetailUseCase _getPropertyDetail;
    private readonly IGetPublicAgentsUseCase _getPublicAgents;
    private readonly IGetPublicPropertyTypesUseCase _getPublicPropertyTypes;
    private readonly IViewModelBuilder<BaseViewModel> _viewModelBuilder;
    private readonly IMapper _mapper;

    public HomeController(
        ICurrentUserService currentUser,
        IGetPropertyListUseCase getPropertyList,
        IGetPropertyDetailUseCase getPropertyDetail,
        IGetPublicAgentsUseCase getPublicAgents,
        IGetPublicPropertyTypesUseCase getPublicPropertyTypes,
        IViewModelBuilder<BaseViewModel> viewModelBuilder,
        IMapper mapper
    )
        : base(currentUser)
    {
        _getPropertyList = getPropertyList;
        _getPropertyDetail = getPropertyDetail;
        _getPublicAgents = getPublicAgents;
        _getPublicPropertyTypes = getPublicPropertyTypes;
        _viewModelBuilder = viewModelBuilder;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] PropertyFilterViewModel filters,
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        page = Math.Max(1, page);
        filters.Code = filters.Code?.Trim();
        filters.PropertyTypes = await GetPropertyTypesAsync(cancellationToken);

        if (
            filters.PropertyTypeId.HasValue
            && !filters.PropertyTypes.Any(type => type.Id == filters.PropertyTypeId.Value)
        )
        {
            ModelState.AddModelError(
                nameof(filters.PropertyTypeId),
                "El tipo de propiedad seleccionado no existe."
            );
        }

        var properties = EmptyPage<PropertyListItemViewModel>(page);
        if (ModelState.IsValid)
        {
            var result = await _getPropertyList.ExecuteAsync(
                new GetPropertyListRequest(
                    Page: page,
                    PageSize: PageSize,
                    PriceMin: filters.PriceMin,
                    PriceMax: filters.PriceMax,
                    Bedrooms: filters.Bedrooms,
                    Bathrooms: filters.Bathrooms,
                    PropertyTypeId: filters.PropertyTypeId,
                    Code: filters.Code
                ),
                cancellationToken
            );

            if (result.IsSuccess)
            {
                properties = MapPage<PropertyListItemResponse, PropertyListItemViewModel>(result.GetValue());
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.GetError().Message);
            }
        }

        var viewModel = new HomeIndexViewModel { Filters = filters, Properties = properties };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
    {
        var result = await _getPropertyDetail.ExecuteAsync(
            new GetPropertyDetailRequest(id),
            cancellationToken
        );

        if (
            result.IsFailure
            || !string.Equals(
                result.GetValue().Status,
                nameof(PropertyStatus.Available),
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            this.SetWarningMessage("La propiedad solicitada no existe o no se encuentra disponible.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = _mapper.Map<PropertyDetailPublicViewModel>(result.GetValue());
        viewModel.PageTitle = $"{viewModel.PropertyTypeName} {viewModel.Code}";
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Agents(
        string? searchTerm,
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        page = Math.Max(1, page);
        var result = await _getPublicAgents.ExecuteAsync(
            new GetPublicAgentsRequest(searchTerm?.Trim(), Page: page, PageSize: PageSize),
            cancellationToken
        );

        var agents = result.IsSuccess
            ? MapPage<
                RealEstateApp.Application.Dtos.Agent.Responses.PublicAgentResponse,
                PublicAgentListItemViewModel
            >(result.GetValue())
            : EmptyPage<PublicAgentListItemViewModel>(page);

        if (result.IsFailure)
            ModelState.AddModelError(string.Empty, result.GetError().Message);

        var viewModel = new AgentsIndexViewModel { SearchTerm = searchTerm?.Trim(), Agents = agents };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AgentProperties(
        string? id,
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            this.SetWarningMessage("El agente solicitado no existe o no se encuentra disponible.");
            return RedirectToAction(nameof(Agents));
        }

        id = id.Trim();
        page = Math.Max(1, page);
        var agentResult = await _getPublicAgents.ExecuteAsync(
            new GetPublicAgentsRequest(AgentId: id, PageSize: 1),
            cancellationToken
        );

        if (agentResult.IsFailure || agentResult.GetValue().Items.Count == 0)
        {
            this.SetWarningMessage("El agente solicitado no existe o no se encuentra disponible.");
            return RedirectToAction(nameof(Agents));
        }

        var propertyResult = await _getPropertyList.ExecuteAsync(
            new GetPropertyListRequest(Page: page, PageSize: PageSize, AgentId: id),
            cancellationToken
        );

        if (propertyResult.IsFailure)
        {
            this.SetErrorMessage(propertyResult.GetError().Message);
            return RedirectToAction(nameof(Agents));
        }

        var viewModel = new PublicAgentPropertiesViewModel
        {
            Agent = _mapper.Map<PublicAgentListItemViewModel>(agentResult.GetValue().Items[0]),
            Properties = MapPage<PropertyListItemResponse, PropertyListItemViewModel>(
                propertyResult.GetValue()
            ),
        };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(
            "~/Views/Shared/Error.cshtml",
            new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, StatusCode = 500 }
        );

    private async Task<IReadOnlyList<SelectListItemViewModel>> GetPropertyTypesAsync(
        CancellationToken cancellationToken
    )
    {
        var result = await _getPublicPropertyTypes.ExecuteAsync(
            new GetPublicPropertyTypesRequest(),
            cancellationToken
        );

        return result.IsSuccess
            ? _mapper.Map<IReadOnlyList<SelectListItemViewModel>>(result.GetValue())
            : [];
    }

    private PagedResult<TDestination> MapPage<TSource, TDestination>(PagedResult<TSource> page)
    {
        return new PagedResult<TDestination>(
            _mapper.Map<IReadOnlyList<TDestination>>(page.Items),
            page.TotalCount,
            page.Page,
            page.PageSize
        );
    }

    private static PagedResult<T> EmptyPage<T>(int page) =>
        new(Array.Empty<T>(), 0, page, PageSize);
}
