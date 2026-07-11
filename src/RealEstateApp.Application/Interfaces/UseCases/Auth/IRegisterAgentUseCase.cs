using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Application.Dtos.Auth.Responses;

namespace RealEstateApp.Application.Interfaces.UseCases.Auth;

public interface IRegisterAgentUseCase : IUseCase<RegisterAgentRequest, AuthResponse>;
