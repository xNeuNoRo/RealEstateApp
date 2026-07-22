using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Agent.Requests;
using RealEstateApp.Application.Dtos.Agent.Responses;

namespace RealEstateApp.Application.Interfaces.UseCases.Agent;

public interface IUpdateAgentProfileUseCase
    : IUseCase<UpdateAgentProfileRequest, AgentProfileResponse>;
