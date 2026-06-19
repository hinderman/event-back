namespace EventosVivos.Application.Common.Errors;

public sealed class ForbiddenAccessException : ApplicationRuleException
{
    public ForbiddenAccessException(string message)
        : base(message)
    {
    }
}
