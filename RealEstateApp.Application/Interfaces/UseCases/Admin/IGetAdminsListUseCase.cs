using RealEstateApp.Application.Common.Interfaces;
using RealEstateApp.Application.Dtos.Admin.Requests;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Interfaces.UseCases.Admin;

public interface IGetAdminsListUseCase
    : IUseCase<GetAdminsListRequest, PagedResult<AdminListItemResponse>>;
