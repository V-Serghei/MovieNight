using Auth.Infrastructure;              // AddAuthInfrastructure
using Auth.Infrastructure.Data;         // AuthDbContext
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Auth.API.Endpoints;               // MapAuthEndpoints
using Auth.Core.Security;               // JwtTokenService

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddAuthInfrastructure(builder.Configuration);

builder.Services.AddSingleton<JwtTokenService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
        o.Title = "Auth API";
        o.WithTheme(ScalarTheme.Mars);
    }).WithDisplayName("API Docs");
}

app.MapAuthEndpoints();

app.Run();