using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Application.Dtos.Agent.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.UseCases.Agent;

public interface IGetPublicAgentsUseCase
    : IUseCase<GetPublicAgentsRequest, PagedResult<PublicAgentResponse>>;
