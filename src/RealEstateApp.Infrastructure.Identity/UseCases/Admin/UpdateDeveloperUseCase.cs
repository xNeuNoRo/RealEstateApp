using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Admin;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.ValueObjects;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Admin;

public sealed class UpdateDeveloperUseCase : IUpdateDeveloperUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateDeveloperRequest> _validator;
    private readonly ILogger<UpdateDeveloperUseCase> _logger;

    public UpdateDeveloperUseCase(
        UserManager<AppUser> userManager,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<UpdateDeveloperRequest> validator,
        ILogger<UpdateDeveloperUseCase> logger
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<DeveloperResponse>> ExecuteAsync(
        UpdateDeveloperRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult<DeveloperResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<DeveloperResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result<DeveloperResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden editar desarrolladores."
                )
            );

        var user = await _userManager.FindByIdAsync(request.DeveloperId);
        if (user is null)
            return Result<DeveloperResponse>.Failure(
                Error.NotFound("User.NotFound", "Desarrollador no encontrado.")
            );

        if (!await _userManager.IsInRoleAsync(user, nameof(Roles.Developer)))
            return Result<DeveloperResponse>.Failure(
                Error.Validation("User.NotDeveloper", "El usuario no tiene rol de Desarrollador.")
            );

        var cedulaResult = IdentityDocument.Create(request.IdentityDocument);
        if (cedulaResult.IsFailure)
            return Result<DeveloperResponse>.Failure(cedulaResult.GetError());

        var cedula = cedulaResult.GetValue();

        if (
            await _userManager.Users.AnyAsync(
                u => u.UserName == request.UserName && u.Id != request.DeveloperId,
                ct
            )
        )
            return Result<DeveloperResponse>.Failure(
                Error.Conflict(
                    "Auth.UserNameTaken",
                    "Ya existe un usuario registrado con este nombre de usuario."
                )
            );

        if (
            await _userManager.Users.AnyAsync(
                u =>
                    u.NormalizedEmail == request.Email.ToUpperInvariant()
                    && u.Id != request.DeveloperId,
                ct
            )
        )
            return Result<DeveloperResponse>.Failure(
                Error.Conflict(
                    "Auth.EmailTaken",
                    "Ya existe un usuario registrado con este correo electrónico."
                )
            );

        if (
            await _userManager.Users.AnyAsync(
                u => u.IdentityDocument == cedula.Value && u.Id != request.DeveloperId,
                ct
            )
        )
            return Result<DeveloperResponse>.Failure(
                Error.Conflict(
                    "Auth.IdentityDocumentTaken",
                    "Ya existe un usuario registrado con esta cédula."
                )
            );

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.UserName = request.UserName;
        user.Email = request.Email;
        user.IdentityDocument = cedula.Value;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Fallo al actualizar dev {DevId}: {Errors}",
                request.DeveloperId,
                errors
            );
            return Result<DeveloperResponse>.Failure(
                Error.Failure("User.UpdateFailed", "No se pudo actualizar el desarrollador.")
            );
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var pwResult = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            if (!pwResult.Succeeded)
            {
                var errors = string.Join(", ", pwResult.Errors.Select(e => e.Description));
                _logger.LogWarning(
                    "Fallo al cambiar contraseña de dev {DevId}: {Errors}",
                    request.DeveloperId,
                    errors
                );
                return Result<DeveloperResponse>.Failure(
                    Error.Failure(
                        "User.PasswordUpdateFailed",
                        "No se pudo actualizar la contraseña."
                    )
                );
            }
        }

        _logger.LogInformation(
            "Admin {AdminId} actualizó desarrollador {DevId}.",
            _currentUser.UserId,
            request.DeveloperId
        );

        var roles = await _userManager.GetRolesAsync(user);
        var response = _mapper.Map<DeveloperResponse>(user);
        return Result<DeveloperResponse>.Success(
            response with
            {
                Roles = roles.ToList().AsReadOnly(),
            }
        );
    }
}
