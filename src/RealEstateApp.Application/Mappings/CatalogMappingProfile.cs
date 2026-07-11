using AutoMapper;
using RealEstateApp.Application.Dtos.Catalog.Responses;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Application.Mappings;

public sealed class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        CreateMap<Improvement, ImprovementResponse>();

        CreateMap<PropertyType, PropertyTypeResponse>();

        CreateMap<SaleType, SaleTypeResponse>()
            .ForMember(d => d.Code, o => o.MapFrom(s => s.Code.ToString()));
    }
}
