using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Api.Controllers.Base;
using RealEstateApp.Api.Dtos.Api.Responses;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Api.Controllers.v1;

[ApiVersion("1.0")]
[Authorize(Policy = Policies.ApiAuthorizationPolicies.ApiAccess)]
public class AgentsController : BaseApiController
{
    private readonly IAdminService _adminService;
    private readonly IPropertyService _propertyService;

    public AgentsController(IAdminService adminService, IPropertyService propertyService)
    {
        _adminService = adminService;
        _propertyService = propertyService;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] GetAgentsListRequest request)
    {
        var result = await _adminService.GetAgentsAsync(request);

        if (result.IsSuccess)
        {
            var data = result.GetValue();
            if (data.TotalCount == 0)
                return NoContent();
            return Success(MapToResponse(data.Items));
        }

        return MapServiceError(result.GetError());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _adminService.GetAgentsAsync(new GetAgentsListRequest(PageSize: 1000));

        if (result.IsFailure)
            return MapServiceError(result.GetError());

        var agent = result.GetValue().Items.FirstOrDefault(a => a.Id == id);
        if (agent is null)
            return NotFound(
                ApiResponse.Failure(ErrorCodes.NotFound, "El agente solicitado no existe.")
            );

        return Success(MapSingleToResponse(agent));
    }

    [HttpGet("{id}/properties")]
    public async Task<IActionResult> GetAgentProperties(
        string id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var agentCheck = await _adminService.GetAgentsAsync(
            new GetAgentsListRequest(PageSize: 1000)
        );
        if (agentCheck.IsFailure)
            return MapServiceError(agentCheck.GetError());

        var agentExists = agentCheck.GetValue().Items.Any(a => a.Id == id);
        if (!agentExists)
            return NotFound(
                ApiResponse.Failure(ErrorCodes.NotFound, "El agente solicitado no existe.")
            );

        var result = await _propertyService.GetListAsync(
            new GetPropertyListRequest(
                Page: page,
                PageSize: pageSize,
                AgentId: id,
                IncludeAllStatuses: true
            )
        );

        if (result.IsSuccess)
        {
            var data = result.GetValue();
            if (data.TotalCount == 0)
                return NoContent();
            return Success(MapToPropertyResponse(data.Items));
        }

        return MapServiceError(result.GetError());
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeStatus(string id, [FromBody] AgentStatusRequest body)
    {
        var result = await _adminService.ToggleAgentStatusAsync(id);

        if (result.IsSuccess)
            return NoContent();

        return MapServiceError(result.GetError());
    }

    private static List<AgentApiResponse> MapToResponse(IEnumerable<AgentListItemResponse> items)
    {
        return items
            .Select(a => new AgentApiResponse
            {
                Id = a.Id,
                Name = a.FirstName,
                LastName = a.LastName,
                PropertiesCount = a.PropertiesCount,
                Email = a.Email,
                Phone = a.Phone,
                Status = a.IsActive,
            })
            .ToList();
    }

    private static AgentApiResponse MapSingleToResponse(AgentListItemResponse a)
    {
        return new AgentApiResponse
        {
            Id = a.Id,
            Name = a.FirstName,
            LastName = a.LastName,
            PropertiesCount = a.PropertiesCount,
            Email = a.Email,
            Phone = null,
            Status = a.IsActive,
        };
    }

    private static List<PropertyApiResponse> MapToPropertyResponse(
        IEnumerable<Application.Dtos.Property.Responses.PropertyListItemResponse> items
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

public sealed record AgentStatusRequest(bool Status);
