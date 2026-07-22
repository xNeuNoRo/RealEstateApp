using AutoMapper;
using RealEstateApp.Application.Dtos.Offers.Responses;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Common;

namespace RealEstateApp.Application.Mappings;

public sealed class OfferMappingProfile : Profile
{
    public OfferMappingProfile()
    {
        CreateMap<Offer, OfferResponse>()
            .ForMember(d => d.PropertyCode, o => o.MapFrom(s => s.Property != null ? s.Property.Code.Value : string.Empty))
            .ForMember(d => d.PropertyTitle, o => o.MapFrom(s => s.Property != null ? s.Property.Title : string.Empty))
            .ForMember(d => d.PropertyDescription, o => o.MapFrom(s => s.Property != null ? s.Property.Description : string.Empty))
            .ForMember(d => d.PropertyTypeName, o => o.MapFrom(s => s.Property != null && s.Property.PropertyType != null ? s.Property.PropertyType.Name : null))
            .ForMember(d => d.SaleTypeName, o => o.MapFrom(s => s.Property != null && s.Property.SaleType != null ? s.Property.SaleType.Name : null))
            .ForMember(
                d => d.PropertyMainImageUrl,
                o =>
                    o.MapFrom(s =>
                        s.Property != null && s.Property.Images.FirstOrDefault(i => i.IsMain) != null
                            ? s.Property.Images.First(i => i.IsMain).Url
                            : null
                    )
            )
            .ForMember(d => d.PropertyPrice, o => o.MapFrom(s => s.Property != null ? s.Property.Price.Amount : 0))
            .ForMember(d => d.PropertyCurrency, o => o.MapFrom(s => s.Property != null ? s.Property.Price.Currency : "DOP"))
            .ForMember(d => d.PropertyStatus, o => o.MapFrom(s => s.Property != null ? s.Property.Status.ToString() : string.Empty))
            .ForMember(d => d.ClientName, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<OfferClientSummary, OfferClientSummaryResponse>()
            .ForMember(destination => destination.ClientName, options => options.Ignore())
            .ForMember(
                destination => destination.LastStatus,
                options => options.MapFrom(source => source.LastStatus.ToString())
            );
    }
}
