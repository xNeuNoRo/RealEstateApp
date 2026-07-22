using FluentValidation;
using RealEstateApp.Application.Dtos.Admin.Requests;

namespace RealEstateApp.Application.Validators.Admin;

public sealed class GetDeveloperByIdRequestValidator : AbstractValidator<GetDeveloperByIdRequest>
{
    public GetDeveloperByIdRequestValidator()
    {
        RuleFor(x => x.DeveloperId).NotEmpty().WithMessage("El ID del desarrollador es requerido.");
    }
}
