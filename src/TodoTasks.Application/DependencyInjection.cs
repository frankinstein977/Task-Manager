using Microsoft.Extensions.DependencyInjection;
using TodoTasks.Application.Services;

namespace TodoTasks.Application;

/// <summary>
/// Provides dependency-injection registration helpers for the Application layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services with the supplied service collection.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ITodoTaskService, TodoTaskService>();

        return services;
    }
}
