using FluentValidation;
using RealEstateApp.Application.Dtos.Favorites.Requests;

namespace RealEstateApp.Application.Validators.Favorites;

public sealed class AddFavoriteRequestValidator : AbstractValidator<AddFavoriteRequest>
{
    public AddFavoriteRequestValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0).WithMessage("El ID de propiedad es requerido.");
    }
}
