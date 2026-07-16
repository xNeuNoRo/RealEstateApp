using FluentValidation;
using RealEstateApp.Application.Dtos.Client.Requests;

namespace RealEstateApp.Application.Validators.Client;

public sealed class UpdateClientProfileRequestValidator
    : AbstractValidator<UpdateClientProfileRequest>
{
    public UpdateClientProfileRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
    }
}
