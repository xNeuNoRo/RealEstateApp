namespace RealEstateApp.Application.ViewModels.Auth;

public sealed class ActivateAccountViewModel
{
    public string UserId { get; init; } = null!;
    public string Token { get; init; } = null!;
    public string? Message { get; init; }
    public bool Success { get; init; }
}
