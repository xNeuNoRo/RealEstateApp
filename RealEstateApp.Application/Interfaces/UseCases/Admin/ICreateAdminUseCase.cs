using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;

namespace RealEstateApp.Application.Interfaces.UseCases.Admin;

public interface ICreateAdminUseCase : IUseCase<CreateAdminRequest, AdminResponse>;
