using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Dtos.Auth;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Exceptions;
using RealEstateApp.Domain.ValueObjects;
using RealEstateApp.Infrastructure.Identity.Entities;
using RealEstateApp.Infrastructure.Identity.Seeds;

namespace RealEstateApp.Infrastructure.Identity.Services;

/// <summary>
/// Lógica compartida de gestión de usuarios para WebApp y WebApi.
/// </summary>
public abstract class BaseAccountService
{
    protected readonly UserManager<AppUser> UserManager;
    protected readonly IMapper Mapper;
    protected readonly ILogger Logger;

    protected BaseAccountService(
        UserManager<AppUser> userManager,
        IMapper mapper,
        ILogger logger
    )
    {
        UserManager = userManager;
        Mapper = mapper;
        Logger = logger;
    }

    public async Task<RegisterResponseDto> RegisterUserAsync(RegisterUserDto dto)
    {
        // Validamos rol permitido
        if (!DefaultRoles.All.Contains(dto.Role))
            throw new DomainException(
                "Auth.InvalidRole",
                $"El rol '{dto.Role}' no es un rol válido."
            );

        // Validamos que el userName sea único
        if (await UserManager.FindByNameAsync(dto.UserName) is not null)
            throw new DomainException(
                "Auth.UserNameTaken",
                "Ya existe un usuario registrado con este nombre de usuario."
            );

        // Validamos que el email sea único
        if (await UserManager.FindByEmailAsync(dto.Email) is not null)
            throw new DomainException(
                "Auth.EmailTaken",
                "Ya existe un usuario registrado con este correo electrónico."
            );

        // Validamos la cédula con VO IdentityDocument del Domain
        var identityResult = IdentityDocument.Create(dto.IdentityDocument);
        if (identityResult.IsFailure)
        {
            var error =
                identityResult.Error
                ?? Error.Validation(
                    "IdentityDocument.Invalid",
                    "El documento de identidad no es válido."
                );
            throw new DomainException(error.Code, error.Message);
        }

        // Validamos que la cédula no esté registrada
        var identityDocument = identityResult.Value!.Value;
        if (await UserManager.Users.AnyAsync(u => u.IdentityDocument == identityDocument))
            throw new DomainException(
                "Auth.IdentityDocumentTaken",
                "Ya existe un usuario registrado con esta cédula."
            );

        // Validamos la confirmación de contraseña
        if (dto.Password != dto.ConfirmPassword)
            throw new DomainException(
                "Auth.PasswordMismatch",
                "La contraseña y la confirmación de contraseña no coinciden."
            );

        var user = new AppUser
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            UserName = dto.UserName,
            Email = dto.Email,
            IdentityDocument = identityDocument,
            Active = true,
            EmailConfirmed = true,
        };

        var result = await UserManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            Logger.LogWarning(
                "Registro fallido para {User}: {Errors}",
                dto.UserName,
                string.Join(", ", errors)
            );
            throw new DomainException(
                "Auth.RegistrationFailed",
                "No se pudo completar el registro.",
                errors
            );
        }

        var roleResult = await UserManager.AddToRoleAsync(user, dto.Role);
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors.Select(e => e.Description).ToList();
            Logger.LogError(
                "Rol '{Role}' no pudo asignarse a {User}: {Errors}",
                dto.Role,
                dto.UserName,
                string.Join(", ", errors)
            );
            throw new DomainException(
                "Auth.RoleAssignmentFailed",
                "No se pudo asignar el rol al usuario.",
                errors
            );
        }

        var roles = await UserManager.GetRolesAsync(user);
        var response = Mapper.Map<RegisterResponseDto>(user);
        response.Roles = roles.ToList().AsReadOnly();

        Logger.LogInformation(
            "Usuario {User} registrado con rol {Role}",
            dto.UserName,
            dto.Role
        );

        return response;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await UserManager.Users.ToListAsync();
        var dtos = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            var roles = await UserManager.GetRolesAsync(user);
            var dto = Mapper.Map<UserDto>(user);
            dto.Roles = roles.ToList().AsReadOnly();
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await UserManager.FindByIdAsync(id);
        return await MapUserWithRolesAsync(user);
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await UserManager.FindByEmailAsync(email);
        return await MapUserWithRolesAsync(user);
    }

    public async Task<UserDto?> GetUserByUserNameAsync(string userName)
    {
        var user = await UserManager.FindByNameAsync(userName);
        return await MapUserWithRolesAsync(user);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await UserManager.FindByIdAsync(id);
        if (user is null)
            return false;

        var result = await UserManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            Logger.LogInformation("Usuario {UserId} eliminado.", id);
            return true;
        }

        Logger.LogWarning("Fallo al eliminar usuario {UserId}.", id);
        return false;
    }

    public async Task<UserDto?> EditUserAsync(EditUserDto dto)
    {
        var user = await UserManager.FindByIdAsync(dto.Id);
        if (user is null)
            return null;

        // Validamos rol permitido
        if (!DefaultRoles.All.Contains(dto.Role))
            throw new DomainException(
                "Auth.InvalidRole",
                $"El rol '{dto.Role}' no es un rol válido."
            );

        // Validamos que el userName sea único (excluyendo el propio usuario)
        if (await UserManager.Users.AnyAsync(u => u.UserName == dto.UserName && u.Id != dto.Id))
            throw new DomainException(
                "Auth.UserNameTaken",
                "Ya existe un usuario registrado con este nombre de usuario."
            );

        // Validamos que el email sea único (excluyendo el propio usuario)
        if (
            await UserManager.Users.AnyAsync(u =>
                u.NormalizedEmail == dto.Email.ToUpperInvariant() && u.Id != dto.Id
            )
        )
            throw new DomainException(
                "Auth.EmailTaken",
                "Ya existe un usuario registrado con este correo electrónico."
            );

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.UserName = dto.UserName;
        user.Email = dto.Email;
        user.SetPhone(dto.Phone);
        user.ProfileImage = dto.ProfileImage;

        var result = await UserManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new DomainException(
                "Auth.EditFailed",
                "No se pudo actualizar el usuario.",
                errors
            );
        }

        // Reasignamos el rol
        var currentRoles = await UserManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
            await UserManager.RemoveFromRolesAsync(user, currentRoles);

        var roleResult = await UserManager.AddToRoleAsync(user, dto.Role);
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors.Select(e => e.Description).ToList();
            Logger.LogError(
                "Rol '{Role}' no pudo asignarse a {UserId}: {Errors}",
                dto.Role,
                dto.Id,
                string.Join(", ", errors)
            );
            throw new DomainException(
                "Auth.RoleAssignmentFailed",
                "No se pudo asignar el rol al usuario.",
                errors
            );
        }

        var roles = await UserManager.GetRolesAsync(user);
        var response = Mapper.Map<UserDto>(user);
        response.Roles = roles.ToList().AsReadOnly();

        Logger.LogInformation("Usuario {UserId} actualizado con rol {Role}.", dto.Id, dto.Role);

        return response;
    }

    public async Task<bool> ActivateUserAsync(string id)
    {
        var user = await UserManager.FindByIdAsync(id);
        if (user is null)
            return false;

        user.Active = true;
        var result = await UserManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            Logger.LogInformation("Usuario {UserId} activado.", id);
            return true;
        }

        Logger.LogWarning("Fallo al activar usuario {UserId}.", id);
        return false;
    }

    public async Task<bool> DeactivateUserAsync(string id)
    {
        var user = await UserManager.FindByIdAsync(id);
        if (user is null)
            return false;

        user.Active = false;
        var result = await UserManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            Logger.LogInformation("Usuario {UserId} desactivado.", id);
            return true;
        }

        Logger.LogWarning("Fallo al desactivar usuario {UserId}.", id);
        return false;
    }

    private async Task<UserDto?> MapUserWithRolesAsync(AppUser? user)
    {
        if (user is null)
            return null;

        var roles = await UserManager.GetRolesAsync(user);
        var dto = Mapper.Map<UserDto>(user);
        dto.Roles = roles.ToList().AsReadOnly();
        return dto;
    }
}
