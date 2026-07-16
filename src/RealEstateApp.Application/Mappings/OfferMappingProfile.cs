using AutoMapper;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Mappings;

public sealed class OfferMappingProfile : Profile
{
    public OfferMappingProfile()
    {
        CreateMap<Offer, OfferResponse>()
            .ForMember(d => d.PropertyCode, o => o.Ignore())
            .ForMember(d => d.ClientName, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}
