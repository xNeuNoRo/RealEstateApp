using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Application.ViewModels.Agent;

public sealed class ReplyMessageViewModel
{
    [Required(ErrorMessage = "Debe escribir un mensaje antes de enviarlo.")]
    [Display(Name = "Mensaje")]
    public string Message { get; set; } = null!;
}
