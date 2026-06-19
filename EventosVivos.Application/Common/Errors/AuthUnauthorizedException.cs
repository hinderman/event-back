namespace EventosVivos.Application.Common.Errors;

public sealed class AuthUnauthorizedException : ApplicationRuleException
{
    public AuthUnauthorizedException(string message)
        : base(message)
    {
    }
}
