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

public sealed class UpdateAdminUseCase : IUpdateAdminUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateAdminRequest> _validator;
    private readonly ILogger<UpdateAdminUseCase> _logger;

    public UpdateAdminUseCase(
        UserManager<AppUser> userManager,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<UpdateAdminRequest> validator,
        ILogger<UpdateAdminUseCase> logger
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AdminResponse>> ExecuteAsync(
        UpdateAdminRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult<AdminResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<AdminResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result<AdminResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden editar administradores."
                )
            );

        if (request.AdminId == _currentUser.UserId)
            return Result<AdminResponse>.Failure(
                Error.Validation(
                    "Admin.SelfEdit",
                    "No puede editar su propio usuario desde este mantenimiento."
                )
            );

        var user = await _userManager.FindByIdAsync(request.AdminId);
        if (user is null)
            return Result<AdminResponse>.Failure(
                Error.NotFound("User.NotFound", "Administrador no encontrado.")
            );

        if (!await _userManager.IsInRoleAsync(user, nameof(Roles.Admin)))
            return Result<AdminResponse>.Failure(
                Error.Validation("User.NotAdmin", "El usuario no tiene rol de Administrador.")
            );

        var cedulaResult = IdentityDocument.Create(request.IdentityDocument);
        if (cedulaResult.IsFailure)
            return Result<AdminResponse>.Failure(cedulaResult.GetError());

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result<AdminResponse>.Failure(emailResult.GetError());

        var cedula = cedulaResult.GetValue();
        var email = emailResult.GetValue();

        if (
            await _userManager.Users.AnyAsync(
                u => u.UserName == request.UserName && u.Id != request.AdminId,
                ct
            )
        )
            return Result<AdminResponse>.Failure(
                Error.Conflict(
                    "Auth.UserNameTaken",
                    "Ya existe un usuario registrado con este nombre de usuario."
                )
            );

        if (
            await _userManager.Users.AnyAsync(
                u =>
                    u.NormalizedEmail == email.Value.ToUpperInvariant()
                    && u.Id != request.AdminId,
                ct
            )
        )
            return Result<AdminResponse>.Failure(
                Error.Conflict(
                    "Auth.EmailTaken",
                    "Ya existe un usuario registrado con este correo electrónico."
                )
            );

        if (
            await _userManager.Users.AnyAsync(
                u => u.IdentityDocument == cedula.Value && u.Id != request.AdminId,
                ct
            )
        )
            return Result<AdminResponse>.Failure(
                Error.Conflict(
                    "Auth.IdentityDocumentTaken",
                    "Ya existe un usuario registrado con esta cédula."
                )
            );

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.UserName = request.UserName;
        user.Email = email.Value;
        user.IdentityDocument = cedula.Value;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Fallo al actualizar admin {AdminId}: {Errors}",
                request.AdminId,
                errors
            );
            return Result<AdminResponse>.Failure(
                Error.Failure("User.UpdateFailed", "No se pudo actualizar el administrador.")
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
                    "Fallo al cambiar contraseña de admin {AdminId}: {Errors}",
                    request.AdminId,
                    errors
                );
                return Result<AdminResponse>.Failure(
                    Error.Failure(
                        "User.PasswordUpdateFailed",
                        "No se pudo actualizar la contraseña."
                    )
                );
            }
        }

        _logger.LogInformation(
            "Admin {CurrentAdminId} actualizó admin {TargetAdminId}.",
            _currentUser.UserId,
            request.AdminId
        );

        var roles = await _userManager.GetRolesAsync(user);
        var response = _mapper.Map<AdminResponse>(user);
        return Result<AdminResponse>.Success(response with { Roles = roles.ToList().AsReadOnly() });
    }
}
