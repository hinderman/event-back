using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("venues");

        builder.Ignore(venue => venue.DomainEvents);

        builder.HasKey(venue => venue.Id)
            .HasName("pk_venues");

        builder.Property(venue => venue.Id)
            .HasColumnName("venue_id")
            .HasConversion(ValueObjectConverters.VenueId)
            .ValueGeneratedOnAdd();

        builder.Property(venue => venue.Name)
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(venue => venue.City)
            .HasColumnName("city")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(venue => venue.Capacity)
            .HasColumnName("capacity")
            .HasConversion(ValueObjectConverters.Capacity)
            .IsRequired();

        builder.Property(venue => venue.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(venue => venue.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(venue => venue.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(venue => new { venue.City, venue.Name })
            .IsUnique()
            .HasDatabaseName("uq_venues_city_name");
    }
}
