namespace EventosVivos.Application.Common.Errors;

public sealed class NotFoundException : ApplicationRuleException
{
    public NotFoundException(string resourceName, object key)
        : base($"{resourceName} con identificador '{key}' no existe.")
    {
        ResourceName = resourceName;
        Key = key;
    }

    public string ResourceName { get; }

    public object Key { get; }
}
