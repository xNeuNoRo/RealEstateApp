using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("Offers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.PropertyId).IsRequired();

        // FK a Identity
        builder.Property(x => x.ClientId).IsRequired().HasMaxLength(128);

        // FK a Property
        builder
            .HasOne(x => x.Property)
            .WithMany()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.RespondedAt).IsRequired(false);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // 1 oferta aceptada por propiedad
        builder
            .HasIndex(x => x.PropertyId)
            .IsUnique()
            .HasFilter("[Status] = 'Accepted'")
            .HasDatabaseName("IX_Offers_PropertyId_Accepted");

        // 1 oferta Pendiente por (propiedad, cliente)
        builder
            .HasIndex(x => new { x.PropertyId, x.ClientId })
            .IsUnique()
            .HasFilter("[Status] = 'Pending'")
            .HasDatabaseName("IX_Offers_PropertyId_ClientId_Pending");

        builder.HasIndex(x => x.ClientId);
    }
}
