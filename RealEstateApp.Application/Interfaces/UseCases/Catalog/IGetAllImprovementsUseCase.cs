using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Catalog.Requests;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.UseCases.Catalog;

public interface IGetAllImprovementsUseCase
    : IUseCase<GetAllImprovementsRequest, PagedResult<ImprovementResponse>>;
