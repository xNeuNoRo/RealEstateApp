using AutoMapper;
using RealEstateApp.Application.Dtos.Admin.Responses;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Mappings;

public class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        CreateMap<AppUser, AgentListItemResponse>()
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.Active))
            .ForMember(d => d.PropertiesCount, o => o.Ignore())
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone));

        CreateMap<AppUser, AdminListItemResponse>()
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.Active));

        CreateMap<AppUser, AdminResponse>()
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.Active))
            .ForMember(d => d.Roles, o => o.Ignore());

        CreateMap<AppUser, DeveloperListItemResponse>()
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.Active));

        CreateMap<AppUser, DeveloperResponse>()
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.Active))
            .ForMember(d => d.Roles, o => o.Ignore());
    }
}
