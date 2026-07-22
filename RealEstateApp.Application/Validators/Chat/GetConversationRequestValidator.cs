using FluentValidation;
using RealEstateApp.Application.Dtos.Chat.Requests;

namespace RealEstateApp.Application.Validators.Chat;

public sealed class GetConversationRequestValidator : AbstractValidator<GetConversationRequest>
{
    public GetConversationRequestValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0).WithMessage("El ID de propiedad es requerido.");
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
