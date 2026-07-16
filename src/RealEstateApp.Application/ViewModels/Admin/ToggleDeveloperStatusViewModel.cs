using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class ToggleDeveloperStatusViewModel
{
    [Required]
    public string DeveloperId { get; set; } = null!;

    [Required]
    public bool NewStatus { get; set; }

    public string DeveloperName { get; init; } = null!;
}
