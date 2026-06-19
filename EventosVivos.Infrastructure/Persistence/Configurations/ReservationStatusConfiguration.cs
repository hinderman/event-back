using EventosVivos.Domain.Catalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class ReservationStatusConfiguration : IEntityTypeConfiguration<ReservationStatus>
{
    public void Configure(EntityTypeBuilder<ReservationStatus> builder)
    {
        builder.ToTable("reservation_statuses");

        builder.HasKey(reservationStatus => reservationStatus.Id)
            .HasName("pk_reservation_statuses");

        builder.Property(reservationStatus => reservationStatus.Id)
            .HasColumnName("reservation_status_id")
            .HasConversion(ValueObjectConverters.ReservationStatusId)
            .ValueGeneratedOnAdd();

        builder.Property(reservationStatus => reservationStatus.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .HasConversion(ValueObjectConverters.CatalogCode)
            .IsRequired();

        builder.Property(reservationStatus => reservationStatus.Name)
            .HasColumnName("name")
            .HasMaxLength(60)
            .IsRequired();

        builder.HasIndex(reservationStatus => reservationStatus.Code)
            .IsUnique()
            .HasDatabaseName("uq_reservation_statuses_code");

        builder.HasIndex(reservationStatus => reservationStatus.Name)
            .IsUnique()
            .HasDatabaseName("uq_reservation_statuses_name");
    }
}
