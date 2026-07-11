namespace RealEstateApp.Application.Dtos.Chat.Responses;

public sealed class ConversationSummaryResponse
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string PropertyDescription { get; init; } = null!;
    public string OtherUserId { get; init; } = null!;
    public string OtherUserName { get; set; } = null!;
    public string OtherUserRole { get; init; } = null!;
    public string LastMessageContent { get; init; } = null!;
    public string LastMessageSenderType { get; init; } = null!;
    public DateTimeOffset LastMessageAt { get; init; }
}
