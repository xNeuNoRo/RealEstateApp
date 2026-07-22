namespace RealEstateApp.Application.ViewModels.Chat;

using RealEstateApp.Domain.Common;
using RealEstateApp.Application.ViewModels.Shared;

public sealed class ChatListViewModel : BaseViewModel
{
    public PagedResult<ConversationSummaryViewModel> Conversations { get; init; } =
        new([], 0, 1, 20);
}
