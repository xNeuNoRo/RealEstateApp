namespace RealEstateApp.Application.ViewModels.Agent;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class ChatListViewModel : BaseViewModel
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public IReadOnlyList<Chat.ConversationSummaryViewModel> Conversations { get; init; } = [];
}
