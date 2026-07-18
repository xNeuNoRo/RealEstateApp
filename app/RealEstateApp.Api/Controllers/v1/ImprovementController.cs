using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Api.Controllers.Base;
using RealEstateApp.Api.Dtos.Api.Responses;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Api.Controllers.v1;

[ApiVersion("1.0")]
[Authorize(Policy = Policies.ApiAuthorizationPolicies.ApiAccess)]
public class ImprovementController : BaseApiController
{
    private readonly IAdminService _adminService;

    public ImprovementController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllImprovementsRequest request)
    {
        var result = await _adminService.GetImprovementsAsync(request);

        if (result.IsSuccess)
        {
            var data = result.GetValue();
            if (data.TotalCount == 0)
                return NoContent();
            return Success(MapToResponse(data.Items));
        }

        return FromResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _adminService.GetImprovementByIdAsync(id);

        if (result.IsSuccess)
            return Success(MapSingleToResponse(result.GetValue()));

        return FromResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateImprovementRequest request)
    {
        var result = await _adminService.CreateImprovementAsync(request);

        if (result.IsSuccess)
        {
            var created = result.GetValue();
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<CatalogApiResponse>.Success(MapSingleToResponse(created))
            );
        }

        return FromResult(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateImprovementRequest request)
    {
        var modified = request with { Id = id };
        var result = await _adminService.UpdateImprovementAsync(modified);
        return FromResult(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _adminService.DeleteImprovementAsync(id);

        if (result.IsSuccess)
            return NoContent();

        return FromResult(result);
    }

    private static List<CatalogApiResponse> MapToResponse(
        IEnumerable<Application.Dtos.Catalog.Responses.ImprovementResponse> items
    )
    {
        return items
            .Select(i => new CatalogApiResponse
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                PropertiesCount = 0,
            })
            .ToList();
    }

    private static CatalogApiResponse MapSingleToResponse(
        Application.Dtos.Catalog.Responses.ImprovementResponse item
    )
    {
        return new CatalogApiResponse
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            PropertiesCount = 0,
        };
    }
}
