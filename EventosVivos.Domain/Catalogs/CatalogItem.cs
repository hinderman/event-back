using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Catalogs;

public abstract class CatalogItem<TId> : Entity<TId>
{
    protected CatalogItem()
    {
        Code = default;
        Name = string.Empty;
    }

    protected CatalogItem(TId id, CatalogCode code, string name)
        : base(id)
    {
        Code = code;
        Name = Guard.AgainstLength(Guard.AgainstBlank(name, nameof(name)), 60, nameof(name));
    }

    public CatalogCode Code { get; private set; }

    public string Name { get; private set; }
}
