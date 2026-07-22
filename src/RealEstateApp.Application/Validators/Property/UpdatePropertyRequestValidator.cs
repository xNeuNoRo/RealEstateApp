using FluentValidation;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Domain.Settings;

namespace RealEstateApp.Application.Validators.Property;

public sealed class UpdatePropertyRequestValidator : AbstractValidator<UpdatePropertyRequest>
{
    private static readonly HashSet<string> AllowedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public UpdatePropertyRequestValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0).WithMessage("El ID de propiedad es requerido.");

        RuleFor(x => x)
            .Must(HasAtLeastOneField)
            .WithMessage("Debe especificar al menos un campo para actualizar.");

        When(
            x => x.Title is not null,
            () =>
            {
                RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
            }
        );

        When(
            x => x.Description is not null,
            () =>
            {
                RuleFor(x => x.Description).NotEmpty().MinimumLength(5).MaximumLength(2000);
            }
        );

        When(
            x => x.Price.HasValue,
            () =>
            {
                RuleFor(x => x.Price).GreaterThan(0).LessThanOrEqualTo(100_000_000);
            }
        );

        When(
            x => x.Currency is not null,
            () =>
            {
                RuleFor(x => x.Currency).NotEmpty().Length(3);
            }
        );

        When(
            x => x.SizeM2.HasValue,
            () =>
            {
                RuleFor(x => x.SizeM2).GreaterThan(0).LessThanOrEqualTo(10000);
            }
        );

        When(x => x.Bedrooms.HasValue, () => RuleFor(x => x.Bedrooms).InclusiveBetween(0, 20));

        When(x => x.Bathrooms.HasValue, () => RuleFor(x => x.Bathrooms).InclusiveBetween(0, 20));

        When(x => x.PropertyTypeId.HasValue, () => RuleFor(x => x.PropertyTypeId).GreaterThan(0));

        When(x => x.SaleTypeId.HasValue, () => RuleFor(x => x.SaleTypeId).GreaterThan(0));

        When(
            x => x.NewImageFiles is { Count: > 0 },
            () =>
            {
                RuleFor(x => x.NewImageFiles!.Count)
                    .LessThanOrEqualTo(4)
                    .WithMessage("No puede incluir más de 4 imágenes.");

                RuleForEach(x => x.NewImageFiles)
                    .ChildRules(file =>
                    {
                        file.RuleFor(f => f.Length)
                            .GreaterThan(0)
                            .LessThanOrEqualTo(FileConstants.MaxImageFileSizeBytes);

                        file.RuleFor(f => f.ContentType)
                            .Must(ct => AllowedMimeTypes.Contains(ct.ToLowerInvariant()));

                        file.RuleFor(f => Path.GetExtension(f.FileName).ToLowerInvariant())
                            .Must(ext => AllowedExtensions.Contains(ext));
                    });
            }
        );

        When(
            x => x.ImprovementIdsToAdd is { Count: > 0 },
            () =>
            {
                RuleFor(x => x.ImprovementIdsToAdd)
                    .Must(ids => ids is { Count: > 0 } && ids.Distinct().Count() == ids.Count)
                    .WithMessage("No puede incluir mejoras duplicadas.");
            }
        );
    }

    private static bool HasAtLeastOneField(UpdatePropertyRequest req) =>
        req.Title is not null
        || req.Description is not null
        || req.Price.HasValue
        || req.Currency is not null
        || req.SizeM2.HasValue
        || req.Bedrooms.HasValue
        || req.Bathrooms.HasValue
        || req.PropertyTypeId.HasValue
        || req.SaleTypeId.HasValue
        || req.NewImageFiles is { Count: > 0 }
        || req.ImageIdsToRemove is { Count: > 0 }
        || req.ImprovementIdsToAdd is { Count: > 0 }
        || req.ImprovementIdsToRemove is { Count: > 0 };
}
