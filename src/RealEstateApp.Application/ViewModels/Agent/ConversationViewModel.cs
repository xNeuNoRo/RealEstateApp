namespace RealEstateApp.Application.ViewModels.Agent;

public sealed class ConversationViewModel
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string OtherUserId { get; init; } = null!;
    public string OtherUserName { get; init; } = null!;
    public IReadOnlyList<Chat.MessageViewModel> Messages { get; init; } = [];
    public ReplyMessageViewModel ReplyForm { get; set; } = new();
}
