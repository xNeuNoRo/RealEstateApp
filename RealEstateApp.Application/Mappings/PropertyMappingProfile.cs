using AutoMapper;
using RealEstateApp.Application.Dtos.Property.Responses;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Mappings;

public sealed class PropertyMappingProfile : Profile
{
    public PropertyMappingProfile()
    {
        CreateMap<Property, PropertyListItemResponse>()
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
            .ForMember(d => d.Code, o => o.MapFrom(s => s.Code.Value))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Price.Currency))
            .ForMember(d => d.SizeM2, o => o.MapFrom(s => s.Size.Area))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
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
            .ForMember(d => d.AgentName, o => o.Ignore());

        CreateMap<Property, PropertyDetailResponse>()
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
            .ForMember(d => d.Code, o => o.MapFrom(s => s.Code.Value))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Price.Currency))
            .ForMember(d => d.SizeM2, o => o.MapFrom(s => s.Size.Area))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
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
            .ForMember(d => d.AgentId, o => o.MapFrom(s => s.AgentId))
            .ForMember(
                d => d.Images,
                o => o.MapFrom(s => s.Images.OrderBy(i => i.IsMain ? 0 : 1).ThenBy(i => i.Id))
            )
            .ForMember(
                d => d.Improvements,
                o => o.MapFrom(s => s.Improvements.Select(pi => pi.Improvement))
            )
            .ForMember(d => d.AgentName, o => o.Ignore())
            .ForMember(d => d.AgentPhone, o => o.Ignore())
            .ForMember(d => d.AgentEmail, o => o.Ignore())
            .ForMember(d => d.AgentProfileImage, o => o.Ignore());

        CreateMap<Property, PropertySummaryResponse>()
            .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
            .ForMember(d => d.Code, o => o.MapFrom(s => s.Code.Value))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.Price.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Price.Currency))
            .ForMember(d => d.SizeM2, o => o.MapFrom(s => s.Size.Area))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
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
            );

        CreateMap<PropertyImage, PropertyImageDto>()
            .ForMember(d => d.Url, o => o.MapFrom(s => s.Url))
            .ForMember(d => d.IsMain, o => o.MapFrom(s => s.IsMain));

        CreateMap<Improvement, PropertyImprovementDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Description));
    }
}
