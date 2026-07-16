using AutoMapper;
using RealEstateApp.Application.Dtos.Client.Responses;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Mappings;

public sealed class ClientMappingProfile : Profile
{
    public ClientMappingProfile()
    {
        CreateMap<UserInfo, ClientProfileResponse>();
    }
}
