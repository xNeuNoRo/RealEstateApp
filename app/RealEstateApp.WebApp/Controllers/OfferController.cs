using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Application.ViewModels.Offers;
using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.WebApp.Extensions;
using RealEstateApp.WebApp.Filters;

namespace RealEstateApp.WebApp.Controllers;

[SessionAuthorize]
public sealed class OfferController : BaseController
{
    private const int PageSize = 12;

    private readonly IClientService _clientService;
    private readonly IAgentService _agentService;
    private readonly IGetPropertyDetailUseCase _getPropertyDetail;
    private readonly IViewModelBuilder<BaseViewModel> _viewModelBuilder;
    private readonly IMapper _mapper;

    public OfferController(
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

    [HttpGet]
    [RoleAuthorize(nameof(Roles.Client))]
    public async Task<IActionResult> MyOffers(
        int page = 1,
        int? propertyId = null,
        OfferStatus? status = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            this.SetWarningMessage("El filtro de estado enviado no es válido.");
            return RedirectToAction(nameof(MyOffers), new { propertyId });
        }

        page = Math.Max(1, page);
        var result = await _clientService.GetMyOffersAsync(
            new GetMyOffersRequest(page, PageSize, propertyId, status),
            cancellationToken
        );
        if (
            result.IsSuccess
            && result.GetValue().TotalPages > 0
            && page > result.GetValue().TotalPages
        )
            return RedirectToAction(
                nameof(MyOffers),
                new { page = result.GetValue().TotalPages, propertyId, status }
            );

        var offers = result.IsSuccess
            ? MapPage(result.GetValue())
            : EmptyPage(page);
        if (result.IsFailure)
            ModelState.AddModelError(string.Empty, result.GetError().Message);

        var viewModel = new MyOffersViewModel
        {
            PageTitle = propertyId.HasValue ? "Ofertas de la propiedad" : "Mis ofertas",
            Offers = offers,
            PropertyId = propertyId,
            Status = status,
        };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    [RoleAuthorize(nameof(Roles.Client))]
    public async Task<IActionResult> Create(
        int propertyId,
        CancellationToken cancellationToken = default
    )
    {
        var model = new CreateOfferViewModel { PropertyId = propertyId };
        if (!await PopulatePropertyAsync(model, cancellationToken))
            return RedirectToAction("Index", "Property");

        var pendingResult = await _clientService.GetMyOffersAsync(
            new GetMyOffersRequest(PageSize: 1, PropertyId: propertyId, Status: OfferStatus.Pending),
            cancellationToken
        );
        if (pendingResult.IsSuccess && pendingResult.GetValue().TotalCount > 0)
        {
            this.SetWarningMessage("Ya tienes una oferta pendiente para esta propiedad.");
            return RedirectToAction("Detail", "Property", new { id = propertyId });
        }

        await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(nameof(Roles.Client))]
    public async Task<IActionResult> Create(
        CreateOfferViewModel model,
        CancellationToken cancellationToken = default
    )
    {
        var propertyIsAvailable = await PopulatePropertyAsync(model, cancellationToken);
        if (!propertyIsAvailable)
        {
            this.SetWarningMessage("La propiedad ya no está disponible para recibir ofertas.");
            return RedirectToAction("Index", "Property");
        }

        if (!ModelState.IsValid)
        {
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
            return View(model);
        }

        var result = await _clientService.CreateOfferAsync(
            new CreateOfferRequest(model.PropertyId, model.Amount),
            cancellationToken
        );
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.GetError().Message);
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
            return View(model);
        }

        this.SetSuccessMessage("La oferta fue enviada correctamente.");
        return RedirectToAction("Detail", "Property", new { id = model.PropertyId });
    }

    [HttpGet]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> PropertyOffers(
        int propertyId,
        int page = 1,
        OfferStatus? status = null,
        string? clientId = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
        {
            this.SetWarningMessage("El filtro de estado enviado no es válido.");
            return RedirectToAction(nameof(PropertyOffers), new { propertyId, clientId });
        }

        page = Math.Max(1, page);
        clientId = string.IsNullOrWhiteSpace(clientId) ? null : clientId.Trim();

        var offers = EmptyPage(page);
        var clients = new PagedResult<OfferClientSummaryViewModel>([], 0, page, PageSize);
        Error? queryError = null;
        if (clientId is null)
        {
            var clientsResult = await _agentService.GetPropertyOfferClientsAsync(
                new GetPropertyOfferClientsRequest(propertyId, page, PageSize),
                cancellationToken
            );
            if (clientsResult.IsSuccess)
            {
                if (
                    clientsResult.GetValue().TotalPages > 0
                    && page > clientsResult.GetValue().TotalPages
                )
                    return RedirectToAction(
                        nameof(PropertyOffers),
                        new { propertyId, page = clientsResult.GetValue().TotalPages }
                    );

                clients = new PagedResult<OfferClientSummaryViewModel>(
                    _mapper.Map<IReadOnlyList<OfferClientSummaryViewModel>>(
                        clientsResult.GetValue().Items
                    ),
                    clientsResult.GetValue().TotalCount,
                    clientsResult.GetValue().Page,
                    clientsResult.GetValue().PageSize
                );
            }
            else
                queryError = clientsResult.GetError();
        }
        else
        {
            var offersResult = await _agentService.GetPropertyOffersAsync(
                new GetPropertyOffersRequest(propertyId, page, PageSize, status, clientId),
                cancellationToken
            );
            if (offersResult.IsSuccess)
            {
                if (
                    offersResult.GetValue().TotalPages > 0
                    && page > offersResult.GetValue().TotalPages
                )
                    return RedirectToAction(
                        nameof(PropertyOffers),
                        new
                        {
                            propertyId,
                            page = offersResult.GetValue().TotalPages,
                            status,
                            clientId,
                        }
                    );

                offers = MapPage(offersResult.GetValue());
            }
            else
                queryError = offersResult.GetError();
        }

        if (queryError is not null)
        {
            if (queryError.Type == ErrorType.Forbidden)
                return RedirectToAction("AccessDenied", "Auth");

            this.SetWarningMessage(queryError.Message);
            return RedirectToAction("MyProperties", "Property");
        }

        var propertyResult = await _getPropertyDetail.ExecuteAsync(
            new GetPropertyDetailRequest(propertyId),
            cancellationToken
        );
        if (propertyResult.IsFailure)
        {
            this.SetWarningMessage(propertyResult.GetError().Message);
            return RedirectToAction("MyProperties", "Property");
        }

        var property = propertyResult.GetValue();
        var viewModel = new PropertyOffersViewModel
        {
            PageTitle = $"Ofertas de {property.Code}",
            PropertyId = property.Id,
            PropertyCode = property.Code,
            PropertyDescription = property.Description,
            PropertyTypeName = property.PropertyTypeName,
            PropertyMainImageUrl = property.MainImageUrl,
            PropertyPrice = property.Price,
            PropertyCurrency = property.Currency,
            PropertyStatus = property.Status,
            ClientId = clientId,
            ClientName = offers.Items.FirstOrDefault()?.ClientName,
            Status = status,
            Offers = offers,
            Clients = clients,
        };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> Accept(
        int id,
        int propertyId,
        string? returnUrl,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _agentService.AcceptOfferAsync(id, cancellationToken);
        if (result.IsFailure)
            this.SetWarningMessage(result.GetError().Message);
        else
            this.SetSuccessMessage(
                "La oferta fue aceptada, la propiedad fue marcada como vendida y las demás ofertas pendientes fueron rechazadas."
            );

        return RedirectAfterMutation(propertyId, returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> Reject(
        int id,
        int propertyId,
        string? returnUrl,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _agentService.RejectOfferAsync(id, cancellationToken);
        if (result.IsFailure)
            this.SetWarningMessage(result.GetError().Message);
        else
            this.SetSuccessMessage("La oferta fue rechazada correctamente.");

        return RedirectAfterMutation(propertyId, returnUrl);
    }

    private async Task<bool> PopulatePropertyAsync(
        CreateOfferViewModel model,
        CancellationToken cancellationToken
    )
    {
        var result = await _getPropertyDetail.ExecuteAsync(
            new GetPropertyDetailRequest(model.PropertyId),
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
            return false;

        var property = result.GetValue();
        model.PropertyCode = property.Code;
        model.PropertyDescription = property.Description;
        model.PropertyTypeName = property.PropertyTypeName;
        model.SaleTypeName = property.SaleTypeName;
        model.PropertyMainImageUrl = property.MainImageUrl;
        model.PropertyPrice = property.Price;
        model.PropertyCurrency = property.Currency;
        model.PageTitle = $"Ofertar por {property.Code}";
        return true;
    }

    private IActionResult RedirectAfterMutation(int propertyId, string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(PropertyOffers), new { propertyId });

    private PagedResult<OfferListItemViewModel> MapPage(PagedResult<OfferResponse> page) =>
        new(
            _mapper.Map<IReadOnlyList<OfferListItemViewModel>>(page.Items),
            page.TotalCount,
            page.Page,
            page.PageSize
        );

    private static PagedResult<OfferListItemViewModel> EmptyPage(int page) =>
        new([], 0, page, PageSize);
}
