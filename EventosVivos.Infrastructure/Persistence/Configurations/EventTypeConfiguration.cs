using EventosVivos.Domain.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class EventTypeConfiguration : IEntityTypeConfiguration<EventType>
{
    public void Configure(EntityTypeBuilder<EventType> builder)
    {
        builder.ToTable("event_types");

        builder.HasKey(eventType => eventType.Id)
            .HasName("pk_event_types");

        builder.Property(eventType => eventType.Id)
            .HasColumnName("event_type_id")
            .HasConversion(ValueObjectConverters.EventTypeId)
            .ValueGeneratedOnAdd();

        builder.Property(eventType => eventType.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .HasConversion(ValueObjectConverters.CatalogCode)
            .IsRequired();

        builder.Property(eventType => eventType.Name)
            .HasColumnName("name")
            .HasMaxLength(60)
            .IsRequired();

        builder.HasIndex(eventType => eventType.Code)
            .IsUnique()
            .HasDatabaseName("uq_event_types_code");

        builder.HasIndex(eventType => eventType.Name)
            .IsUnique()
            .HasDatabaseName("uq_event_types_name");
    }
}
