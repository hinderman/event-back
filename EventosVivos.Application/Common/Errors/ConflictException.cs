namespace EventosVivos.Application.Common.Errors;

public sealed class ConflictException : ApplicationRuleException
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
