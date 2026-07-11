using FluentValidation;
using RealEstateApp.Application.Dtos.Chat.Requests;

namespace RealEstateApp.Application.Validators.Chat;

public sealed class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0).WithMessage("El ID de propiedad es requerido.");
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("El mensaje no puede estar vacío.")
            .MaximumLength(2000)
            .WithMessage("El mensaje no debe exceder 2000 caracteres.");
    }
}
