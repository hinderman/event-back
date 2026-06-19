using System.Text.RegularExpressions;
using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly partial record struct CatalogCode
{
    public CatalogCode(string value)
    {
        value = Guard.AgainstBlank(value, nameof(value)).ToLowerInvariant();
        value = Guard.AgainstLength(value, 30, nameof(value));

        if (!CatalogCodeExpression().IsMatch(value))
        {
            throw new DomainException("El codigo de catalogo debe iniciar con letra minuscula y solo puede contener letras, numeros y guion bajo.");
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString()
    {
        return Value;
    }

    [GeneratedRegex("^[a-z][a-z0-9_]*$")]
    private static partial Regex CatalogCodeExpression();
}
