using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly record struct Money
{
    public const decimal MaxAmount = 9999999999.99m;

    public Money(decimal amount)
    {
        amount = Guard.AgainstNonPositive(amount, nameof(amount));

        if (amount > MaxAmount)
        {
            throw new DomainException($"El valor monetario no puede superar {MaxAmount}.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new DomainException("El valor monetario solo puede tener dos decimales.");
        }

        Amount = amount;
    }

    public decimal Amount { get; }

    public Money Multiply(int multiplier)
    {
        Guard.AgainstNonPositive(multiplier, nameof(multiplier));
        return new Money(Amount * multiplier);
    }
}
