using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.EntityConfigurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.PropertyId).IsRequired();

        // FK lógicas a Identity
        builder.Property(x => x.ClientId).IsRequired().HasMaxLength(128);
        builder.Property(x => x.AgentId).IsRequired().HasMaxLength(128);

        builder.Property(x => x.SenderType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(2000);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // FK a Property
        builder
            .HasOne(x => x.Property)
            .WithMany()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indices para consultas de conversación
        builder.HasIndex(x => x.PropertyId);
        builder
            .HasIndex(x => new
            {
                x.PropertyId,
                x.ClientId,
                x.AgentId,
                x.CreatedAt,
            })
            .HasDatabaseName("IX_Messages_Conversation");
    }
}
