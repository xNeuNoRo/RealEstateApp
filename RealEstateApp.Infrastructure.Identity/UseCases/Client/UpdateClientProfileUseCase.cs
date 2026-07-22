using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Client.Requests;
using RealEstateApp.Application.Dtos.Client.Responses;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Client;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Domain.ValueObjects;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Client;

public sealed class UpdateClientProfileUseCase : IUpdateClientProfileUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateClientProfileRequest> _validator;
    private readonly ILogger<UpdateClientProfileUseCase> _logger;

    public UpdateClientProfileUseCase(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ICurrentUserService currentUser,
        IFileService fileService,
        IMapper mapper,
        IValidator<UpdateClientProfileRequest> validator,
        ILogger<UpdateClientProfileUseCase> logger
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _currentUser = currentUser;
        _fileService = fileService;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ClientProfileResponse>> ExecuteAsync(
        UpdateClientProfileRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult<ClientProfileResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<ClientProfileResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Client)))
            return Result<ClientProfileResponse>.Failure(
                Error.Forbidden("Auth.ClientOnly", "Solo los clientes pueden actualizar su perfil.")
            );

        var user = await _userManager.FindByIdAsync(_currentUser.UserId);
        if (user is null)
            return Result<ClientProfileResponse>.Failure(
                Error.NotFound("User.NotFound", "No se encontró su cuenta.")
            );

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();

        var phoneResult = PhoneNumber.Create(request.Phone);
        if (phoneResult.IsFailure)
            return Result<ClientProfileResponse>.Failure(phoneResult.GetError());
        user.SetPhone(phoneResult.GetValue().Value);

        string? newProfileImage = null;
        var previousProfileImage = user.ProfileImage;
        if (request.PhotoFile is not null)
        {
            if (!_fileService.IsImageValid(request.PhotoFile))
                return Result<ClientProfileResponse>.Failure(
                    Error.Validation(
                        "Client.InvalidImage",
                        $"La imagen no tiene un formato válido (JPEG, PNG o WebP máximo {FileConstants.MaxImageFileSizeBytes / (1024 * 1024)} MB)."
                    )
                );

            newProfileImage = await _fileService.UploadFileAsync(request.PhotoFile, "users");
            user.ProfileImage = newProfileImage;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            if (newProfileImage is not null)
                await _fileService.DeleteFileAsync(newProfileImage);

            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Fallo al actualizar perfil del cliente {UserId}: {Errors}",
                _currentUser.UserId,
                errors
            );
            return Result<ClientProfileResponse>.Failure(
                Error.Failure(
                    "Client.ProfileUpdateFailed",
                    "No se pudo actualizar su perfil. Intente nuevamente."
                )
            );
        }

        if (newProfileImage is not null && !string.IsNullOrWhiteSpace(previousProfileImage))
            await _fileService.DeleteFileAsync(previousProfileImage);

        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("Cliente {UserId} actualizó su perfil.", _currentUser.UserId);

        var response = _mapper.Map<ClientProfileResponse>(user);
        return Result<ClientProfileResponse>.Success(response);
    }
}
