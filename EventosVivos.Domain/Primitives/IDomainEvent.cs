namespace EventosVivos.Domain.Primitives;

public interface IDomainEvent
{
    DateTimeOffset OccurredOnUtc { get; }
}
