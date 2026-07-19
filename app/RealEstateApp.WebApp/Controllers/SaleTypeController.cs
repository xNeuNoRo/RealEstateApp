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
public sealed class SaleTypeController : BaseController
{
    private const int PageSize = 20;

    private readonly IAdminService _adminService;
    private readonly IViewModelBuilder<BaseViewModel> _viewModelBuilder;
    private readonly IMapper _mapper;

    public SaleTypeController(
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
        var request = new GetAllSaleTypesRequest(searchTerm?.Trim(), page, PageSize);
        var result = await _adminService.GetSaleTypesAsync(request, ct);

        if (result.IsSuccess && result.GetValue().TotalPages > 0 && page > result.GetValue().TotalPages)
            return RedirectToAction(nameof(Index), new { searchTerm, page = result.GetValue().TotalPages });

        var items = result.IsSuccess ? MapPage(result.GetValue()) : EmptyPage(page);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);

        var viewModel = new SaleTypeListViewModel
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
        var viewModel = new CreateSaleTypeViewModel();
        await this.PopulateBaseViewModelAsync(viewModel, _viewModelBuilder, ct);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateSaleTypeViewModel model,
        CancellationToken ct = default
    )
    {
        if (!ModelState.IsValid)
        {
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, ct);
            return View(model);
        }

        var dto = _mapper.Map<CreateSaleTypeRequest>(model);
        var result = await _adminService.CreateSaleTypeAsync(dto, ct);

        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, ct);
            return View(model);
        }

        this.SetSuccessMessage($"Tipo de venta \"{result.GetValue().Name}\" creado correctamente.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var result = await _adminService.GetSaleTypeByIdAsync(id, ct);

        if (result.IsFailure)
        {
            this.SetWarningMessage(result.GetError().Message);
            return RedirectToAction(nameof(Index));
        }

        var dto = result.GetValue();
        var viewModel = new EditSaleTypeViewModel
        {
            Id = dto.Id,
            Code = Enum.TryParse<SaleTypeCode>(dto.Code, out var code) ? code : SaleTypeCode.Sale,
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
        EditSaleTypeViewModel model,
        CancellationToken ct = default
    )
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, ct);
            return View(model);
        }

        var dto = _mapper.Map<UpdateSaleTypeRequest>(model);
        var result = await _adminService.UpdateSaleTypeAsync(dto, ct);

        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            await this.PopulateBaseViewModelAsync(model, _viewModelBuilder, ct);
            return View(model);
        }

        this.SetSuccessMessage($"Tipo de venta \"{model.Name}\" actualizado correctamente.");
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
        var result = await _adminService.DeleteSaleTypeAsync(id, ct);

        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);
        else
            this.SetSuccessMessage("Tipo de venta eliminado correctamente.");

        return RedirectToAction(nameof(Index), new { searchTerm, page });
    }

    private static PagedResult<SaleTypeListItemViewModel> MapPage(
        PagedResult<SaleTypeResponse> page
    ) =>
        new(
            page.Items
                .Select(i => new SaleTypeListItemViewModel
                {
                    Id = i.Id,
                    Code = i.Code,
                    Name = i.Name,
                    Description = i.Description,
                })
                .ToList()
                .AsReadOnly(),
            page.TotalCount,
            page.Page,
            page.PageSize
        );

    private static PagedResult<SaleTypeListItemViewModel> EmptyPage(int page) =>
        new([], 0, page, PageSize);
}
