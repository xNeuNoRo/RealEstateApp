using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.UseCases.Offers;

public interface IGetPropertyOfferClientsUseCase
    : IUseCase<GetPropertyOfferClientsRequest, PagedResult<OfferClientSummaryResponse>>;
