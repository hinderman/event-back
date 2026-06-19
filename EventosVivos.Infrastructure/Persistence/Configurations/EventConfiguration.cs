using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainEvent = EventosVivos.Domain.Events.Event;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class EventConfiguration : IEntityTypeConfiguration<DomainEvent>
{
    public void Configure(EntityTypeBuilder<DomainEvent> builder)
    {
        builder.ToTable("events");

        builder.Ignore(@event => @event.DomainEvents);

        builder.HasKey(@event => @event.Id)
            .HasName("pk_events");

        builder.Property(@event => @event.Id)
            .HasColumnName("event_id")
            .HasConversion(ValueObjectConverters.EventId)
            .ValueGeneratedOnAdd();

        builder.Property(@event => @event.VenueId)
            .HasColumnName("venue_id")
            .HasConversion(ValueObjectConverters.VenueId)
            .IsRequired();

        builder.Property(@event => @event.EventTypeId)
            .HasColumnName("event_type_id")
            .HasConversion(ValueObjectConverters.EventTypeId)
            .IsRequired();

        builder.Property(@event => @event.EventStatusId)
            .HasColumnName("event_status_id")
            .HasConversion(ValueObjectConverters.EventStatusId)
            .IsRequired();

        builder.Property(@event => @event.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .HasConversion(ValueObjectConverters.UserId)
            .IsRequired();

        builder.ComplexProperty(@event => @event.Details, details =>
        {
            details.Property(value => value.Title)
                .HasColumnName("title")
                .HasMaxLength(100)
                .UseCollation("Latin1_General_100_CI_AI_SC")
                .IsRequired();

            details.Property(value => value.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.Property(@event => @event.MaxCapacity)
            .HasColumnName("max_capacity")
            .HasConversion(ValueObjectConverters.Capacity)
            .IsRequired();

        builder.ComplexProperty(@event => @event.Schedule, schedule =>
        {
            schedule.Property(value => value.StartsAt)
                .HasColumnName("starts_at")
                .IsRequired();

            schedule.Property(value => value.EndsAt)
                .HasColumnName("ends_at")
                .IsRequired();
        });

        builder.Property(@event => @event.Price)
            .HasColumnName("price")
            .HasPrecision(12, 2)
            .HasConversion(ValueObjectConverters.Money)
            .IsRequired();

        builder.Property(@event => @event.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(@event => @event.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<Venue>()
            .WithMany()
            .HasForeignKey(@event => @event.VenueId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_events_venues");

        builder.HasOne<EventType>()
            .WithMany()
            .HasForeignKey(@event => @event.EventTypeId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_events_event_types");

        builder.HasOne<EventStatus>()
            .WithMany()
            .HasForeignKey(@event => @event.EventStatusId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_events_event_statuses");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(@event => @event.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_events_created_by_users");

        builder.HasIndex(@event => @event.CreatedByUserId)
            .HasDatabaseName("ix_events_created_by");
    }
}
