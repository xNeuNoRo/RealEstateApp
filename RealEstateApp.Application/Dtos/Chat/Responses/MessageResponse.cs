using RealEstateApp.Domain.Enums;

namespace RealEstateApp.Application.Dtos.Chat.Responses;

public sealed class MessageResponse
{
    public int Id { get; init; }
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string SenderId { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public SenderType SenderType { get; init; }
    public string Content { get; init; } = null!;
    public DateTimeOffset SentAt { get; init; }
}
