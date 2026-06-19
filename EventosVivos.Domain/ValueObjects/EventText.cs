using EventosVivos.Domain.Primitives;

namespace EventosVivos.Domain.ValueObjects;

public readonly record struct EventText
{
    public EventText(string title, string description)
    {
        title = Guard.AgainstBlank(title, nameof(title));
        description = Guard.AgainstBlank(description, nameof(description));

        Title = Guard.AgainstLength(title, 5, 100, nameof(title));
        Description = Guard.AgainstLength(description, 10, 500, nameof(description));
    }

    public string Title { get; }

    public string Description { get; }
}
