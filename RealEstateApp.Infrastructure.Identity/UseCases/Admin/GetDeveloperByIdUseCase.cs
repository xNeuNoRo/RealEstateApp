using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Admin;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Admin;

public sealed class GetDeveloperByIdUseCase : IGetDeveloperByIdUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetDeveloperByIdRequest> _validator;
    private readonly ILogger<GetDeveloperByIdUseCase> _logger;

    public GetDeveloperByIdUseCase(
        UserManager<AppUser> userManager,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetDeveloperByIdRequest> validator,
        ILogger<GetDeveloperByIdUseCase> logger
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<DeveloperListItemResponse>> ExecuteAsync(
        GetDeveloperByIdRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<DeveloperListItemResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<DeveloperListItemResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result<DeveloperListItemResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden consultar desarrolladores."
                )
            );

        var user = await _userManager.FindByIdAsync(request.DeveloperId);
        if (user is null)
            return Result<DeveloperListItemResponse>.Failure(
                Error.NotFound("Developer.NotFound", "El desarrollador solicitado no existe.")
            );

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(nameof(Roles.Developer)))
            return Result<DeveloperListItemResponse>.Failure(
                Error.NotFound("Developer.NotFound", "El usuario solicitado no es un desarrollador.")
            );

        var response = _mapper.Map<DeveloperListItemResponse>(user);

        _logger.LogInformation(
            "Admin {AdminId} consultó desarrollador {TargetId}.",
            _currentUser.UserId,
            request.DeveloperId
        );

        return Result<DeveloperListItemResponse>.Success(response);
    }
}
