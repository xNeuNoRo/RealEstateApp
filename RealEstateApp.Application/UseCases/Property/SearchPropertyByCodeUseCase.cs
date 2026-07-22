using AutoMapper;
using FluentValidation;
using RealEstateApp.Application.Common.Validation;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Application.Interfaces.UseCases.Property;
using RealEstateApp.Domain.Common;
using RealEstateApp.Domain.Entities;
using ImprovementEntity = RealEstateApp.Domain.Entities.Improvement;
using RealEstateApp.Domain.Interfaces.Persistence.Repositories;
using RealEstateApp.Domain.ValueObjects;
using PropertyEntity = RealEstateApp.Domain.Entities.Property;

namespace RealEstateApp.Application.UseCases.Property;

public sealed class SearchPropertyByCodeUseCase : ISearchPropertyByCodeUseCase
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IGenericRepository<ImprovementEntity> _improvementRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<SearchPropertyByCodeRequest> _validator;

    public SearchPropertyByCodeUseCase(
        IPropertyRepository propertyRepository,
        IGenericRepository<ImprovementEntity> improvementRepository,
        IUserRepository userRepository,
        IMapper mapper,
        IValidator<SearchPropertyByCodeRequest> validator
    )
    {
        _propertyRepository = propertyRepository;
        _improvementRepository = improvementRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PropertyDetailResponse>> ExecuteAsync(
        SearchPropertyByCodeRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PropertyDetailResponse>();

        var codeResult = PropertyCode.Create(request.Code);
        if (codeResult.IsFailure)
            return Result<PropertyDetailResponse>.Failure(codeResult.GetError());

        var property = await _propertyRepository.GetFirstOrDefaultAsync(
            new QueryOptions<PropertyEntity>
            {
                Filter = p => p.Code == codeResult.Value,
                Includes =
                [
                    p => p.PropertyType!,
                    p => p.SaleType!,
                    p => p.Images,
                    p => p.Improvements,
                ],
                UseSplitQuery = true,
            },
            cancellationToken
        );

        if (property is null)
            return Result<PropertyDetailResponse>.Failure(
                Error.NotFound(
                    "Property.NotFound",
                    "No se encontró una propiedad con el código especificado."
                )
            );

        var response = _mapper.Map<PropertyDetailResponse>(property);

        var improvementIds = property.Improvements.Select(pi => pi.ImprovementId).ToList();
        if (improvementIds.Count > 0)
        {
            var improvements = await _improvementRepository.GetAllAsync(
                new QueryOptions<ImprovementEntity> { Filter = imp => improvementIds.Contains(imp.Id) },
                cancellationToken
            );

            response.Improvements = improvements
                .Select(i => new PropertyImprovementDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                })
                .ToList();
        }

        var agent = await _userRepository.GetByIdAsync(property.AgentId, cancellationToken);
        if (agent is not null)
        {
            response.AgentName = $"{agent.FirstName} {agent.LastName}".Trim();
            response.AgentPhone = agent.Phone;
            response.AgentEmail = agent.Email;
            response.AgentProfileImage = agent.ProfileImage;
        }

        return Result<PropertyDetailResponse>.Success(response);
    }
}
