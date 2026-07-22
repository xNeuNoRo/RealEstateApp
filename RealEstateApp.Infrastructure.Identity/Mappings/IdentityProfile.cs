using AutoMapper;
using RealEstateApp.Application.Dtos.Agent.Responses;
using RealEstateApp.Application.Dtos.Auth;
using RealEstateApp.Application.Dtos.Client.Responses;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Mappings;

/// <summary>
/// Perfiles de mapeo entre AppUser y los DTOs de autenticación.
/// </summary>
public class IdentityProfile : Profile
{
    public IdentityProfile()
    {
        CreateMap<AppUser, UserDto>();
        CreateMap<AppUser, RegisterResponseDto>();
        CreateMap<AppUser, ClientProfileResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        CreateMap<AppUser, AgentProfileResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
    }
}
