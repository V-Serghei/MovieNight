using System.Net.Sockets;
using Messages.Infrastructure;              
using Messages.Infrastructure.Data;         
using Microsoft.EntityFrameworkCore;
using Messages.API.Endpoints;
using Microsoft.Data.SqlClient;
//using Messages.Infrastructure.Data.Migrations;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMessagesInfrastructure(builder.Configuration);

var app = builder.Build();
static bool IsTransient(Exception ex)
{
    if (ex is SqlException) return true;

    for (var e = ex; e != null; e = e.InnerException)
        if (e is SocketException) return true;

    return false;
}

static async Task MigrateWithRetryAsync(DbContext db, ILogger logger, CancellationToken ct)
{
    var delay = TimeSpan.FromSeconds(2);

    for (var attempt = 1; attempt <= 30; attempt++)
    {
        try
        {
            logger.LogInformation("DB migrate attempt {Attempt}/30", attempt);
            await db.Database.MigrateAsync(ct);
            logger.LogInformation("DB migrated successfully");
            return;
        }
        catch (Exception ex) when (IsTransient(ex))
        {
            logger.LogWarning(ex, "Transient DB connect error. Retry in {Delay}s", delay.TotalSeconds);
            await Task.Delay(delay, ct);
            delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 15));
        }
    }

    throw new Exception("DB migration failed after retries.");
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MessagesDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    await MigrateWithRetryAsync(db, logger, CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
        o.Title = "Messages API";
        o.WithTheme(ScalarTheme.Mars);
    }).WithDisplayName("API Docs");
}

app.MapMessagesEndpoints();

app.Run();