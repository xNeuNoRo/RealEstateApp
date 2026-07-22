using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Client.Requests;
using RealEstateApp.Application.Dtos.Client.Responses;

namespace RealEstateApp.Application.Interfaces.UseCases.Client;

public interface IGetClientDashboardUseCase
    : IUseCase<GetClientDashboardRequest, ClientDashboardResponse>;
