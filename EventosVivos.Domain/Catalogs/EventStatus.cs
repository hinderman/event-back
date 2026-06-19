using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Catalogs;

public sealed class EventStatus : CatalogItem<EventStatusId>
{
    public const string ActiveCode = "activo";
    public const string CanceledCode = "cancelado";
    public const string CompletedCode = "completado";

    private EventStatus()
    {
    }

    public EventStatus(EventStatusId id, CatalogCode code, string name)
        : base(id, code, name)
    {
    }
}
