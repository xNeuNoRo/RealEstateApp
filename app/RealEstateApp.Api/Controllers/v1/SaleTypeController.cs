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
public class SaleTypeController : BaseApiController
{
    private readonly IAdminService _adminService;

    public SaleTypeController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllSaleTypesRequest request)
    {
        var result = await _adminService.GetSaleTypesAsync(request);

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
        var result = await _adminService.GetSaleTypeByIdAsync(id);

        if (result.IsSuccess)
            return Success(MapSingleToResponse(result.GetValue()));

        return FromResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSaleTypeRequest request)
    {
        var result = await _adminService.CreateSaleTypeAsync(request);

        if (result.IsSuccess)
        {
            var created = result.GetValue();
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<SaleTypeApiResponse>.Success(MapSingleToResponse(created))
            );
        }

        return FromResult(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSaleTypeRequest request)
    {
        var modified = request with { Id = id };
        var result = await _adminService.UpdateSaleTypeAsync(modified);
        return FromResult(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _adminService.DeleteSaleTypeAsync(id);

        if (result.IsSuccess)
            return NoContent();

        return FromResult(result);
    }

    private static List<SaleTypeApiResponse> MapToResponse(
        IEnumerable<Application.Dtos.Catalog.Responses.SaleTypeResponse> items
    )
    {
        return items
            .Select(i => new SaleTypeApiResponse
            {
                Id = i.Id,
                Code = i.Code.ToString(),
                Name = i.Name,
                Description = i.Description,
            })
            .ToList();
    }

    private static SaleTypeApiResponse MapSingleToResponse(
        Application.Dtos.Catalog.Responses.SaleTypeResponse item
    )
    {
        return new SaleTypeApiResponse
        {
            Id = item.Id,
            Code = item.Code.ToString(),
            Name = item.Name,
            Description = item.Description,
        };
    }
}
