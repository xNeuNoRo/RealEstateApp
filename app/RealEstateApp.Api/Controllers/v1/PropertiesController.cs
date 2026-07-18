using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Api.Controllers.Base;
using RealEstateApp.Api.Dtos.Api.Responses;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Api.Controllers.v1;

[ApiVersion("1.0")]
[Authorize(Policy = Policies.ApiAuthorizationPolicies.ApiAccess)]
public class PropertiesController : BaseApiController
{
    private readonly IPropertyService _propertyService;

    public PropertiesController(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] GetPropertyListRequest request)
    {
        request = request with { IncludeAllStatuses = true };
        var result = await _propertyService.GetListAsync(request);

        if (result.IsSuccess)
        {
            var data = result.GetValue();
            if (data.TotalCount == 0)
                return NoContent();
            return Success(MapToListResponse(data.Items));
        }

        return MapServiceError(result.GetError());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _propertyService.GetDetailAsync(id);

        if (result.IsSuccess)
            return Success(MapToDetailResponse(result.GetValue()));

        return MapServiceError(result.GetError());
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _propertyService.SearchByCodeAsync(code);

        if (result.IsSuccess)
            return Success(MapToDetailResponse(result.GetValue()));

        return MapServiceError(result.GetError());
    }

    private static List<PropertyApiResponse> MapToListResponse(
        IEnumerable<PropertyListItemResponse> items
    )
    {
        return items
            .Select(i => new PropertyApiResponse
            {
                Id = i.Id,
                Code = i.Code,
                PropertyType = i.PropertyTypeName ?? "",
                SaleType = i.SaleTypeName ?? "",
                Price = i.Price,
                LandSize = i.SizeM2,
                Bedrooms = i.Bedrooms,
                Bathrooms = i.Bathrooms,
                AgentName = i.AgentName,
                Status = i.Status,
            })
            .ToList();
    }

    private static PropertyApiResponse MapToDetailResponse(PropertyDetailResponse d)
    {
        return new PropertyApiResponse
        {
            Id = d.Id,
            Code = d.Code,
            PropertyType = d.PropertyTypeName ?? "",
            SaleType = d.SaleTypeName ?? "",
            Price = d.Price,
            LandSize = d.SizeM2,
            Bedrooms = d.Bedrooms,
            Bathrooms = d.Bathrooms,
            Description = d.Description,
            Improvements =
                d.Improvements?.Select(imp => new ImprovementApiItem
                    {
                        Id = imp.Id,
                        Name = imp.Name,
                        Description = imp.Description,
                    })
                    .ToList()
                ?? [],
            AgentName = d.AgentName,
            AgentId = d.AgentId,
            Status = d.Status,
        };
    }

    private IActionResult MapServiceError(Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound => NotFound(ApiResponse.Failure(error.Code, error.Message)),
            ErrorType.Validation => BadRequest(ApiResponse.Failure(error.Code, error.Message)),
            _ => FailureResponse(error.Code, error.Message, 500),
        };
    }
}
