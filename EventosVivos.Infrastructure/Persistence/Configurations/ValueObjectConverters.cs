using EventosVivos.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EventosVivos.Infrastructure.Persistence.Configurations;

internal static class ValueObjectConverters
{
    public static readonly ValueConverter<UserTypeId, short> UserTypeId = new(
        id => id.Value,
        value => new UserTypeId(value));

    public static readonly ValueConverter<UserId, long> UserId = new(
        id => id.Value,
        value => new UserId(ToGeneratedEntityId(value)));

    public static readonly ValueConverter<UserId?, long?> NullableUserId = new(
        id => id.HasValue ? id.Value.Value : null,
        value => value.HasValue ? new UserId(value.Value) : null);

    public static readonly ValueConverter<EventTypeId, short> EventTypeId = new(
        id => id.Value,
        value => new EventTypeId(value));

    public static readonly ValueConverter<EventStatusId, short> EventStatusId = new(
        id => id.Value,
        value => new EventStatusId(value));

    public static readonly ValueConverter<ReservationStatusId, short> ReservationStatusId = new(
        id => id.Value,
        value => new ReservationStatusId(value));

    public static readonly ValueConverter<VenueId, int> VenueId = new(
        id => id.Value,
        value => new VenueId(value));

    public static readonly ValueConverter<EventId, long> EventId = new(
        id => id.Value,
        value => new EventId(ToGeneratedEntityId(value)));

    public static readonly ValueConverter<ReservationId, long> ReservationId = new(
        id => id.Value,
        value => new ReservationId(ToGeneratedEntityId(value)));

    public static readonly ValueConverter<CatalogCode, string> CatalogCode = new(
        code => code.Value,
        value => new CatalogCode(value));

    public static readonly ValueConverter<Email, string> Email = new(
        email => email.Value,
        value => new Email(value));

    public static readonly ValueConverter<PasswordHash, string> PasswordHash = new(
        hash => hash.Value,
        value => new PasswordHash(value));

    public static readonly ValueConverter<Capacity, int> Capacity = new(
        capacity => capacity.Value,
        value => new Capacity(value));

    public static readonly ValueConverter<TicketQuantity, int> TicketQuantity = new(
        quantity => quantity.Value,
        value => new TicketQuantity(value));

    public static readonly ValueConverter<Money, decimal> Money = new(
        money => money.Amount,
        value => new Money(value));

    public static readonly ValueConverter<Money?, decimal?> NullableMoney = new(
        money => money.HasValue ? money.Value.Amount : null,
        value => value.HasValue ? new Money(value.Value) : null);

    public static readonly ValueConverter<ReservationCode?, string?> NullableReservationCode = new(
        code => code.HasValue ? code.Value.Value : null,
        value => value == null ? null : new ReservationCode(value));

    private static long ToGeneratedEntityId(long value)
    {
        return value < 0 ? Math.Abs(value) : value;
    }
}
