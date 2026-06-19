namespace EventosVivos.Domain.Primitives;

internal static class Guard
{
    public static string AgainstBlank(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} no puede estar vacio.");
        }

        return value.Trim();
    }

    public static string AgainstLength(string value, int maxLength, string parameterName)
    {
        if (value.Length > maxLength)
        {
            throw new DomainException($"{parameterName} no puede superar {maxLength} caracteres.");
        }

        return value;
    }

    public static string AgainstLength(string value, int minLength, int maxLength, string parameterName)
    {
        if (value.Length < minLength || value.Length > maxLength)
        {
            throw new DomainException($"{parameterName} debe tener entre {minLength} y {maxLength} caracteres.");
        }

        return value;
    }

    public static int AgainstNonPositive(int value, string parameterName)
    {
        if (value <= 0)
        {
            throw new DomainException($"{parameterName} debe ser mayor que cero.");
        }

        return value;
    }

    public static long AgainstNonPositive(long value, string parameterName)
    {
        if (value <= 0)
        {
            throw new DomainException($"{parameterName} debe ser mayor que cero.");
        }

        return value;
    }

    public static short AgainstNonPositive(short value, string parameterName)
    {
        if (value <= 0)
        {
            throw new DomainException($"{parameterName} debe ser mayor que cero.");
        }

        return value;
    }

    public static decimal AgainstNonPositive(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new DomainException($"{parameterName} debe ser mayor que cero.");
        }

        return value;
    }

    public static short AgainstNegative(short value, string parameterName)
    {
        if (value < 0)
        {
            throw new DomainException($"{parameterName} no puede ser negativo.");
        }

        return value;
    }

    public static int AgainstNegative(int value, string parameterName)
    {
        if (value < 0)
        {
            throw new DomainException($"{parameterName} no puede ser negativo.");
        }

        return value;
    }

    public static long AgainstNegative(long value, string parameterName)
    {
        if (value < 0)
        {
            throw new DomainException($"{parameterName} no puede ser negativo.");
        }

        return value;
    }

    public static DateTimeOffset AgainstDefault(DateTimeOffset value, string parameterName)
    {
        if (value == default)
        {
            throw new DomainException($"{parameterName} debe tener una fecha valida.");
        }

        return value;
    }
}
