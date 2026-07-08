using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Contexts;

/// <summary>
/// Contexto de persistencia para ASP.NET Core Identity.
/// Usa el schema "Identity" para separar las tablas de Identity del schema "dbo" de AppDbContext.
/// </summary>
public class IdentityContext : IdentityDbContext<AppUser>
{
    public IdentityContext(DbContextOptions<IdentityContext> options)
        : base(options) { }

    // Configuración adicional de Identity
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("Identity");

        builder.Entity<AppUser>(e =>
        {
            e.ToTable("Users");
            e.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            e.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            e.Property(u => u.Phone).HasMaxLength(20);
            e.Property(u => u.ProfileImage).HasMaxLength(500);
            e.Property(u => u.IdentityDocument).HasMaxLength(11);
            e.Property(u => u.Active).IsRequired().HasDefaultValue(true);

            // Indice único para IdentityDocument, pero permite nulos
            // (no todos los usuarios tienen que tenerlo)
            e.HasIndex(u => u.IdentityDocument)
                .IsUnique()
                .HasFilter("[IdentityDocument] IS NOT NULL");
        });

        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
    }
}
