using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Catalogs;

public sealed class EventType : CatalogItem<EventTypeId>
{
    public const string ConferenceCode = "conferencia";
    public const string WorkshopCode = "taller";
    public const string ConcertCode = "concierto";

    private EventType()
    {
    }

    public EventType(EventTypeId id, CatalogCode code, string name)
        : base(id, code, name)
    {
    }
}
