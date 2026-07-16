using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.UseCases.Property;

public interface IGetPropertyListUseCase
    : IUseCase<GetPropertyListRequest, PagedResult<PropertyListItemResponse>>;
