using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public sealed class PropertyImprovementConfiguration : IEntityTypeConfiguration<PropertyImprovement>
{
    public void Configure(EntityTypeBuilder<PropertyImprovement> builder)
    {
        builder.ToTable("PropertyImprovements");

        builder.HasKey(x => new { x.PropertyId, x.ImprovementId });

        builder
            .HasOne(x => x.Property)
            .WithMany(x => x.Improvements)
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Improvement)
            .WithMany()
            .HasForeignKey(x => x.ImprovementId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ImprovementId);
    }
}
