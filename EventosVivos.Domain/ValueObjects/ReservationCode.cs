using System.Text.RegularExpressions;
using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly partial record struct ReservationCode
{
    public const string Prefix = "EV";

    public ReservationCode(string value)
    {
        value = Guard.AgainstBlank(value, nameof(value)).ToUpperInvariant();

        if (!ReservationCodeExpression().IsMatch(value))
        {
            throw new DomainException("El codigo de reserva debe tener el formato EV-000001.");
        }

        Value = value;
    }

    public string Value { get; }

    public static ReservationCode FromSequence(long sequence)
    {
        Guard.AgainstNonPositive(sequence, nameof(sequence));
        return new ReservationCode($"{Prefix}-{sequence:000000}");
    }

    public override string ToString()
    {
        return Value;
    }

    [GeneratedRegex("^EV-[0-9]{6}$")]
    private static partial Regex ReservationCodeExpression();
}
