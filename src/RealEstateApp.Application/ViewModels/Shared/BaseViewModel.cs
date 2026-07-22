namespace RealEstateApp.Application.ViewModels.Shared;

public abstract class BaseViewModel
{
    public string? CurrentUserId { get; set; }
    public string? CurrentUserFullName { get; set; }
    public string? CurrentUserName { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    public bool IsAuthenticated { get; set; }
    public int UnreadNotificationsCount { get; set; }
    public int PendingMessagesCount { get; set; }
    public string PageTitle { get; set; } = string.Empty;
}
