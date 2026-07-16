using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Catalog.Responses;

namespace RealEstateApp.Application.Interfaces.UseCases.Catalog;

public interface ICreatePropertyTypeUseCase
    : IUseCase<CreatePropertyTypeRequest, PropertyTypeResponse>;
