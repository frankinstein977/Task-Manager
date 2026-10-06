namespace TodoTasks.Application.Exceptions;

/// <summary>
/// Thrown when a domain business rule is violated during an operation.
/// </summary>
public class BusinessRuleViolationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleViolationException"/> class.
    /// </summary>
    /// <param name="message">A description of the violated rule.</param>
    public BusinessRuleViolationException(string message)
        : base(message)
    {
    }
}
