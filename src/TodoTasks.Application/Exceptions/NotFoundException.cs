namespace TodoTasks.Application.Exceptions;

/// <summary>
/// Thrown when a requested entity cannot be located in the data store.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">The name of the entity type that was not found.</param>
    /// <param name="key">The key that was used for the lookup.</param>
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    /// <summary>
    /// Gets the name of the entity type that was not found.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the key that was used for the lookup.
    /// </summary>
    public object Key { get; }
}
