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
using RealEstateApp.Infrastructure.Identity.Seeds;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Admin;

public sealed class CreateAdminUseCase : ICreateAdminUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateAdminRequest> _validator;
    private readonly ILogger<CreateAdminUseCase> _logger;

    public CreateAdminUseCase(
        UserManager<AppUser> userManager,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<CreateAdminRequest> validator,
        ILogger<CreateAdminUseCase> logger
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AdminResponse>> ExecuteAsync(
        CreateAdminRequest request,
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
                    "Solo administradores pueden crear administradores."
                )
            );

        var cedulaResult = IdentityDocument.Create(request.IdentityDocument);
        if (cedulaResult.IsFailure)
            return Result<AdminResponse>.Failure(cedulaResult.GetError());

        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result<AdminResponse>.Failure(emailResult.GetError());

        var cedula = cedulaResult.GetValue();
        var email = emailResult.GetValue();

        if (await _userManager.FindByNameAsync(request.UserName) is not null)
            return Result<AdminResponse>.Failure(
                Error.Conflict(
                    "Auth.UserNameTaken",
                    "Ya existe un usuario registrado con este nombre de usuario."
                )
            );

        if (await _userManager.FindByEmailAsync(email.Value) is not null)
            return Result<AdminResponse>.Failure(
                Error.Conflict(
                    "Auth.EmailTaken",
                    "Ya existe un usuario registrado con este correo electrónico."
                )
            );

        if (await _userManager.Users.AnyAsync(u => u.IdentityDocument == cedula.Value, ct))
            return Result<AdminResponse>.Failure(
                Error.Conflict(
                    "Auth.IdentityDocumentTaken",
                    "Ya existe un usuario registrado con esta cédula."
                )
            );

        var user = new AppUser
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            UserName = request.UserName,
            Email = email.Value,
            IdentityDocument = cedula.Value,
            Active = true,
            EmailConfirmed = true,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Registro de admin fallido para {User}: {Errors}",
                request.UserName,
                errors
            );
            return Result<AdminResponse>.Failure(
                Error.Failure("Auth.RegistrationFailed", "No fue posible completar el registro.")
            );
        }

        var roleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.Admin);
        if (!roleResult.Succeeded)
        {
            _logger.LogError("No se pudo asignar rol Admin a {UserId}.", user.Id);
            return Result<AdminResponse>.Failure(
                Error.Failure("Auth.RoleAssignmentFailed", "No se pudo completar el registro.")
            );
        }

        _logger.LogInformation(
            "Admin {CurrentAdminId} creó administrador {NewAdminId}.",
            _currentUser.UserId,
            user.Id
        );

        var response = _mapper.Map<AdminResponse>(user);
        return Result<AdminResponse>.Success(
            response with
            {
                Roles = new[] { DefaultRoles.Admin },
            }
        );
    }
}
