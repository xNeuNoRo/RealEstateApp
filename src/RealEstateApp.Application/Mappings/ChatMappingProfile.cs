using AutoMapper;
using RealEstateApp.Application.Dtos.Chat.Responses;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Mappings;

public sealed class ChatMappingProfile : Profile
{
    public ChatMappingProfile()
    {
        CreateMap<Message, MessageResponse>()
            .ForMember(d => d.PropertyCode, o => o.MapFrom(s => s.Property.Code.Value))
            .ForMember(
                d => d.SenderId,
                o => o.MapFrom(s => s.SenderType == SenderType.Client ? s.ClientId : s.AgentId)
            )
            .ForMember(d => d.SentAt, o => o.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.SenderName, o => o.Ignore());
    }
}
