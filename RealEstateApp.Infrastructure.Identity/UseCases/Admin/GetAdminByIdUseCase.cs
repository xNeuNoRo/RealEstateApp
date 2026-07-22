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

public sealed class GetAdminByIdUseCase : IGetAdminByIdUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAdminByIdRequest> _validator;
    private readonly ILogger<GetAdminByIdUseCase> _logger;

    public GetAdminByIdUseCase(
        UserManager<AppUser> userManager,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetAdminByIdRequest> validator,
        ILogger<GetAdminByIdUseCase> logger
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AdminListItemResponse>> ExecuteAsync(
        GetAdminByIdRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<AdminListItemResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<AdminListItemResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result<AdminListItemResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden consultar administradores."
                )
            );

        var user = await _userManager.FindByIdAsync(request.AdminId);
        if (user is null)
            return Result<AdminListItemResponse>.Failure(
                Error.NotFound("Admin.NotFound", "El administrador solicitado no existe.")
            );

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(nameof(Roles.Admin)))
            return Result<AdminListItemResponse>.Failure(
                Error.NotFound("Admin.NotFound", "El usuario solicitado no es un administrador.")
            );

        var response = _mapper.Map<AdminListItemResponse>(user);

        _logger.LogInformation(
            "Admin {AdminId} consultó administrador {TargetId}.",
            _currentUser.UserId,
            request.AdminId
        );

        return Result<AdminListItemResponse>.Success(response);
    }
}
