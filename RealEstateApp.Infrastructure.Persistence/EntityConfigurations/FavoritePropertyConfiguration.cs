using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public sealed class FavoritePropertyConfiguration : IEntityTypeConfiguration<FavoriteProperty>
{
    public void Configure(EntityTypeBuilder<FavoriteProperty> builder)
    {
        builder.ToTable("FavoriteProperties");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // FK lógica a Identity
        builder.Property(x => x.ClientId).IsRequired().HasMaxLength(128);

        // FK a Property
        builder.Property(x => x.PropertyId).IsRequired();

        builder
            .HasOne(x => x.Property)
            .WithMany()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // Un cliente solo puede favoritar una propiedad una vez
        builder.HasIndex(x => new { x.ClientId, x.PropertyId }).IsUnique();
        builder.HasIndex(x => x.PropertyId);
    }
}
