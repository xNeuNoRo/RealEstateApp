using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Adapters;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Favorites.Requests;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Application.Interfaces.UseCases.Favorites;
using RealEstateApp.Application.Interfaces.UseCases.Offers;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Application.ViewModels.Agent;
using RealEstateApp.Application.ViewModels.Home;
using RealEstateApp.Application.ViewModels.Property;
using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Settings;
using RealEstateApp.WebApp.Extensions;
using RealEstateApp.WebApp.Filters;

namespace RealEstateApp.WebApp.Controllers;

public sealed class PropertyController : BaseController
{
    private const int PageSize = 12;
    private const int CatalogPageSize = 100;
    private const long PropertyUploadLimit = (4 * FileConstants.MaxImageFileSizeBytes) + (1024 * 1024);

    private readonly IGetPropertyListUseCase _getPropertyList;
    private readonly IGetPropertyDetailUseCase _getPropertyDetail;
    private readonly IGetAgentPropertiesUseCase _getAgentProperties;
    private readonly ICreatePropertyUseCase _createProperty;
    private readonly IUpdatePropertyUseCase _updateProperty;
    private readonly IDeletePropertyUseCase _deleteProperty;
    private readonly IGetPublicPropertyTypesUseCase _getPublicPropertyTypes;
    private readonly IGetAllPropertyTypesUseCase _getPropertyTypes;
    private readonly IGetAllSaleTypesUseCase _getSaleTypes;
    private readonly IGetAllImprovementsUseCase _getImprovements;
    private readonly IAddFavoriteUseCase _addFavorite;
    private readonly IRemoveFavoriteUseCase _removeFavorite;
    private readonly IGetMyOffersUseCase _getMyOffers;
    private readonly IViewModelBuilder<BaseViewModel> _viewModelBuilder;
    private readonly IMapper _mapper;

    public PropertyController(
        ICurrentUserService currentUser,
        IGetPropertyListUseCase getPropertyList,
        IGetPropertyDetailUseCase getPropertyDetail,
        IGetAgentPropertiesUseCase getAgentProperties,
        ICreatePropertyUseCase createProperty,
        IUpdatePropertyUseCase updateProperty,
        IDeletePropertyUseCase deleteProperty,
        IGetPublicPropertyTypesUseCase getPublicPropertyTypes,
        IGetAllPropertyTypesUseCase getPropertyTypes,
        IGetAllSaleTypesUseCase getSaleTypes,
        IGetAllImprovementsUseCase getImprovements,
        IAddFavoriteUseCase addFavorite,
        IRemoveFavoriteUseCase removeFavorite,
        IGetMyOffersUseCase getMyOffers,
        IViewModelBuilder<BaseViewModel> viewModelBuilder,
        IMapper mapper
    )
        : base(currentUser)
    {
        _getPropertyList = getPropertyList;
        _getPropertyDetail = getPropertyDetail;
        _getAgentProperties = getAgentProperties;
        _createProperty = createProperty;
        _updateProperty = updateProperty;
        _deleteProperty = deleteProperty;
        _getPublicPropertyTypes = getPublicPropertyTypes;
        _getPropertyTypes = getPropertyTypes;
        _getSaleTypes = getSaleTypes;
        _getImprovements = getImprovements;
        _addFavorite = addFavorite;
        _removeFavorite = removeFavorite;
        _getMyOffers = getMyOffers;
        _viewModelBuilder = viewModelBuilder;
        _mapper = mapper;
    }

    [HttpGet]
    [SessionAuthorize]
    [RoleAuthorize(nameof(Roles.Client))]
    public async Task<IActionResult> Index(
        [FromQuery] PropertyFilterViewModel filters,
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        page = Math.Max(1, page);
        filters.Code = filters.Code?.Trim();
        filters.PropertyTypes = await GetPublicPropertyTypesAsync(cancellationToken);

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
                properties = MapPage<PropertyListItemResponse, PropertyListItemViewModel>(result.GetValue());
            else
                ModelState.AddModelError(string.Empty, result.GetError().Message);
        }

        var viewModel = new HomeIndexViewModel { Filters = filters, Properties = properties };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View("~/Views/Home/Index.cshtml", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
    {
        var result = await _getPropertyDetail.ExecuteAsync(
            new GetPropertyDetailRequest(id),
            cancellationToken
        );

        if (result.IsFailure)
        {
            this.SetWarningMessage("La propiedad solicitada no existe o no se encuentra disponible.");
            return RedirectToCatalog();
        }

        var property = result.GetValue();
        var isOwner = CurrentUser.IsAuthenticated
            && CurrentUser.IsInRole(nameof(Roles.Agent))
            && string.Equals(CurrentUser.UserId, property.AgentId, StringComparison.Ordinal);
        var isAvailable = string.Equals(
            property.Status,
            nameof(PropertyStatus.Available),
            StringComparison.OrdinalIgnoreCase
        );
        if (!isAvailable && !isOwner)
        {
            this.SetWarningMessage("La propiedad solicitada no existe o no se encuentra disponible.");
            return RedirectToCatalog();
        }

        var viewModel = _mapper.Map<PropertyDetailPublicViewModel>(property);
        if (CurrentUser.IsInRole(nameof(Roles.Client)))
        {
            var offersResult = await _getMyOffers.ExecuteAsync(
                new GetMyOffersRequest(PageSize: 5, PropertyId: id),
                cancellationToken
            );
            if (offersResult.IsSuccess)
                viewModel.Offers = _mapper.Map<IReadOnlyList<Application.ViewModels.Offers.OfferListItemViewModel>>(
                    offersResult.GetValue().Items
                );
        }
        viewModel.PageTitle = $"{viewModel.PropertyTypeName} {viewModel.Code}";
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View("~/Views/Home/Detail.cshtml", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [SessionAuthorize]
    [RoleAuthorize(nameof(Roles.Client))]
    public async Task<IActionResult> ToggleFavorite(
        int propertyId,
        bool add,
        string? returnUrl,
        CancellationToken cancellationToken = default
    )
    {
        Error? error = null;
        if (add)
        {
            var result = await _addFavorite.ExecuteAsync(
                new AddFavoriteRequest(propertyId),
                cancellationToken
            );
            if (result.IsFailure)
                error = result.GetError();
        }
        else
        {
            var result = await _removeFavorite.ExecuteAsync(
                new RemoveFavoriteRequest(propertyId),
                cancellationToken
            );
            if (result.IsFailure)
                error = result.GetError();
        }

        if (error is not null)
            this.SetWarningMessage(error.Message);
        else
            this.SetSuccessMessage(
                add
                    ? "La propiedad fue agregada a sus favoritas correctamente."
                    : "La propiedad fue eliminada de sus favoritas correctamente."
            );

        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [SessionAuthorize]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> MyProperties(
        int page = 1,
        CancellationToken cancellationToken = default
    )
    {
        page = Math.Max(1, page);
        var result = await _getAgentProperties.ExecuteAsync(
            new GetAgentPropertiesRequest(page, PageSize, PropertyStatus.Available),
            cancellationToken
        );

        var properties = result.IsSuccess
            ? MapPage<PropertySummaryResponse, AgentPropertyListItemViewModel>(result.GetValue())
            : EmptyPage<AgentPropertyListItemViewModel>(page);

        if (result.IsFailure)
            ModelState.AddModelError(string.Empty, result.GetError().Message);

        var viewModel = new PropertyMaintenanceViewModel
        {
            PageTitle = "Mis propiedades",
            Properties = properties,
        };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpGet]
    [SessionAuthorize]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new CreatePropertyViewModel { PageTitle = "Publicar propiedad" };
        await LoadFormOptionsAsync(viewModel, cancellationToken);
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(PropertyUploadLimit)]
    [SessionAuthorize]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> Create(
        CreatePropertyViewModel model,
        List<IFormFile> imageFiles,
        CancellationToken cancellationToken = default
    )
    {
        await LoadFormOptionsAsync(model, cancellationToken);
        ValidateCatalogSelections(model);

        if (imageFiles.Count == 0)
            ModelState.AddModelError(nameof(imageFiles), "Debe cargar al menos una imagen de la propiedad.");
        else if (imageFiles.Count > 4)
            ModelState.AddModelError(nameof(imageFiles), "Solo se permite registrar hasta 4 imágenes por propiedad.");

        if (!ModelState.IsValid)
        {
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
            return View(model);
        }

        var result = await _createProperty.ExecuteAsync(
            new CreatePropertyRequest(
                model.Description,
                model.Price,
                "DOP",
                model.SizeM2,
                model.Bedrooms,
                model.Bathrooms,
                model.PropertyTypeId,
                model.SaleTypeId,
                imageFiles.Select(file => file.ToAppFile()!).ToArray(),
                model.ImprovementIds
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            AddPropertyError(result.GetError());
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
            return View(model);
        }

        this.SetSuccessMessage($"La propiedad fue creada correctamente. Código {result.GetValue().Code}.");
        return RedirectToAction(nameof(MyProperties));
    }

    [HttpGet]
    [SessionAuthorize]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var detailResult = await GetOwnedAvailablePropertyAsync(id, "modificar", cancellationToken);
        if (detailResult.IsFailure)
            return PropertyAccessFailureResult(detailResult.GetError());
        var detail = detailResult.GetValue();

        var viewModel = new EditPropertyViewModel
        {
            Id = detail.Id,
            Code = detail.Code,
            Description = detail.Description,
            Price = detail.Price,
            Currency = "DOP",
            SizeM2 = detail.SizeM2,
            Bedrooms = detail.Bedrooms,
            Bathrooms = detail.Bathrooms,
            PropertyTypeId = detail.PropertyTypeId,
            SaleTypeId = detail.SaleTypeId,
            ImprovementIds = detail.Improvements.Select(item => item.Id).ToList(),
            ExistingImages = detail.Images,
            PageTitle = $"Editar {detail.Code}",
        };
        await LoadFormOptionsAsync(viewModel, cancellationToken);
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(PropertyUploadLimit)]
    [SessionAuthorize]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> Edit(
        EditPropertyViewModel model,
        List<IFormFile> newImageFiles,
        CancellationToken cancellationToken = default
    )
    {
        var detailResult = await GetOwnedAvailablePropertyAsync(
            model.Id,
            "modificar",
            cancellationToken
        );
        if (detailResult.IsFailure)
            return PropertyAccessFailureResult(detailResult.GetError());
        var detail = detailResult.GetValue();

        model.Code = detail.Code;
        model.ExistingImages = detail.Images;
        await LoadFormOptionsAsync(model, cancellationToken);
        ValidateCatalogSelections(model);

        var imageIds = detail.Images.Select(image => image.Id).ToHashSet();
        model.ImageIdsToRemove = model.ImageIdsToRemove.Distinct().ToList();
        if (model.ImageIdsToRemove.Any(id => !imageIds.Contains(id)))
            ModelState.AddModelError(nameof(model.ImageIdsToRemove), "Una imagen seleccionada no pertenece a la propiedad.");

        var finalImageCount = detail.Images.Count - model.ImageIdsToRemove.Count + newImageFiles.Count;
        if (finalImageCount < 1)
            ModelState.AddModelError(nameof(newImageFiles), "La propiedad debe mantener al menos una imagen después de la edición.");
        else if (finalImageCount > 4)
            ModelState.AddModelError(nameof(newImageFiles), "La propiedad no debe tener más de 4 imágenes en total.");

        if (!ModelState.IsValid)
        {
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
            return View(model);
        }

        var currentImprovements = detail.Improvements.Select(item => item.Id).ToHashSet();
        var selectedImprovements = model.ImprovementIds.ToHashSet();
        var result = await _updateProperty.ExecuteAsync(
            new UpdatePropertyRequest(
                model.Id,
                model.Description,
                model.Price,
                "DOP",
                model.SizeM2,
                model.Bedrooms,
                model.Bathrooms,
                model.PropertyTypeId,
                model.SaleTypeId,
                newImageFiles.Select(file => file.ToAppFile()!).ToArray(),
                model.ImageIdsToRemove,
                selectedImprovements.Except(currentImprovements).ToArray(),
                currentImprovements.Except(selectedImprovements).ToArray()
            ),
            cancellationToken
        );

        if (result.IsFailure)
        {
            AddPropertyError(result.GetError());
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, cancellationToken);
            return View(model);
        }

        this.SetSuccessMessage("La propiedad fue actualizada correctamente.");
        return RedirectToAction(nameof(MyProperties));
    }

    [HttpGet]
    [SessionAuthorize]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var detailResult = await GetOwnedAvailablePropertyAsync(id, "eliminar", cancellationToken);
        if (detailResult.IsFailure)
            return PropertyAccessFailureResult(detailResult.GetError());
        var detail = detailResult.GetValue();

        var viewModel = new DeletePropertyViewModel
        {
            Id = detail.Id,
            Code = detail.Code,
            Description = detail.Description,
            MainImageUrl = detail.MainImageUrl,
            PageTitle = $"Eliminar {detail.Code}",
        };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, cancellationToken);
        return View(viewModel);
    }

    [HttpPost, ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    [SessionAuthorize]
    [RoleAuthorize(nameof(Roles.Agent))]
    public async Task<IActionResult> DeleteConfirmed(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _deleteProperty.ExecuteAsync(
            new DeletePropertyRequest(id),
            cancellationToken
        );

        if (result.IsFailure)
        {
            if (result.GetError().Code == "Property.NotOwner")
                return RedirectToAction("AccessDenied", "Auth");

            this.SetWarningMessage(result.GetError().Message);
            return RedirectToAction(nameof(MyProperties));
        }

        this.SetSuccessMessage("La propiedad fue eliminada correctamente.");
        return RedirectToAction(nameof(MyProperties));
    }

    private async Task<IReadOnlyList<SelectListItemViewModel>> GetPublicPropertyTypesAsync(
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

    private async Task LoadFormOptionsAsync(
        CreatePropertyViewModel model,
        CancellationToken cancellationToken
    )
    {
        var propertyTypes = await LoadAllCatalogItemsAsync(page =>
            _getPropertyTypes.ExecuteAsync(
                new GetAllPropertyTypesRequest(Page: page, PageSize: CatalogPageSize),
                cancellationToken
            )
        );
        var saleTypes = await LoadAllCatalogItemsAsync(page =>
            _getSaleTypes.ExecuteAsync(
                new GetAllSaleTypesRequest(Page: page, PageSize: CatalogPageSize),
                cancellationToken
            )
        );
        var improvements = await LoadAllCatalogItemsAsync(page =>
            _getImprovements.ExecuteAsync(
                new GetAllImprovementsRequest(Page: page, PageSize: CatalogPageSize),
                cancellationToken
            )
        );

        model.PropertyTypes = _mapper.Map<IReadOnlyList<SelectListItemViewModel>>(propertyTypes);
        model.SaleTypes = _mapper.Map<IReadOnlyList<SelectListItemViewModel>>(saleTypes);
        model.Improvements = _mapper.Map<IReadOnlyList<SelectListItemViewModel>>(improvements);

        AddCatalogAvailabilityErrors(model.PropertyTypes, model.SaleTypes, model.Improvements);
    }

    private async Task<IReadOnlyList<T>> LoadAllCatalogItemsAsync<T>(
        Func<int, Task<Result<PagedResult<T>>>> fetchPage
    )
    {
        var items = new List<T>();
        var page = 1;
        var totalPages = 1;

        do
        {
            var result = await fetchPage(page);
            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.GetError().Message);
                return [];
            }

            var current = result.GetValue();
            items.AddRange(current.Items);
            totalPages = current.TotalPages;
            page++;
        } while (page <= totalPages);

        return items;
    }

    private void AddCatalogAvailabilityErrors(
        IReadOnlyList<SelectListItemViewModel> propertyTypes,
        IReadOnlyList<SelectListItemViewModel> saleTypes,
        IReadOnlyList<SelectListItemViewModel> improvements
    )
    {
        if (propertyTypes.Count == 0)
            ModelState.AddModelError(string.Empty, "No existen tipos de propiedades registrados. Debe crear al menos uno antes de registrar una propiedad.");
        if (saleTypes.Count == 0)
            ModelState.AddModelError(string.Empty, "No existen tipos de ventas registrados. Debe crear al menos uno antes de registrar una propiedad.");
        if (improvements.Count == 0)
            ModelState.AddModelError(string.Empty, "No existen mejoras registradas. Debe crear al menos una antes de registrar una propiedad.");
    }

    private void ValidateCatalogSelections(CreatePropertyViewModel model)
    {
        if (!model.PropertyTypes.Any(item => item.Id == model.PropertyTypeId))
            ModelState.AddModelError(nameof(model.PropertyTypeId), "El tipo de propiedad seleccionado no existe.");
        if (!model.SaleTypes.Any(item => item.Id == model.SaleTypeId))
            ModelState.AddModelError(nameof(model.SaleTypeId), "El tipo de venta seleccionado no existe.");
        if (
            model.ImprovementIds.Count > 0
            && model.ImprovementIds.Any(id => !model.Improvements.Any(item => item.Id == id))
        )
            ModelState.AddModelError(nameof(model.ImprovementIds), "Una de las mejoras seleccionadas no existe.");
    }

    private async Task<Result<PropertyDetailResponse>> GetOwnedAvailablePropertyAsync(
        int propertyId,
        string operation,
        CancellationToken cancellationToken
    )
    {
        var result = await _getPropertyDetail.ExecuteAsync(
            new GetPropertyDetailRequest(propertyId),
            cancellationToken
        );
        if (result.IsFailure)
            return Result<PropertyDetailResponse>.Failure(result.GetError());

        var property = result.GetValue();
        if (!string.Equals(property.AgentId, CurrentUser.UserId, StringComparison.Ordinal))
            return Result<PropertyDetailResponse>.Failure(
                Error.Forbidden(
                    "Property.NotOwner",
                    $"No tiene permisos para {operation} esta propiedad."
                )
            );
        if (!string.Equals(property.Status, nameof(PropertyStatus.Available), StringComparison.OrdinalIgnoreCase))
            return Result<PropertyDetailResponse>.Failure(
                Error.Conflict(
                    "Property.NotAvailable",
                    $"No se puede {operation} una propiedad que ya fue vendida."
                )
            );

        return Result<PropertyDetailResponse>.Success(property);
    }

    private IActionResult PropertyAccessFailureResult(Error error)
    {
        if (error.Type == ErrorType.Forbidden)
            return RedirectToAction("AccessDenied", "Auth");

        this.SetWarningMessage(error.Message);
        return RedirectToAction(nameof(MyProperties));
    }

    private IActionResult RedirectToCatalog() =>
        CurrentUser.IsAuthenticated && CurrentUser.IsInRole(nameof(Roles.Client))
            ? RedirectToAction(nameof(Index))
            : RedirectToAction("Index", "Home");

    private void AddPropertyError(Error error)
    {
        var key = error.Code switch
        {
            "Price.Invalid" => "Price",
            "Size.Invalid" => "SizeM2",
            "Property.PropertyType" => "PropertyTypeId",
            "Property.SaleType" => "SaleTypeId",
            "Property.Improvements" or "Property.LastImprovement" => "ImprovementIds",
            "Property.Images" or "Property.InvalidImage" or "Property.NoImages" => string.Empty,
            _ => string.Empty,
        };
        ModelState.AddModelError(key, error.Message);
    }

    private PagedResult<TDestination> MapPage<TSource, TDestination>(PagedResult<TSource> page) =>
        new(
            _mapper.Map<IReadOnlyList<TDestination>>(page.Items),
            page.TotalCount,
            page.Page,
            page.PageSize
        );

    private static PagedResult<T> EmptyPage<T>(int page) => new([], 0, page, PageSize);
}
