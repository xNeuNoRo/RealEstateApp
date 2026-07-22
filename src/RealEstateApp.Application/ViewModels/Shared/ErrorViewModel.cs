namespace RealEstateApp.Application.ViewModels.Shared;

public sealed class ErrorViewModel : BaseViewModel
{
    public string? RequestId { get; init; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string? Message { get; init; }
    public int StatusCode { get; init; }
}
