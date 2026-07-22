using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Admin;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class ToggleAdminStatusViewModel : BaseViewModel
{
    [Required]
    public string AdminId { get; set; } = null!;

    [Required]
    public bool NewStatus { get; set; }

    public string AdminName { get; init; } = null!;
}
