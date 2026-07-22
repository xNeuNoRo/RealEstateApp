using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Enums;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using ImprovementEntity = RealEstateApp.Domain.Entities.Improvement;

namespace RealEstateApp.Application.UseCases.Catalog;

public sealed class GetImprovementByIdUseCase : IGetImprovementByIdUseCase
{
    private readonly IGenericRepository<ImprovementEntity> _repository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetImprovementByIdRequest> _validator;

    public GetImprovementByIdUseCase(
        IGenericRepository<ImprovementEntity> repository,
        IMapper mapper,
        ICurrentUserService currentUser,
        IValidator<GetImprovementByIdRequest> validator
    )
    {
        _repository = repository;
        _mapper = mapper;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<Result<ImprovementResponse>> ExecuteAsync(
        GetImprovementByIdRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<ImprovementResponse>();

        if (!_currentUser.IsAuthenticated || _currentUser.UserId is null)
            return Result<ImprovementResponse>.Failure(
                Error.Unauthorized("Auth.NotAuthenticated", "Debe iniciar sesión.")
            );

        if (
            !_currentUser.IsInRole(nameof(Roles.Admin))
            && !_currentUser.IsInRole(nameof(Roles.Developer))
        )
            return Result<ImprovementResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOrDeveloperOnly",
                    "Solo administradores o desarrolladores pueden consultar mejoras."
                )
            );

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result<ImprovementResponse>.Failure(
                Error.NotFound("Improvement.NotFound", "La mejora solicitada no existe.")
            );

        return Result<ImprovementResponse>.Success(_mapper.Map<ImprovementResponse>(entity));
    }
}
