namespace RealEstateApp.Application.ViewModels.Agent;

public sealed class ChatListViewModel
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public IReadOnlyList<Chat.ConversationSummaryViewModel> Conversations { get; init; } = [];
}
