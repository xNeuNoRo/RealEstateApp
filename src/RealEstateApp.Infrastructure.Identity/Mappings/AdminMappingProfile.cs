using AutoMapper;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Domain.Common;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Mappings;

public class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        CreateMap<AppUser, AgentListItemResponse>()
            .ConstructUsing(s => new AgentListItemResponse(
                s.Id, s.FirstName, s.LastName,
                s.Email, s.UserName, 0, s.Active, s.Phone));

        CreateMap<AppUser, AdminListItemResponse>()
            .ConstructUsing(s => new AdminListItemResponse(
                s.Id, s.FirstName, s.LastName,
                s.UserName, null, s.Email, s.Active));

        CreateMap<AppUser, AdminResponse>()
            .ConstructUsing(s => new AdminResponse(
                s.Id, s.FirstName, s.LastName,
                s.UserName, s.IdentityDocument, s.Email,
                s.Active, s.CreatedAt, Array.Empty<string>()));

        CreateMap<AppUser, DeveloperListItemResponse>()
            .ConstructUsing(s => new DeveloperListItemResponse(
                s.Id, s.FirstName, s.LastName,
                s.UserName, null, s.Email, s.Active));

        CreateMap<AppUser, DeveloperResponse>()
            .ConstructUsing(s => new DeveloperResponse(
                s.Id, s.FirstName, s.LastName,
                s.UserName, s.IdentityDocument, s.Email,
                s.Active, s.CreatedAt, Array.Empty<string>()));

        CreateMap<UserInfo, AgentListItemResponse>()
            .ConstructUsing(s => new AgentListItemResponse(
                s.Id, s.FirstName, s.LastName,
                s.Email, s.UserName, 0, s.IsActive, s.Phone));

        CreateMap<UserInfo, AdminListItemResponse>()
            .ConstructUsing(s => new AdminListItemResponse(
                s.Id, s.FirstName, s.LastName,
                s.UserName, null, s.Email, s.IsActive));

        CreateMap<UserInfo, DeveloperListItemResponse>()
            .ConstructUsing(s => new DeveloperListItemResponse(
                s.Id, s.FirstName, s.LastName,
                s.UserName, null, s.Email, s.IsActive));
    }
}
