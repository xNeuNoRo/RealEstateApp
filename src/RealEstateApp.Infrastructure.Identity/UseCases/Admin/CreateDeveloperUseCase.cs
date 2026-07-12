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

public sealed class CreateDeveloperUseCase : ICreateDeveloperUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateDeveloperRequest> _validator;
    private readonly ILogger<CreateDeveloperUseCase> _logger;

    public CreateDeveloperUseCase(
        UserManager<AppUser> userManager,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<CreateDeveloperRequest> validator,
        ILogger<CreateDeveloperUseCase> logger
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<DeveloperResponse>> ExecuteAsync(
        CreateDeveloperRequest request,
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
                    "Solo administradores pueden crear desarrolladores."
                )
            );

        var cedulaResult = IdentityDocument.Create(request.IdentityDocument);
        if (cedulaResult.IsFailure)
            return Result<DeveloperResponse>.Failure(cedulaResult.GetError());

        var cedula = cedulaResult.GetValue();

        if (await _userManager.FindByNameAsync(request.UserName) is not null)
            return Result<DeveloperResponse>.Failure(
                Error.Conflict(
                    "Auth.UserNameTaken",
                    "Ya existe un usuario registrado con este nombre de usuario."
                )
            );

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
            return Result<DeveloperResponse>.Failure(
                Error.Conflict(
                    "Auth.EmailTaken",
                    "Ya existe un usuario registrado con este correo electrónico."
                )
            );

        if (await _userManager.Users.AnyAsync(u => u.IdentityDocument == cedula.Value, ct))
            return Result<DeveloperResponse>.Failure(
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
            Email = request.Email,
            IdentityDocument = cedula.Value,
            Active = true,
            EmailConfirmed = true,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Registro de developer fallido para {User}: {Errors}",
                request.UserName,
                errors
            );
            return Result<DeveloperResponse>.Failure(
                Error.Failure("Auth.RegistrationFailed", "No fue posible completar el registro.")
            );
        }

        var roleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.Developer);
        if (!roleResult.Succeeded)
        {
            _logger.LogError("No se pudo asignar rol Developer a {UserId}.", user.Id);
            return Result<DeveloperResponse>.Failure(
                Error.Failure("Auth.RoleAssignmentFailed", "No se pudo completar el registro.")
            );
        }

        _logger.LogInformation(
            "Admin {AdminId} creó desarrollador {DevId}.",
            _currentUser.UserId,
            user.Id
        );

        var response = _mapper.Map<DeveloperResponse>(user);
        return Result<DeveloperResponse>.Success(
            response with
            {
                Roles = new[] { DefaultRoles.Developer },
            }
        );
    }
}
