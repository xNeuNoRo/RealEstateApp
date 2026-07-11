using AutoMapper;
using RealEstateApp.Application.Dtos.Auth;
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
    }
}
