using EventosVivos.Domain.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class EventStatusConfiguration : IEntityTypeConfiguration<EventStatus>
{
    public void Configure(EntityTypeBuilder<EventStatus> builder)
    {
        builder.ToTable("event_statuses");

        builder.HasKey(eventStatus => eventStatus.Id)
            .HasName("pk_event_statuses");

        builder.Property(eventStatus => eventStatus.Id)
            .HasColumnName("event_status_id")
            .HasConversion(ValueObjectConverters.EventStatusId)
            .ValueGeneratedOnAdd();

        builder.Property(eventStatus => eventStatus.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .HasConversion(ValueObjectConverters.CatalogCode)
            .IsRequired();

        builder.Property(eventStatus => eventStatus.Name)
            .HasColumnName("name")
            .HasMaxLength(60)
            .IsRequired();

        builder.HasIndex(eventStatus => eventStatus.Code)
            .IsUnique()
            .HasDatabaseName("uq_event_statuses_code");

        builder.HasIndex(eventStatus => eventStatus.Name)
            .IsUnique()
            .HasDatabaseName("uq_event_statuses_name");
    }
}
