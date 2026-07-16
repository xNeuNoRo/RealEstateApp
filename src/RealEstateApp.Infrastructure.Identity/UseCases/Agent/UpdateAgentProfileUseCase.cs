using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Application.Dtos.Agent.Responses;
using RealEstateApp.Application.Interfaces;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Agent;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.UseCases.Agent;

public sealed class UpdateAgentProfileUseCase : IUpdateAgentProfileUseCase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileService _fileService;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateAgentProfileRequest> _validator;
    private readonly ILogger<UpdateAgentProfileUseCase> _logger;

    public UpdateAgentProfileUseCase(
        UserManager<AppUser> userManager,
        ICurrentUserService currentUser,
        IFileService fileService,
        IMapper mapper,
        IValidator<UpdateAgentProfileRequest> validator,
        ILogger<UpdateAgentProfileUseCase> logger
    )
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _fileService = fileService;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<AgentProfileResponse>> ExecuteAsync(
        UpdateAgentProfileRequest request,
        CancellationToken ct = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.ToResult<AgentProfileResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<AgentProfileResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (!_currentUser.IsInRole(nameof(Roles.Agent)))
            return Result<AgentProfileResponse>.Failure(
                Error.Forbidden("Auth.AgentOnly", "Solo los agentes pueden actualizar su perfil.")
            );

        var user = await _userManager.FindByIdAsync(_currentUser.UserId);
        if (user is null)
            return Result<AgentProfileResponse>.Failure(
                Error.NotFound("User.NotFound", "No se encontró su cuenta.")
            );

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.SetPhone(request.Phone.Trim());

        if (request.PhotoFile is not null)
        {
            if (!_fileService.IsImageValid(request.PhotoFile))
                return Result<AgentProfileResponse>.Failure(
                    Error.Validation(
                        "Agent.InvalidImage",
                        $"La imagen no tiene un formato válido (JPEG, PNG o WebP máximo {FileConstants.MaxImageFileSizeBytes / (1024 * 1024)} MB)."
                    )
                );

            user.ProfileImage = await _fileService.UploadFileAsync(request.PhotoFile, "users");
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Fallo al actualizar perfil del agente {UserId}: {Errors}",
                _currentUser.UserId,
                errors
            );
            return Result<AgentProfileResponse>.Failure(
                Error.Failure(
                    "Agent.ProfileUpdateFailed",
                    "No se pudo actualizar su perfil. Intente nuevamente."
                )
            );
        }

        _logger.LogInformation("Agente {UserId} actualizó su perfil.", _currentUser.UserId);

        var response = _mapper.Map<AgentProfileResponse>(user);
        return Result<AgentProfileResponse>.Success(response);
    }
}
