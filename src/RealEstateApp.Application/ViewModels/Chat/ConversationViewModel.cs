namespace RealEstateApp.Application.ViewModels.Chat;

using RealEstateApp.Domain.Common;
using RealEstateApp.Application.ViewModels.Shared;

public sealed class ConversationViewModel : BaseViewModel
{
    public int PropertyId { get; init; }
    public string PropertyCode { get; init; } = null!;
    public string PropertyTitle { get; init; } = null!;
    public string PropertyDescription { get; init; } = null!;
    public string OtherUserId { get; init; } = null!;
    public string OtherUserName { get; init; } = null!;
    public string OtherUserRole { get; init; } = null!;
    public PagedResult<MessageViewModel> Messages { get; init; } = new([], 0, 1, 20);
    public int? ReplyToMessageId { get; init; }
}
