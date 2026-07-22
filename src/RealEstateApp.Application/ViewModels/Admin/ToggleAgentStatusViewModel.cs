using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Admin;

using RealEstateApp.Application.ViewModels.Shared;

public sealed class ToggleAgentStatusViewModel : BaseViewModel
{
    [Required]
    public string AgentId { get; set; } = null!;

    [Required]
    public bool NewStatus { get; set; }

    public string AgentName { get; init; } = null!;
}
