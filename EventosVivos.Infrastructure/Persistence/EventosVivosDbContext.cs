using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;
using DomainEvent = EventosVivos.Domain.Events.Event;

namespace EventosVivos.Infrastructure.Persistence;

public sealed class EventosVivosDbContext(DbContextOptions<EventosVivosDbContext> options)
    : DbContext(options)
{
    public const string Schema = "eventos_vivos";

    public DbSet<UserType> UserTypes => Set<UserType>();

    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public DbSet<EventType> EventTypes => Set<EventType>();

    public DbSet<EventStatus> EventStatuses => Set<EventStatus>();

    public DbSet<ReservationStatus> ReservationStatuses => Set<ReservationStatus>();

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<DomainEvent> Events => Set<DomainEvent>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventosVivosDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
