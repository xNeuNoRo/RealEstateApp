using FluentValidation;
using RealEstateApp.Application.Dtos.Property.Requests;

namespace RealEstateApp.Application.Validators.Property;

public sealed class DeletePropertyRequestValidator : AbstractValidator<DeletePropertyRequest>
{
    public DeletePropertyRequestValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0).WithMessage("El ID de propiedad es requerido.");
    }
}
