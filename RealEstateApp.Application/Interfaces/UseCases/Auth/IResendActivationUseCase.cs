using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Auth.Requests;

namespace RealEstateApp.Application.Interfaces.UseCases.Auth;

public interface IResendActivationUseCase : IUseCase<ResendActivationRequest>;
