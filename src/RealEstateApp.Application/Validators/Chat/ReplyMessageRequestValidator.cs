using FluentValidation;
using RealEstateApp.Application.Dtos.Chat.Requests;

namespace RealEstateApp.Application.Validators.Chat;

public sealed class ReplyMessageRequestValidator : AbstractValidator<ReplyMessageRequest>
{
    public ReplyMessageRequestValidator()
    {
        RuleFor(x => x.MessageId).GreaterThan(0).WithMessage("El ID de mensaje es requerido.");
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("El mensaje no puede estar vacío.")
            .MaximumLength(2000)
            .WithMessage("El mensaje no debe exceder 2000 caracteres.");
    }
}
