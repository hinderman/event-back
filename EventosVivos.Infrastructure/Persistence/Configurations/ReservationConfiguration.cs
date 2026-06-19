using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainEvent = EventosVivos.Domain.Events.Event;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservations");

        builder.Ignore(reservation => reservation.DomainEvents);
        builder.Ignore(reservation => reservation.TotalAmount);

        builder.HasKey(reservation => reservation.Id)
            .HasName("pk_reservations");

        builder.Property(reservation => reservation.Id)
            .HasColumnName("reservation_id")
            .HasConversion(ValueObjectConverters.ReservationId)
            .ValueGeneratedOnAdd();

        builder.Property(reservation => reservation.EventId)
            .HasColumnName("event_id")
            .HasConversion(ValueObjectConverters.EventId)
            .IsRequired();

        builder.Property(reservation => reservation.BuyerUserId)
            .HasColumnName("buyer_user_id")
            .HasConversion(ValueObjectConverters.UserId)
            .IsRequired();

        builder.Property(reservation => reservation.ReservationStatusId)
            .HasColumnName("reservation_status_id")
            .HasConversion(ValueObjectConverters.ReservationStatusId)
            .IsRequired();

        builder.Property(reservation => reservation.Quantity)
            .HasColumnName("quantity")
            .HasConversion(ValueObjectConverters.TicketQuantity)
            .IsRequired();

        builder.Property(reservation => reservation.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(12, 2)
            .HasConversion(ValueObjectConverters.Money)
            .IsRequired();

        builder.Property(reservation => reservation.ReservationCode)
            .HasColumnName("reservation_code")
            .HasMaxLength(9)
            .HasConversion(ValueObjectConverters.NullableReservationCode);

        builder.Property(reservation => reservation.PaidAmount)
            .HasColumnName("paid_amount")
            .HasPrecision(12, 2)
            .HasConversion(ValueObjectConverters.NullableMoney);

        builder.Property(reservation => reservation.PaymentReference)
            .HasColumnName("payment_reference")
            .HasMaxLength(80);

        builder.Property(reservation => reservation.ConfirmedAt)
            .HasColumnName("confirmed_at");

        builder.Property(reservation => reservation.ConfirmedByUserId)
            .HasColumnName("confirmed_by_user_id")
            .HasConversion(ValueObjectConverters.NullableUserId);

        builder.Property(reservation => reservation.IsPenalizedCancellation)
            .HasColumnName("is_penalized_cancellation")
            .IsRequired();

        builder.Property(reservation => reservation.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(300);

        builder.Property(reservation => reservation.CancelledByUserId)
            .HasColumnName("cancelled_by_user_id")
            .HasConversion(ValueObjectConverters.NullableUserId);

        builder.Property(reservation => reservation.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.Property(reservation => reservation.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(reservation => reservation.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<DomainEvent>()
            .WithMany()
            .HasForeignKey(reservation => reservation.EventId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_reservations_events");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(reservation => reservation.BuyerUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_reservations_buyer_users");

        builder.HasOne<ReservationStatus>()
            .WithMany()
            .HasForeignKey(reservation => reservation.ReservationStatusId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_reservations_reservation_statuses");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(reservation => reservation.CancelledByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_reservations_cancelled_by_users");

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(reservation => reservation.ConfirmedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_reservations_confirmed_by_users");

        builder.HasIndex(reservation => reservation.ReservationCode)
            .IsUnique()
            .HasFilter("[reservation_code] IS NOT NULL")
            .HasDatabaseName("uq_reservations_reservation_code");

        builder.HasIndex(reservation => new { reservation.EventId, reservation.ReservationStatusId })
            .HasDatabaseName("ix_reservations_event_status");

        builder.HasIndex(reservation => reservation.BuyerUserId)
            .HasDatabaseName("ix_reservations_buyer");

        builder.HasIndex(reservation => reservation.ConfirmedByUserId)
            .HasDatabaseName("ix_reservations_confirmed_by");

        builder.HasIndex(reservation => reservation.PaymentReference)
            .IsUnique()
            .HasFilter("[payment_reference] IS NOT NULL")
            .HasDatabaseName("uq_reservations_payment_reference");
    }
}
