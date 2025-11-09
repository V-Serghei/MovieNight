using MoviePlayer.Infrastructure;              
using MoviePlayer.Infrastructure.Data;         
using Microsoft.EntityFrameworkCore;
using MoviePlayer.API.Endpoints;
using MoviePlayer.Infrastructure.Data.Migrations;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMoviePlayerInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MoviePlayerDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
        o.Title = "Movies API";
        o.WithTheme(ScalarTheme.Mars);
    }).WithDisplayName("API Docs");
}

app.MapMoviesEndpoints();

app.Run();