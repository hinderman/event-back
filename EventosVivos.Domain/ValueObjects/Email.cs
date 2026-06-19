using System.Text.RegularExpressions;
using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly partial record struct Email
{
    public Email(string value)
    {
        value = Guard.AgainstBlank(value, nameof(value)).ToLowerInvariant();
        value = Guard.AgainstLength(value, 254, nameof(value));

        if (!EmailExpression().IsMatch(value))
        {
            throw new DomainException("El correo electronico no tiene un formato valido.");
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }

    [GeneratedRegex("^[A-Z0-9._%+\\-]+@[A-Z0-9.\\-]+\\.[A-Z]{2,}$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailExpression();
}
