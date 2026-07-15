using AutoMapper;
using FluentValidation;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using ImprovementEntity = RealEstateApp.Domain.Entities.Improvement;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Interfaces.Services;
using RealEstateApp.Application.Interfaces.UseCases.Catalog;
using RealEstateApp.Domain.Interfaces.Persistence;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.UseCases.Improvement;

public sealed class CreateImprovementUseCase : ICreateImprovementUseCase
{
    private readonly IGenericRepository<ImprovementEntity> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateImprovementRequest> _validator;


    public CreateImprovementUseCase(
        IGenericRepository<ImprovementEntity> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        IValidator<CreateImprovementRequest> validator
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<ImprovementResponse>> ExecuteAsync(
        CreateImprovementRequest request,
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

        if (!_currentUser.IsInRole(nameof(Roles.Admin)))
            return Result<ImprovementResponse>.Failure(
                Error.Forbidden(
                    "Auth.AdminOnly",
                    "Solo administradores pueden gestionar mejoras."
                )
            );

        var createResult = ImprovementEntity.Create(request.Name, request.Description);
        if (createResult.IsFailure)
            return Result<ImprovementResponse>.Failure(createResult.GetError());

        var improvement = createResult.GetValue();
        await _repository.AddAsync(improvement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = _mapper.Map<ImprovementResponse>(improvement);
        return Result<ImprovementResponse>.Success(response);
    }
}