using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Tests.ValueObjects;

public sealed class ValueObjectTests
{
    [Theory]
    [InlineData("USER@Example.COM", "user@example.com")]
    [InlineData("buyer.name+tag@eventos.com", "buyer.name+tag@eventos.com")]
    public void Email_NormalizesValidValues(string input, string expected)
    {
        var email = new Email(input);

        Assert.Equal(expected, email.Value);
        Assert.Equal(expected, email.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("user@")]
    public void Email_RejectsInvalidValues(string input)
    {
        Assert.Throws<DomainException>(() => new Email(input));
    }

    [Theory]
    [InlineData("Activo", "activo")]
    [InlineData("pendiente_pago", "pendiente_pago")]
    public void CatalogCode_NormalizesValidValues(string input, string expected)
    {
        var code = new CatalogCode(input);

        Assert.Equal(expected, code.Value);
    }

    [Theory]
    [InlineData("_invalid")]
    [InlineData("invalid-code")]
    [InlineData("1invalid")]
    public void CatalogCode_RejectsInvalidValues(string input)
    {
        Assert.Throws<DomainException>(() => new CatalogCode(input));
    }

    [Fact]
    public void ReservationCode_FromSequence_FormatsWithPrefixAndSixDigits()
    {
        var code = ReservationCode.FromSequence(42);

        Assert.Equal("EV-000042", code.Value);
    }

    [Theory]
    [InlineData("ev-000001", "EV-000001")]
    [InlineData("EV-999999", "EV-999999")]
    public void ReservationCode_NormalizesValidValues(string input, string expected)
    {
        var code = new ReservationCode(input);

        Assert.Equal(expected, code.Value);
    }

    [Theory]
    [InlineData("EV-1")]
    [InlineData("XX-000001")]
    [InlineData("EV-ABCDEF")]
    public void ReservationCode_RejectsInvalidValues(string input)
    {
        Assert.Throws<DomainException>(() => new ReservationCode(input));
    }

    [Theory]
    [InlineData(80, 3, 240)]
    [InlineData(99.99, 2, 199.98)]
    public void Money_Multiply_ReturnsExpectedAmount(decimal amount, int multiplier, decimal expected)
    {
        var result = new Money(amount).Multiply(multiplier);

        Assert.Equal(expected, result.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.999)]
    public void Money_RejectsInvalidAmounts(decimal amount)
    {
        Assert.Throws<DomainException>(() => new Money(amount));
    }

    [Fact]
    public void EventSchedule_Overlaps_ReturnsTrueForIntersectingRanges()
    {
        var schedule = new EventSchedule(
            new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero));
        var other = new EventSchedule(
            new DateTimeOffset(2026, 6, 20, 11, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 20, 13, 0, 0, TimeSpan.Zero));

        Assert.True(schedule.Overlaps(other));
    }

    [Fact]
    public void EventSchedule_Overlaps_ReturnsFalseForTouchingRanges()
    {
        var schedule = new EventSchedule(
            new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero));
        var other = new EventSchedule(
            new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 20, 13, 0, 0, TimeSpan.Zero));

        Assert.False(schedule.Overlaps(other));
    }
}
