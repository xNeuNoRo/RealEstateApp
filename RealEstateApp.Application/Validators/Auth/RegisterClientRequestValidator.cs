using FluentValidation;
using RealEstateApp.Application.Dtos.Auth.Requests;
using RealEstateApp.Domain.Settings;
using RealEstateApp.Domain.ValueObjects;

namespace RealEstateApp.Application.Validators.Auth;

public sealed class RegisterClientRequestValidator : AbstractValidator<RegisterClientRequest>
{
    private static readonly HashSet<string> AllowedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public RegisterClientRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("El nombre es requerido.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("El apellido es requerido.")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("El teléfono es requerido.")
            .Must(phone => PhoneNumber.Create(phone!).IsSuccess)
            .WithMessage("Debe ingresar un número telefónico válido.")
            .Matches(@"^(809|829|849)-\d{3}-\d{4}$")
            .WithMessage(
                "Debe ingresar un número telefónico de República Dominicana (ej. 809-555-1234)."
            );

        RuleFor(x => x.PhotoFile)
            .NotNull()
            .WithMessage("La foto de usuario es requerida.")
            .Must(f => f == null || f.Length > 0)
            .WithMessage("La foto de usuario no puede estar vacía.")
            .Must(f => f == null || f.Length <= FileConstants.MaxImageFileSizeBytes)
            .WithMessage(
                $"La imagen no debe exceder los {FileConstants.MaxImageFileSizeBytes / (1024 * 1024)} MB."
            )
            .Must(f => f == null || AllowedMimeTypes.Contains(f.ContentType.ToLowerInvariant()))
            .WithMessage("Solo se permiten imágenes en formato JPEG, PNG o WebP.")
            .Must(f =>
                f == null
                || AllowedExtensions.Contains(Path.GetExtension(f.FileName).ToLowerInvariant())
            )
            .WithMessage(
                "La extensión del archivo no está permitida (use .jpg, .jpeg, .png o .webp)."
            );

        RuleFor(x => x.UserName).NotEmpty().WithMessage("El nombre de usuario es requerido.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es requerido.")
            .Must(email => Email.Create(email!).IsSuccess)
            .WithMessage("Debe ingresar un correo electrónico válido.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es requerida.")
            .MinimumLength(8)
            .WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$")
            .WithMessage("La contraseña debe incluir mayúscula, minúscula, número y símbolo.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("La confirmación de contraseña es requerida.")
            .Equal(x => x.Password)
            .WithMessage("La contraseña y la confirmación de contraseña no coinciden.");

        RuleFor(x => x.Origin)
            .Must(IsHttpOrigin)
            .WithMessage("El origen de la aplicación no es válido.");
    }

    private static bool IsHttpOrigin(string origin) =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
