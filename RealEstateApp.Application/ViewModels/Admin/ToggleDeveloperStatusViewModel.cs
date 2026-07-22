using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Admin;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class ToggleDeveloperStatusViewModel : BaseViewModel
{
    [Required]
    public string DeveloperId { get; set; } = null!;

    [Required]
    public bool NewStatus { get; set; }

    public string DeveloperName { get; init; } = null!;
}
