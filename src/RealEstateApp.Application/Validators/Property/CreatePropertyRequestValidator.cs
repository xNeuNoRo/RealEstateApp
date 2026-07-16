using FluentValidation;
using RealEstateApp.Application.Dtos.Property.Requests;
using RealEstateApp.Domain.Settings;

namespace RealEstateApp.Application.Validators.Property;

public sealed class CreatePropertyRequestValidator : AbstractValidator<CreatePropertyRequest>
{
    private static readonly HashSet<string> AllowedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public CreatePropertyRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("La descripción es requerida.")
            .MinimumLength(5)
            .WithMessage("La descripción debe tener al menos 5 caracteres.")
            .MaximumLength(2000)
            .WithMessage("La descripción no debe exceder 2000 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("El precio debe ser mayor que cero.")
            .LessThanOrEqualTo(100_000_000)
            .WithMessage("El precio no debe exceder 100 millones.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La moneda es requerida.")
            .Length(3)
            .WithMessage("La moneda debe tener 3 caracteres (ej. DOP, USD).");

        RuleFor(x => x.SizeM2)
            .GreaterThan(0)
            .WithMessage("El tamaño debe ser mayor que cero.")
            .LessThanOrEqualTo(10000)
            .WithMessage("El tamaño no debe exceder 10,000 m².");

        RuleFor(x => x.Bedrooms)
            .InclusiveBetween(0, 20)
            .WithMessage("Las habitaciones deben estar entre 0 y 20.");

        RuleFor(x => x.Bathrooms)
            .InclusiveBetween(0, 20)
            .WithMessage("Los baños deben estar entre 0 y 20.");

        RuleFor(x => x.PropertyTypeId)
            .GreaterThan(0)
            .WithMessage("El tipo de propiedad es requerido.");

        RuleFor(x => x.SaleTypeId).GreaterThan(0).WithMessage("El tipo de venta es requerido.");

        RuleFor(x => x.ImageFiles)
            .NotEmpty()
            .WithMessage("Debe incluir al menos una imagen.")
            .Must(f => f.Count <= 4)
            .WithMessage("No puede incluir más de 4 imágenes.");

        RuleForEach(x => x.ImageFiles)
            .ChildRules(file =>
            {
                file.RuleFor(f => f.Length)
                    .GreaterThan(0)
                    .WithMessage("La imagen no puede estar vacía.")
                    .LessThanOrEqualTo(FileConstants.MaxImageFileSizeBytes)
                    .WithMessage(
                        $"La imagen no debe exceder {FileConstants.MaxImageFileSizeBytes / (1024 * 1024)} MB."
                    );

                file.RuleFor(f => f.ContentType)
                    .Must(ct => AllowedMimeTypes.Contains(ct.ToLowerInvariant()))
                    .WithMessage("Solo se permiten imágenes JPEG, PNG o WebP.");

                file.RuleFor(f => Path.GetExtension(f.FileName).ToLowerInvariant())
                    .Must(ext => AllowedExtensions.Contains(ext))
                    .WithMessage("La extensión del archivo no está permitida.");
            });

        RuleFor(x => x.ImprovementIds)
            .NotEmpty()
            .WithMessage("Debe incluir al menos una mejora.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("No puede incluir mejoras duplicadas.");
    }
}
