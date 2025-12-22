using Access.API.Endpoints;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Access.Infrastructure;
using Access.Infrastructure.Data;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddAccessInfrastructure(builder.Configuration);

var app = builder.Build();

static bool IsTransient(Exception ex)
{
    if (ex is SqlException) return true;

    for (var e = ex; e != null; e = e.InnerException)
        if (e is SocketException) return true;

    return false;
}

static async Task MigrateAndSeedWithRetryAsync(
    AccessDbContext db,
    ILogger logger,
    CancellationToken ct)
{
    var delay = TimeSpan.FromSeconds(2);

    for (var attempt = 1; attempt <= 30; attempt++)
    {
        try
        {
            logger.LogInformation("DB migrate attempt {Attempt}/30", attempt);

            await db.Database.MigrateAsync(ct);
            await Access.Infrastructure.Seed.AccessSeeder.SeedAsync(db);

            logger.LogInformation("DB migrated & seeded successfully");
            return;
        }
        catch (Exception ex) when (IsTransient(ex))
        {
            logger.LogWarning(ex, "Transient DB connect error. Retry in {Delay}s", delay.TotalSeconds);
            await Task.Delay(delay, ct);
            delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 15));
        }
    }

    throw new Exception("DB migrate/seed failed after retries.");
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccessDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    await MigrateAndSeedWithRetryAsync(db, logger, CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.Title = "Access API");
}

app.MapAccessEndpoints();
app.Run();