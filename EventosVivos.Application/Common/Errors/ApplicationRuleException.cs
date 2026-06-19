namespace EventosVivos.Application.Common.Errors;

public class ApplicationRuleException : Exception
{
    public ApplicationRuleException(string message)
        : base(message)
    {
    }
}
