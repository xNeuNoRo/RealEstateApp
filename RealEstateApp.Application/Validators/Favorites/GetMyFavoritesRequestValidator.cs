using FluentValidation;
using RealEstateApp.Application.Dtos.Favorites.Requests;

namespace RealEstateApp.Application.Validators.Favorites;

public sealed class GetMyFavoritesRequestValidator : AbstractValidator<GetMyFavoritesRequest>
{
    public GetMyFavoritesRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
