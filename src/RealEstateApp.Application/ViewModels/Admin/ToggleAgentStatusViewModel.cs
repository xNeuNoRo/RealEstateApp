using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Admin;

public sealed class ToggleAgentStatusViewModel
{
    [Required]
    public string AgentId { get; set; } = null!;

    [Required]
    public bool NewStatus { get; set; }

    public string AgentName { get; init; } = null!;
}
