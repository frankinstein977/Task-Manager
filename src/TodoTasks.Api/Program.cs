using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using TodoTasks.Api.Middleware;
using TodoTasks.Application;
using TodoTasks.Infrastructure;
using TodoTasks.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Clean Architecture layer registration (composition root).
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

// Apply pending migrations automatically on startup (sample convenience).
using (IServiceScope scope = app.Services.CreateScope())
{
    TodoTasksDbContext dbContext = scope.ServiceProvider.GetRequiredService<TodoTasksDbContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
