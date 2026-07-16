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

namespace RealEstateApp.Application.UseCases.Property;

public sealed class GetPropertyDetailUseCase : IGetPropertyDetailUseCase
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IGenericRepository<ImprovementEntity> _improvementRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetPropertyDetailRequest> _validator;

    public GetPropertyDetailUseCase(
        IPropertyRepository propertyRepository,
        IGenericRepository<ImprovementEntity> improvementRepository,
        IUserRepository userRepository,
        IMapper mapper,
        IValidator<GetPropertyDetailRequest> validator
    )
    {
        _propertyRepository = propertyRepository;
        _improvementRepository = improvementRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<Result<PropertyDetailResponse>> ExecuteAsync(
        GetPropertyDetailRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToResult<PropertyDetailResponse>();

        var property = await _propertyRepository.GetByIdAsync(
            request.PropertyId,
            cancellationToken,
            p => p.PropertyType!,
            p => p.SaleType!,
            p => p.Images,
            p => p.Improvements
        );

        if (property is null)
            return Result<PropertyDetailResponse>.Failure(
                Error.NotFound("Property.NotFound", "No se encontró la propiedad especificada.")
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
