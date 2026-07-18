namespace RealEstateApp.Application.ViewModels.Chat;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class ConversationSummaryViewModel : BaseViewModel
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string PropertyDescription { get; init; } = null!;
    public string OtherUserId { get; init; } = null!;
    public string OtherUserName { get; init; } = null!;
    public string OtherUserRole { get; init; } = null!;
    public string LastMessageContent { get; init; } = null!;
    public string LastMessageSenderType { get; init; } = null!;
    public DateTimeOffset LastMessageAt { get; init; }
}
