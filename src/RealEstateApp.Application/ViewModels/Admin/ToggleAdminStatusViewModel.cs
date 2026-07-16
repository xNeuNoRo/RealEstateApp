using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class ToggleAdminStatusViewModel
{
    [Required]
    public string AdminId { get; set; } = null!;

    [Required]
    public bool NewStatus { get; set; }

    public string AdminName { get; init; } = null!;
}
