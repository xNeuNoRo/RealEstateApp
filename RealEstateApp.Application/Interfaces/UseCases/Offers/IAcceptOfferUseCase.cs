using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Offers.Requests;
using RealEstateApp.Application.Dtos.Offers.Responses;

namespace RealEstateApp.Application.Interfaces.UseCases.Offers;

public interface IAcceptOfferUseCase : IUseCase<AcceptOfferRequest, OfferResponse>;
