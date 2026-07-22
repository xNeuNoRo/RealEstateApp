using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
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
public sealed class UserController : BaseController
{
    private const int PageSize = 20;

    private readonly IAdminService _adminService;
    private readonly IViewModelBuilder<BaseViewModel> _viewModelBuilder;
    private readonly IMapper _mapper;

    public UserController(
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
    public IActionResult Index() => RedirectToAction(nameof(Agents));

    private Task PopulateAsync(BaseViewModel vm, CancellationToken ct) =>
        this.PopulateBaseViewModelAsync(vm, _viewModelBuilder, ct);

    // ============================================================
    // AGENTS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Agents(
        string? searchTerm = null,
        int page = 1,
        CancellationToken ct = default
    )
    {
        page = Math.Max(1, page);
        var request = new GetAgentsListRequest(searchTerm?.Trim(), page, PageSize);
        var result = await _adminService.GetAgentsAsync(request, ct);

        if (result.IsSuccess && result.GetValue().TotalPages > 0 && page > result.GetValue().TotalPages)
            return RedirectToAction(nameof(Agents), new { searchTerm, page = result.GetValue().TotalPages });

        var items = result.IsSuccess ? MapAgentsPage(result.GetValue()) : EmptyAgentsPage(page);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);

        var viewModel = new AgentListViewModel
        {
            Items = items,
            SearchTerm = searchTerm?.Trim(),
        };
        await PopulateAsync(viewModel, ct);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAgentStatus(
        string id,
        string? searchTerm = null,
        int page = 1,
        CancellationToken ct = default
    )
    {
        var result = await _adminService.ToggleAgentStatusAsync(id, ct);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);
        else
            this.SetSuccessMessage("Estado del agente actualizado correctamente.");

        return RedirectToAction(nameof(Agents), new { searchTerm, page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAgent(
        string id,
        string? searchTerm = null,
        int page = 1,
        CancellationToken ct = default
    )
    {
        var result = await _adminService.DeleteAgentAsync(id, ct);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);
        else
            this.SetSuccessMessage("Agente eliminado correctamente.");

        return RedirectToAction(nameof(Agents), new { searchTerm, page });
    }

    // ============================================================
    // ADMINS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Admins(
        string? searchTerm = null,
        int page = 1,
        CancellationToken ct = default
    )
    {
        page = Math.Max(1, page);
        var request = new GetAdminsListRequest(searchTerm?.Trim(), page, PageSize);
        var result = await _adminService.GetAdminsAsync(request, ct);

        if (result.IsSuccess && result.GetValue().TotalPages > 0 && page > result.GetValue().TotalPages)
            return RedirectToAction(nameof(Admins), new { searchTerm, page = result.GetValue().TotalPages });

        var items = result.IsSuccess ? MapAdminsPage(result.GetValue()) : EmptyAdminsPage(page);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);

        var viewModel = new AdminListViewModel
        {
            Items = items,
            SearchTerm = searchTerm?.Trim(),
        };
        await PopulateAsync(viewModel, ct);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CreateAdmin(CancellationToken ct = default)
    {
        var viewModel = new CreateAdminViewModel();
        await PopulateAsync(viewModel, ct);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(
        CreateAdminViewModel model,
        CancellationToken ct = default
    )
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, ct);
            return View(model);
        }

        var dto = _mapper.Map<CreateAdminRequest>(model);
        var result = await _adminService.CreateAdminAsync(dto, ct);

        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            await PopulateAsync(model, ct);
            return View(model);
        }

        this.SetSuccessMessage($"Administrador \"{result.GetValue().FirstName} {result.GetValue().LastName}\" creado correctamente.");
        return RedirectToAction(nameof(Admins));
    }

    [HttpGet]
    public async Task<IActionResult> EditAdmin(string id, CancellationToken ct = default)
    {
        var result = await _adminService.GetAdminByIdAsync(id, ct);

        if (result.IsFailure)
        {
            this.SetWarningMessage(result.GetError().Message);
            return RedirectToAction(nameof(Admins));
        }

        var dto = result.GetValue();
        var viewModel = new EditAdminViewModel
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IdentityDocument = dto.IdentityDocument ?? string.Empty,
            Email = dto.Email ?? string.Empty,
            UserName = dto.UserName ?? string.Empty,
        };
        await PopulateAsync(viewModel, ct);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAdmin(
        string id,
        EditAdminViewModel model,
        CancellationToken ct = default
    )
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, ct);
            return View(model);
        }

        var dto = _mapper.Map<UpdateAdminRequest>(model);
        var result = await _adminService.UpdateAdminAsync(dto, ct);

        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            await PopulateAsync(model, ct);
            return View(model);
        }

        this.SetSuccessMessage($"Administrador \"{model.FirstName} {model.LastName}\" actualizado correctamente.");
        return RedirectToAction(nameof(Admins));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAdminStatus(
        string id,
        string? searchTerm = null,
        int page = 1,
        CancellationToken ct = default
    )
    {
        var result = await _adminService.ToggleAdminStatusAsync(id, ct);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);
        else
            this.SetSuccessMessage("Estado del administrador actualizado correctamente.");

        return RedirectToAction(nameof(Admins), new { searchTerm, page });
    }

    // ============================================================
    // DEVELOPERS
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> Developers(
        string? searchTerm = null,
        int page = 1,
        CancellationToken ct = default
    )
    {
        page = Math.Max(1, page);
        var request = new GetDevelopersListRequest(searchTerm?.Trim(), page, PageSize);
        var result = await _adminService.GetDevelopersAsync(request, ct);

        if (result.IsSuccess && result.GetValue().TotalPages > 0 && page > result.GetValue().TotalPages)
            return RedirectToAction(nameof(Developers), new { searchTerm, page = result.GetValue().TotalPages });

        var items = result.IsSuccess ? MapDevelopersPage(result.GetValue()) : EmptyDevelopersPage(page);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);

        var viewModel = new DeveloperListViewModel
        {
            Items = items,
            SearchTerm = searchTerm?.Trim(),
        };
        await PopulateAsync(viewModel, ct);
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> CreateDeveloper(CancellationToken ct = default)
    {
        var viewModel = new CreateDeveloperViewModel();
        await PopulateAsync(viewModel, ct);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDeveloper(
        CreateDeveloperViewModel model,
        CancellationToken ct = default
    )
    {
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, ct);
            return View(model);
        }

        var dto = _mapper.Map<CreateDeveloperRequest>(model);
        var result = await _adminService.CreateDeveloperAsync(dto, ct);

        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            await PopulateAsync(model, ct);
            return View(model);
        }

        this.SetSuccessMessage($"Desarrollador \"{result.GetValue().FirstName} {result.GetValue().LastName}\" creado correctamente.");
        return RedirectToAction(nameof(Developers));
    }

    [HttpGet]
    public async Task<IActionResult> EditDeveloper(string id, CancellationToken ct = default)
    {
        var result = await _adminService.GetDeveloperByIdAsync(id, ct);

        if (result.IsFailure)
        {
            this.SetWarningMessage(result.GetError().Message);
            return RedirectToAction(nameof(Developers));
        }

        var dto = result.GetValue();
        var viewModel = new EditDeveloperViewModel
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IdentityDocument = dto.IdentityDocument ?? string.Empty,
            Email = dto.Email ?? string.Empty,
            UserName = dto.UserName ?? string.Empty,
        };
        await PopulateAsync(viewModel, ct);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDeveloper(
        string id,
        EditDeveloperViewModel model,
        CancellationToken ct = default
    )
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await PopulateAsync(model, ct);
            return View(model);
        }

        var dto = _mapper.Map<UpdateDeveloperRequest>(model);
        var result = await _adminService.UpdateDeveloperAsync(dto, ct);

        if (result.IsFailure)
        {
            this.SetErrorMessage(result.GetError().Message);
            await PopulateAsync(model, ct);
            return View(model);
        }

        this.SetSuccessMessage($"Desarrollador \"{model.FirstName} {model.LastName}\" actualizado correctamente.");
        return RedirectToAction(nameof(Developers));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDeveloperStatus(
        string id,
        string? searchTerm = null,
        int page = 1,
        CancellationToken ct = default
    )
    {
        var result = await _adminService.ToggleDeveloperStatusAsync(id, ct);
        if (result.IsFailure)
            this.SetErrorMessage(result.GetError().Message);
        else
            this.SetSuccessMessage("Estado del desarrollador actualizado correctamente.");

        return RedirectToAction(nameof(Developers), new { searchTerm, page });
    }

    // ============================================================
    // MAP HELPERS
    // ============================================================

    private static PagedResult<AgentListItemViewModel> MapAgentsPage(
        PagedResult<AgentListItemResponse> page
    ) =>
        new(
            page.Items
                .Select(i => new AgentListItemViewModel
                {
                    Id = i.Id,
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    Email = i.Email,
                    UserName = i.UserName,
                    PropertiesCount = i.PropertiesCount,
                    IsActive = i.IsActive,
                })
                .ToList()
                .AsReadOnly(),
            page.TotalCount,
            page.Page,
            page.PageSize
        );

    private static PagedResult<AgentListItemViewModel> EmptyAgentsPage(int page) =>
        new([], 0, page, PageSize);

    private static PagedResult<AdminListItemViewModel> MapAdminsPage(
        PagedResult<AdminListItemResponse> page
    ) =>
        new(
            page.Items
                .Select(i => new AdminListItemViewModel
                {
                    Id = i.Id,
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    UserName = i.UserName,
                    IdentityDocument = i.IdentityDocument,
                    Email = i.Email,
                    IsActive = i.IsActive,
                })
                .ToList()
                .AsReadOnly(),
            page.TotalCount,
            page.Page,
            page.PageSize
        );

    private static PagedResult<AdminListItemViewModel> EmptyAdminsPage(int page) =>
        new([], 0, page, PageSize);

    private static PagedResult<DeveloperListItemViewModel> MapDevelopersPage(
        PagedResult<DeveloperListItemResponse> page
    ) =>
        new(
            page.Items
                .Select(i => new DeveloperListItemViewModel
                {
                    Id = i.Id,
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    UserName = i.UserName,
                    IdentityDocument = i.IdentityDocument,
                    Email = i.Email,
                    IsActive = i.IsActive,
                })
                .ToList()
                .AsReadOnly(),
            page.TotalCount,
            page.Page,
            page.PageSize
        );

    private static PagedResult<DeveloperListItemViewModel> EmptyDevelopersPage(int page) =>
        new([], 0, page, PageSize);
}
