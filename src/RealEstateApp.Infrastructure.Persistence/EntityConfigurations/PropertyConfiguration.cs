using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.EntityConfigurations.ValueConverters;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder
            .Property(x => x.Code)
            .HasConversion<PropertyCodeConverter>()
            .HasMaxLength(6)
            .IsRequired();

        builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);

        builder.OwnsOne(
            x => x.Price,
            price =>
            {
                price
                    .Property(p => p.Amount)
                    .HasColumnName("PriceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                price
                    .Property(p => p.Currency)
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            x => x.Size,
            size =>
            {
                size.Property(s => s.Area)
                    .HasColumnName("SizeArea")
                    .HasPrecision(18, 2)
                    .IsRequired();
                size.Property(s => s.Unit).HasColumnName("SizeUnit").HasMaxLength(10).IsRequired();
            }
        );

        builder.Property(x => x.Bedrooms).IsRequired();
        builder.Property(x => x.Bathrooms).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        // FKs a catalogos
        builder.Property(x => x.PropertyTypeId).IsRequired();

        builder
            .HasOne(x => x.PropertyType)
            .WithMany()
            .HasForeignKey(x => x.PropertyTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.SaleTypeId).IsRequired();

        builder
            .HasOne(x => x.SaleType)
            .WithMany()
            .HasForeignKey(x => x.SaleTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK logica a Identity
        builder.Property(x => x.AgentId).IsRequired().HasMaxLength(128);

        // Auditoria
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // Indices
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.AgentId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Status, x.PropertyTypeId });
        builder.HasIndex("PriceAmount").HasDatabaseName("IX_Properties_PriceAmount");
        builder.HasIndex("SizeArea").HasDatabaseName("IX_Properties_SizeArea");
    }
}
