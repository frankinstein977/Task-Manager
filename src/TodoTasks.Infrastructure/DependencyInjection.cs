using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoTasks.Domain.Interfaces;
using TodoTasks.Infrastructure.Persistence;
using TodoTasks.Infrastructure.Repositories;

namespace TodoTasks.Infrastructure;

/// <summary>
/// Provides dependency-injection registration helpers for the Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    private const string ConnectionStringName = "TodoTasksDatabase";

    /// <summary>
    /// Registers the PostgreSQL-backed persistence stack (DbContext, repositories,
    /// Unit of Work) with the supplied service collection.
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="configuration">The application configuration containing the connection string.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the connection string is missing from configuration.
    /// </exception>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured.");

        services.AddDbContext<TodoTasksDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(TodoTasksDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
