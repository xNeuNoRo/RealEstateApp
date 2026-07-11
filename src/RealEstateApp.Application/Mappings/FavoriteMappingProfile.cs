using AutoMapper;
using RealEstateApp.Application.Dtos.Favorites.Responses;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Mappings;

public sealed class FavoriteMappingProfile : Profile
{
    public FavoriteMappingProfile()
    {
        CreateMap<Property, FavoriteResponse>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PropertyId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Code, o => o.MapFrom(s => s.Code.Value))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Price.Currency))
            .ForMember(
                d => d.MainImageUrl,
                o =>
                    o.MapFrom(s =>
                        s.Images.FirstOrDefault(i => i.IsMain) != null
                            ? s.Images.First(i => i.IsMain).Url
                            : null
                    )
            )
            .ForMember(
                d => d.PropertyTypeName,
                o => o.MapFrom(s => s.PropertyType != null ? s.PropertyType.Name : null)
            )
            .ForMember(
                d => d.SaleTypeName,
                o => o.MapFrom(s => s.SaleType != null ? s.SaleType.Name : null)
            )
            .ForMember(d => d.AgentName, o => o.Ignore())
            .ForMember(d => d.FavoritedAt, o => o.Ignore());
    }
}
