using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Client.Requests;
using RealEstateApp.Application.Dtos.Client.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Client;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;

namespace RealEstateApp.Application.UseCases.Client;

public sealed class GetClientProfileUseCase : IGetClientProfileUseCase
{
    private readonly IUserRepository _userRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<GetClientProfileRequest> _validator;

    public GetClientProfileUseCase(
        IUserRepository userRepo,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<GetClientProfileRequest> validator
    )
    {
        _userRepo = userRepo;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<ClientProfileResponse>> ExecuteAsync(
        GetClientProfileRequest request,
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
                Error.Forbidden("Auth.ClientOnly", "Solo los clientes pueden consultar su perfil.")
            );

        var userInfo = await _userRepo.GetByIdAsync(_currentUser.UserId, ct);
        if (userInfo is null)
            return Result<ClientProfileResponse>.Failure(
                Error.NotFound("User.NotFound", "No se encontró su información de perfil.")
            );

        var response = _mapper.Map<ClientProfileResponse>(userInfo);
        return Result<ClientProfileResponse>.Success(response);
    }
}
