using Access.API.Endpoints;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Access.Infrastructure;
using Access.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddAccessInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AccessDbContext>();
    await db.Database.MigrateAsync();
    await Access.Infrastructure.Seed.AccessSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.Title = "Access API");
}

app.MapAccessEndpoints();

app.Run();