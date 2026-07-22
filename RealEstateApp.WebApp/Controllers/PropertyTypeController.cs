using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.ViewModels.Admin;
using RealEstateApp.Application.ViewModels.Shared;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.WebApp.Extensions;
using RealEstateApp.WebApp.Filters;

namespace RealEstateApp.WebApp.Controllers;

[SessionAuthorize]
[RoleAuthorize(nameof(Roles.Admin))]
public sealed class PropertyTypeController : BaseController
{
    private const int PageSize = 20;

    private readonly IAdminService _adminService;
    private readonly IViewModelBuilder<BaseViewModel> _viewModelBuilder;
    private readonly IMapper _mapper;

    public PropertyTypeController(
        ICurrentUserService currentUser,
        IAdminService adminService,
        IViewModelBuilder<BaseViewModel> viewModelBuilder,
        IMapper mapper
    )
        : base(currentUser)
    {
        _adminService = adminService;
        _viewModelBuilder = viewModelBuilder;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm = null,
        int page = 1,
        CancellationToken ct = default
    )
    {
        page = Math.Max(1, page);
        var request = new GetAllPropertyTypesRequest(searchTerm?.Trim(), page, PageSize);
        var result = await _adminService.GetPropertyTypesAsync(request, ct);

        if (
            result.IsSuccess
            && result.GetValue().TotalPages > 0
            && page > result.GetValue().TotalPages
        )
            return RedirectToAction(
                nameof(Index),
                new { searchTerm, page = result.GetValue().TotalPages }
            );

        var items = result.IsSuccess ? MapPage(result.GetValue()) : EmptyPage(page);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);

        var viewModel = new PropertyTypeListViewModel
        {
            Items = items,
            SearchTerm = searchTerm?.Trim(),
        };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, ct);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct = default)
    {
        var viewModel = new CreatePropertyTypeViewModel();
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, ct);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreatePropertyTypeViewModel model,
        CancellationToken ct = default
    )
    {
        if (!ModelState.IsValid)
        {
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, ct);
            return View(model);
        }

        var dto = _mapper.Map<CreatePropertyTypeRequest>(model);
        var result = await _adminService.CreatePropertyTypeAsync(dto, ct);

        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, ct);
            return View(model);
        }

        this.SetSuccessMessage(
            $"Tipo de propiedad \"{result.GetValue().Name}\" creado correctamente."
        );
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var result = await _adminService.GetPropertyTypeByIdAsync(id, ct);

        if (result.IsFailure)
        {
            this.SetWarningMessage(result.GetError().Message);
            return RedirectToAction(nameof(Index));
        }

        var dto = result.GetValue();
        var viewModel = new EditPropertyTypeViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
        };
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, ct);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        EditPropertyTypeViewModel model,
        CancellationToken ct = default
    )
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, ct);
            return View(model);
        }

        var dto = _mapper.Map<UpdatePropertyTypeRequest>(model);
        var result = await _adminService.UpdatePropertyTypeAsync(dto, ct);

        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, ct);
            return View(model);
        }

        this.SetSuccessMessage($"Tipo de propiedad \"{model.Name}\" actualizado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        string? searchTerm = null,
        int page = 1,
        CancellationToken ct = default
    )
    {
        var result = await _adminService.DeletePropertyTypeAsync(id, ct);

        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);
        else
            this.SetSuccessMessage("Tipo de propiedad eliminado correctamente.");

        return RedirectToAction(nameof(Index), new { searchTerm, page });
    }

    private static PagedResult<PropertyTypeListItemViewModel> MapPage(
        PagedResult<PropertyTypeResponse> page
    ) =>
        new(
            page.Items.Select(i => new PropertyTypeListItemViewModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    PropertiesCount = i.PropertiesCount,
                })
                .ToList()
                .AsReadOnly(),
            page.TotalCount,
            page.Page,
            page.PageSize
        );

    private static PagedResult<PropertyTypeListItemViewModel> EmptyPage(int page) =>
        new([], 0, page, PageSize);
}
