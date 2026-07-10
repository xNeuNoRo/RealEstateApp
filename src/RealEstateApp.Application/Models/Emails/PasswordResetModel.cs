namespace RealEstateApp.Application.Models.Emails;

/// <summary>
/// Modelo para el correo de restablecimiento de contraseña.
/// </summary>
public sealed record PasswordResetModel(string FullName, string ResetLink) : IEmailModel;
