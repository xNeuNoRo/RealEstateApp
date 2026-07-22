using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;

namespace RealEstateApp.Application.Interfaces.UseCases.Property;

public interface ISearchPropertyByCodeUseCase
    : IUseCase<SearchPropertyByCodeRequest, PropertyDetailResponse>;
