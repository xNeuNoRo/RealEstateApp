namespace RealEstateApp.Application.ViewModels.Shared;

public sealed class ConfirmDialogViewModel
{
    public string Title { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string ConfirmText { get; init; } = "Aceptar";
    public string CancelText { get; init; } = "Cancelar";
    public string? ReturnUrl { get; init; }
    public string? ActionName { get; init; }
    public string? ControllerName { get; init; }
}
