using FluentValidation;
using RealEstateApp.Application.Dtos.Chat.Requests;

namespace RealEstateApp.Application.Validators.Chat;

public sealed class GetMyConversationsRequestValidator
    : AbstractValidator<GetMyConversationsRequest>
{
    public GetMyConversationsRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
